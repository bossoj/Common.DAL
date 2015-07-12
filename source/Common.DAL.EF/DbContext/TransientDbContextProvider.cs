using System.Data.Entity;

namespace Common.DAL.EF
{
    /// <summary>
    /// Провайдер сессии EntityFramework.
    /// Хранит сессию в приватном поле
    /// </summary>
    public class TransientDbContextProvider : IDbContextProvider
    {
        private DbContext _dbContext;

        public DbContext CurrentDbContext
        {
            get
            {
                if (_dbContext != null)
                    return _dbContext;

                throw new DalEFException(
                    "Сессия не открыта. Логика доступа к базе данных не может быть использована." +
                    "Пожалуйста, откройте сеанс явно через UnitOfWorkFactory.");
            }
            set
            {
                if (value != null && _dbContext != null)
                    throw new DalEFException(
                    "Текущая сессия не закрыта. " +
                    "Пожалуйста, закройте текущий сеанс явно через UnitOfWork.Commit() или UnitOfWork.Dispose().");

                _dbContext = value;
            }
        }

        public bool IsEmpty
        {
            get { return _dbContext == null; }
        }
    }
}
