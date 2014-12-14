using JetBrains.Annotations;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Monads;

namespace Common.DAL.EF
{
    /// <summary>
    /// Фабрика DbContext
    /// </summary>
    public class DbContextFactory: IDbContextFactory<DbContext>
    {
        private readonly string _connectionString;
        private readonly Func<string, DbContext> _creator;

        public DbContextFactory([NotNull] string connectionString, [NotNull] Func<string, DbContext> creator)
        {
            _connectionString = connectionString.CheckNull("connectionString");
            _creator = creator.CheckNull("creator");
        }

        public DbContext Create()
        {
            return _creator(_connectionString);
        }
    }
}
