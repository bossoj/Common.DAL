using Common.DAL.Interface;
using JetBrains.Annotations;
using System.Data;
using System.Data.Entity;
using System.Monads;

namespace Common.DAL.EF
{
    /// <summary>
    /// Единица работы
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDbContextProvider _dbContextProvider;
        private readonly DbContext _dbContext;
        private readonly DbContextTransaction _transaction;
        private readonly TransactionOption _scopeOption;
        private readonly bool _isNested;

        private bool _wasCommitted = false;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbContextProvider">Провайдер сессии EntityFramework</param>
        /// <param name="dbContext">Сессия EntityFramework</param>
        /// <param name="scopeOption">Требование к транзакции</param>
        /// <param name="isolationLevel">Уровень изоляции (задает поведение при блокировке транзакции для подключения)</param>
        /// <param name="isNested">Признак того, является ли еденица работы вложенная в другую</param>
        public UnitOfWork([NotNull] IDbContextProvider dbContextProvider, [NotNull] DbContext dbContext, 
            TransactionOption scopeOption = TransactionOption.New, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
            bool isNested = false)
        {
            _dbContextProvider = dbContextProvider.CheckNull("dbContextProvider");
            _dbContext = dbContext.CheckNull("dbContext");
            _isNested = isNested;

            _scopeOption = scopeOption;

            if (!_isNested)
            {
                _dbContextProvider.CurrentDbContext = _dbContext;

                if (_scopeOption == TransactionOption.New)
                    _transaction = _dbContext.Database.BeginTransaction(isolationLevel);
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

            if (_isNested)
            {
                ExceptionWrapper.WrapCall(() =>
                {
                    _dbContext.SaveChanges();
                });

            }
            else
            {
                ExceptionWrapper.WrapCall(() =>
                {
                    _dbContext.SaveChanges();

                    if (_scopeOption == TransactionOption.New)
                        _transaction.Commit();
                });

                _dbContextProvider.CurrentDbContext = null;
            }

            _wasCommitted = true;
        }

        /// <summary>
        /// Откатить ВСЕ изменения в базе
        /// </summary>
        private void Rollback()
        {
            if (!_wasCommitted)
            {
                ExceptionWrapper.WrapCall(() =>
                {
                    if (_scopeOption == TransactionOption.New)
                        _transaction.Rollback();
                });  
            }
        }

        public void Dispose()
        {
            if (!_isNested)
            {
                Rollback();

                if (_scopeOption == TransactionOption.New)
                    _transaction.Dispose();

                _dbContextProvider.CurrentDbContext = null;
                _dbContext.Dispose();

                // Обнулим хранилище родительского UnitOfWork
                UnitOfWorkFactory.CurrentParentUnitOfWork = null;
            }
        }
    }
}
