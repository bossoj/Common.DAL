using JetBrains.Annotations;
using System.Data.Entity;
using System.Monads;

namespace Common.DAL.EF
{
    /// <summary>
    /// Абстрактный базовый класс репозитория
    /// </summary>
    public abstract class RepositoryBase
    {
        private readonly IDbContextProvider _dbContextProvider;

        protected RepositoryBase([NotNull] IDbContextProvider dbContextProvider)
        {
            _dbContextProvider = dbContextProvider.CheckNull("dbContextProvider");
        }

        protected virtual DbContext DbContext
        {
            get { return _dbContextProvider.CurrentDbContext; }
        }
    }
}
