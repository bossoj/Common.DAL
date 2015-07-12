using Common.DAL.Interface;
using Common.Entity;
using JetBrains.Annotations;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Monads;
using System.Threading.Tasks;
using System.Transactions;

namespace Common.DAL.EF
{
    /// <summary>
    /// Единица работы
    /// </summary>
    public class UnitOfWorkAsync : IUnitOfWorkAsync
    {
        private readonly IDbContextFactory<DbContext> _dbContextFactory;
        private readonly IDbContextProvider _dbContextProvider;
        private readonly DbContext _dbContext;
        private readonly TransactionScope _transaction;

        private bool _wasCommitted = false;
        private bool _hasTransaction = false;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbContextFactory">Провайдер сессии EntityFramework</param>
        /// <param name="isolationLevel">Уровень изоляции (задает поведение при блокировке транзакции для подключения)</param>
        public UnitOfWorkAsync([NotNull] IDbContextFactory<DbContext> dbContextFactory, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            _dbContextFactory = dbContextFactory.CheckNull("dbContextFactory");
            _dbContext = _dbContextFactory.Create();

            _dbContextProvider = new TransientDbContextProvider { CurrentDbContext = _dbContext };

            if (Transaction.Current == null)
            {
                _transaction = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions {IsolationLevel = isolationLevel}, TransactionScopeAsyncFlowOption.Enabled);

                _hasTransaction = true;
            }
        }

        /// <summary>
        /// Асинхронно cохранить ВСЕ изменения в базу
        /// </summary>
        public async Task CommitAsync()
        {
            if (_wasCommitted)
            {
                throw new DalEFException(
                    "Для текущей сессии уже был вызван Commit." +
                    "Пожалуйста, откройте новый сеанс через UnitOfWorkFactory.");
            }

            try
            {
                await _dbContext.SaveChangesAsync();

                if (_hasTransaction)
                {
                    _transaction.Complete();                    
                }
            }
            catch (Exception ex)
            {
                throw ExceptionWrapper.Wrap(ex);
            }

            _dbContextProvider.CurrentDbContext = null;
            _wasCommitted = true;
        }

        /// <summary>
        /// Создать репозиторий, привязанный к UnitOfWork через сессию
        /// </summary>
        public IRepositoryAsync<TEntity> CreateRepository<TEntity>() where TEntity : class, IEntity
        {
            return new RepositoryAsync<TEntity>(_dbContextProvider);
        }

        public void Dispose()
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