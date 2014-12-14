using Common.DAL.Interface;
using JetBrains.Annotations;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Monads;

namespace Common.DAL.EF
{
    /// <summary>
    /// Фабрика UnitOfWork
    /// </summary>
    public class UnitOfWorkFactory : IUnitOfWorkFactory
    {        
        /// <summary>
        /// Хранилище родительского UnitOfWork 
        /// </summary>
        [ThreadStatic]
        public static IUnitOfWork CurrentParentUnitOfWork;
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
            return Create(IsolationLevel.ReadCommitted);
        }

        public IUnitOfWork Create(IsolationLevel isolationLevel, TransactionOption transactionOption = TransactionOption.New)
        {
            // Если в хранилище нет UnitOfWork, то создаем родительский UnitOfWork
            if (CurrentParentUnitOfWork == null)
            {
                return CurrentParentUnitOfWork = new UnitOfWork(_dbContextProvider, _dbContextFactory.Create(), transactionOption, isolationLevel);
            }
            // Если в хранилище есть UnitOfWork, то создаем вложенный UnitOfWork
            else
            {
                return new UnitOfWork(_dbContextProvider, _dbContextFactory.Create(), transactionOption, isolationLevel, isNested: true);
            }              
        }

        public IUnitOfWork Create(TransactionOption transactionOption)
        {
            return Create(IsolationLevel.ReadCommitted, transactionOption);
        }


        public IUnitOfWorkAsync CreateAsync()
        {
            return CreateAsync(IsolationLevel.ReadCommitted);
        }

        public IUnitOfWorkAsync CreateAsync(IsolationLevel isolationLevel, TransactionOption transactionOption = TransactionOption.New)
        {
            return new UnitOfWorkAsync(_dbContextFactory, transactionOption, isolationLevel);
        }

        public IUnitOfWorkAsync CreateAsync(TransactionOption transactionOption)
        {
            return CreateAsync(IsolationLevel.ReadCommitted, transactionOption);
        }
    }
}
