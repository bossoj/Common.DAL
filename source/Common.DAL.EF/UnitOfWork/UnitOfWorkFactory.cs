using Common.DAL.Interface;
using JetBrains.Annotations;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Monads;
using System.Transactions;

namespace Common.DAL.EF
{
    /// <summary>
    /// Фабрика UnitOfWork
    /// </summary>
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {
        private readonly IDbContextProvider _dbContextProvider;
        private readonly IDbContextFactory<DbContext> _dbContextFactory;

        /// <summary> ctor </summary>
        /// <param name="dbContextProvider">Провайдер сессии EntityFramework</param>
        /// <param name="dbContextFactory">Фабрика создания сессии EntityFramework</param>
        public UnitOfWorkFactory([NotNull] IDbContextFactory<DbContext> dbContextFactory, [NotNull] IDbContextProvider dbContextProvider)
        {
            _dbContextFactory = dbContextFactory.CheckNull("dbContextFactory");
            _dbContextProvider = dbContextProvider.CheckNull("dbContextProvider");
        }

        public IUnitOfWork Create()
        {
            return Create(IsolationLevel.Serializable);
        }

        public IUnitOfWork Create(IsolationLevel isolationLevel)
        {
            return new UnitOfWork(_dbContextProvider, _dbContextFactory.Create(), isolationLevel);
        }

        public IUnitOfWorkAsync CreateAsync()
        {
            return CreateAsync(IsolationLevel.Serializable);
        }

        public IUnitOfWorkAsync CreateAsync(IsolationLevel isolationLevel)
        {
            return new UnitOfWorkAsync(_dbContextFactory, isolationLevel);
        }
    }
}