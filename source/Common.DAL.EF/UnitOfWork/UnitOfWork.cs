using Common.DAL.Interface;
using JetBrains.Annotations;
using System.Data.Entity;
using System.Monads;
using System.Transactions;

namespace Common.DAL.EF
{
    /// <summary>
    /// Единица работы
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContextProvider _dbContextProvider;
        private readonly DbContext _dbContext;
        private readonly TransactionScope _transaction;       

        private bool _wasCommitted = false;
        private bool _hasTransaction = false;
        private bool _isNested = false;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbContextProvider">Провайдер сессии EntityFramework</param>
        /// <param name="dbContext">Сессия EntityFramework</param>
        /// <param name="isolationLevel">Уровень изоляции (задает поведение при блокировке транзакции для подключения)</param>
        public UnitOfWork([NotNull] IDbContextProvider dbContextProvider, [NotNull] DbContext dbContext,
            IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            _dbContextProvider = dbContextProvider.CheckNull("dbContextProvider");
            _dbContext = dbContext.CheckNull("dbContext");

            _isNested = !_dbContextProvider.IsEmpty && Transaction.Current != null;

            if (!_isNested)
            {
                _dbContextProvider.CurrentDbContext = _dbContext;

                if (Transaction.Current == null)
                {
                    _transaction = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = isolationLevel });

                    _hasTransaction = true;
                }
            }
        }

        /// <summary>
        /// Сохранить ВСЕ изменения в базу
        /// </summary>
        public void Commit()
        {
            if (_wasCommitted)
            {
                throw new DalEFException(
                    "Для текущей сессии уже был вызван Commit." +
                    "Пожалуйста, откройте новый сеанс через UnitOfWorkFactory.");
            }

            ExceptionWrapper.WrapCall(() =>
            {
                _dbContext.SaveChanges();

                if (!_isNested)
                {
                    if (_hasTransaction)
                    {
                        _transaction.Complete();
                    }

                    _dbContextProvider.CurrentDbContext = null;
                }
            });

            _wasCommitted = true;
        }
        public void Dispose()
        {
            if (!_isNested)
            {
                if (_hasTransaction)
                {
                    _transaction.Dispose();
                }

                _dbContextProvider.CurrentDbContext = null;
                _dbContext.Dispose();
            }
        }
    }
}