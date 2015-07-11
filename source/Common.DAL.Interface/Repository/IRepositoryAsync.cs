using Common.Entity;
using JetBrains.Annotations;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Common.DAL.Interface
{
    /// <summary>
    /// Интерфейс асинхронного репозитория для сущности типа {TEntity}
    /// </summary>
    /// <typeparam name="TEntity">Cущность доменной модели</typeparam>
    public interface IRepositoryAsync<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Асинхронно получить все сущности
        /// </summary>
        /// <param name="noTracking">Возвращаемые сущности не привязаны к сессии</param>
        /// <returns>Задача, возвращающая все сущности</returns>
        [NotNull]
        Task<List<TEntity>> GetAllAsync(bool noTracking = false);

        /// <summary>
        /// Асинхронно найти сущность по ключу (или составному ключу).
        /// </summary>
        /// <param name="id">Ключ (или составной ключ)</param>
        /// <returns>Задача, возвращающая найденную сущность или null если сущность не найдена</returns>
        [NotNull]
        Task<TEntity> FindAsync( params object[] id);

        /// <summary>
        /// Асинхронно добавить заданную коллекцию сущностей к контексту
        /// </summary>
        /// <param name="entities">Коллекция сущностей для добавления</param>
        /// <returns>Задача, возвращающая количество измененных/сохраненных объектов</returns>
        Task<int> AddRangeAsync([NotNull] IEnumerable<TEntity> entities);

        /// <summary>
        /// Асинхронно добавить заданную коллекцию сущностей к контексту. 
        /// Если сущность существует, то обновить сущность в контексте
        /// </summary>
        /// <param name="entities">Коллекция сущностей для добавления/обновления</param>
        /// <returns>Задача, возвращающая количество добавленных/измененных объектов</returns>
        [NotNull]
        Task<int> AddOrUpdateAsync([NotNull] TEntity[] entities);

        /// <summary>
        /// Добавить заданную коллекцию сущностей к контексту. 
        /// Если сущность существует, то обновить сущность в контексте
        /// </summary>
        /// <param name="entities">Коллекция сущностей для добавления/обновления</param>
        /// <param name="identifier">Выражение, определяющее свойства, которые должны быть использованы при определении 
        /// надо ли провести операцию добавления или обновления.</param>
        /// <returns>Задача, возвращающая количество добавленных/измененных объектов</returns>
        [NotNull]
        Task<int> AddOrUpdateAsync([NotNull] TEntity[] entities, Expression<Func<TEntity, object>> identifier);

        /// <summary>
        /// Асинхронно удалить сущности по условию в контексте
        /// </summary>
        /// <param name="filter">Условие отбора сущностей для удаления</param>
        /// <returns>Задача, удаляющая сущности по условию, возвращающая количество удаленных объектов</returns>
        [NotNull]
        Task<int> DeleteAllAsync([NotNull] Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Асинхронно получить количество всех (или по условию) элементов в коллекции
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <returns>Задача, возвращающая количество элементов в коллекции</returns>
        [NotNull]
        Task<long> CountAsync([CanBeNull] Expression<Func<TEntity, bool>> filter = null);

        /// <summary>
        /// Асинхронный запрос на получение сущностей из контекста
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="noTracking">Возвращаемые сущности не привязаны к сессии</param>
        /// <param name="orderBy">Условие сортировки</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Задача, возвращающая найденные сущности</returns>
        [NotNull]
        Task<IList<TEntity>> QueryAsync(
            [CanBeNull] Expression<Func<TEntity, bool>> filter,
            [CanBeNull] Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool noTracking = false,
            params Expression<Func<TEntity, object>>[] include);

        /// <summary>
        /// Асинхронный запрос на получение сущностей из контекста
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="orderBy">Условие сортировки</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Задача, возвращающая найденные сущности</returns>
        [NotNull]
        Task<IList<TEntity>> QueryAsync(
            [CanBeNull] Expression<Func<TEntity, bool>> filter,
            [CanBeNull] Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] include);

        /// <summary>
        /// Асинхронный запрос на получение сущностей из контекста
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="noTracking">Возвращаемые сущности не привязаны к сессии</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Задача, возвращающая найденные сущности</returns>
        [NotNull]
        Task<IList<TEntity>> QueryAsync(
            [CanBeNull] Expression<Func<TEntity, bool>> filter,
            bool noTracking = false,
            params Expression<Func<TEntity, object>>[] include);

        /// <summary>
        /// Асинхронный запрос на получение сущностей из контекста
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Задача, возвращающая найденные сущности</returns>
        [NotNull]
        Task<IList<TEntity>> QueryAsync(
            [CanBeNull] Expression<Func<TEntity, bool>> filter,
            params Expression<Func<TEntity, object>>[] include);

        /// <summary>
        /// Асинхронный запрос на получение сущности из контекста
        /// </summary>
        /// <param name="callback">Условие отбора сущности</param>
        /// <param name="noTracking">Возвращаемая сущность не привязаны к сессии</param>
        /// <returns>Задача, возвращающая найденную сущность</returns>
        [NotNull]
        Task<TEntity> QueryAsync([NotNull] Func<IQueryable<TEntity>, Task<TEntity>> callback, bool noTracking = false);

        /// <summary>
        /// Асинхронный запрос на получение проекции сущности из контекста
        /// </summary>
        /// <param name="callback">Условие отбора проекции</param>
        /// <param name="noTracking">Возвращаемая сущность не привязаны к сессии</param>
        /// <returns>Задача, возвращающая найденную проекцию</returns>
        [NotNull]
        Task<TResult> QueryAsync<TResult>([NotNull] Func<IQueryable<TEntity>, Task<TResult>> callback, bool noTracking = false);

        /// <summary>
        /// Асинхронный запрос на получение постраничного вывода сущностей из контекста.
        /// Работает только с заданным условием сортировки
        /// </summary>
        /// <param name="pageNumber">Номер страницы (начинается с 1)</param>
        /// <param name="pageSize">Количество записей на странице (минимальное 1)</param>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="orderBy">Условие сортировки</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Найденные сущности</returns>
        [NotNull]
        Task<IPagedList<TEntity>> PagedAsync( int pageNumber, int pageSize,
            [CanBeNull] Expression<Func<TEntity, bool>> filter,
            [NotNull] Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            params Expression<Func<TEntity, object>>[] include);

        #region Methods immediately executed, pass by tracking system

        /// <summary>
        /// Асинхронно удалить сущности по условию в контексте.
        /// Метод выполняется немедленно, минуя систему трекинга.
        /// Изменения не будут отражаться на сущностях в текущем контексте.
        /// </summary>
        /// <param name="filter">Условие отбора сущностей для удаления</param>
        /// <returns>Задача, возвращающая количество удаленных объектов</returns>
        Task<int> DeleteImmediatelyAsync([NotNull] Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Асинхронно обновить сущности по условию в контексте.
        /// Метод выполняется немедленно, минуя систему трекинга.
        /// Изменения не будут отражаться на сущностях в текущем контексте.
        /// </summary>
        /// <param name="filter">Условие отбора сущностей для обновления</param>
        /// <param name="updater">Выражение указывает, какие поля необходимо обновить</param>
        /// <returns>Задача, возвращающая количество обновленных объектов</returns>
        Task<int> UpdateImmediatelyAsync([NotNull] Expression<Func<TEntity, bool>> filter, [NotNull] Expression<Func<TEntity, TEntity>> updater);

        #endregion
    }
}
