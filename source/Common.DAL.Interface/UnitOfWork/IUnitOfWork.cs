using System;

namespace Common.DAL.Interface
{
    /// <summary>
    /// Интерфейс единицы работы
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Сохранить ВСЕ изменения в базу
        /// </summary>
        void Commit();
    }
}
