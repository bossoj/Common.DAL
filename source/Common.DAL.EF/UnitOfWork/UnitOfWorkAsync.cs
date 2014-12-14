using Common.DAL.Interface;
using Common.Entity;
using JetBrains.Annotations;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Monads;
using System.Threading.Tasks;

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
        private readonly DbContextTransaction _transaction;
        private readonly TransactionOption _scopeOption;

        private bool _wasCommitted = false;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="dbContextFactory">Провайдер сессии EntityFramework</param>
        /// <param name="scopeOption">Требование к транзакции</param>
        /// <param name="isolationLevel">Уровень изоляции (задает поведение при блокировке транзакции для подключения)</param>
        public UnitOfWorkAsync([NotNull] IDbContextFactory<DbContext> dbContextFactory, 
            TransactionOption scopeOption = TransactionOption.New, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _dbContextFactory = dbContextFactory.CheckNull("dbContextFactory");
            _dbContext = _dbContextFactory.Create();

            _scopeOption = scopeOption;

            _dbContextProvider = new TransientDbContextProvider {CurrentDbContext = _dbContext};

            if (_scopeOption == TransactionOption.New)
                _transaction = _dbContext.Database.BeginTransaction(isolationLevel);
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

                if (_scopeOption == TransactionOption.New)
                    _transaction.Commit();
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
            Rollback();

            if (_scopeOption == TransactionOption.New)
                _transaction.Dispose();

            _dbContextProvider.CurrentDbContext = null;
            _dbContext.Dispose();
        }
    }
}
