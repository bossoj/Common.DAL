using Common.Entity;
using JetBrains.Annotations;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Common.DAL.Interface
{
    /// <summary>
    /// Интерфейс репозитория
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Тип сущности, с которой работает данный репозиторий
        /// </summary>
        [NotNull]
        Type ObjectType { get; }
    }

    /// <summary>
    /// Интерфейс репозитория для сущности типа {TEntity}
    /// </summary>
    /// <typeparam name="TEntity">Cущность доменной модели</typeparam>
    public interface IRepository<TEntity> : IRepository
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Получить все сущности
        /// </summary>
        /// <param name="noTracking">Возвращаемые сущности не привязаны к сессии</param>
        /// <returns>Все сущности</returns>
        [NotNull]
        IList<TEntity> GetAll(bool noTracking = false);

        /// <summary>
        /// Найти сущность по ключу (или составному ключу)
        /// </summary>
        /// <param name="id">Ключ (или составной ключ)</param>
        /// <returns>Найденная сущность или null если сущность не найдена</returns>
        [CanBeNull]
        TEntity Find(params object[] id);

        /// <summary>
        /// Добавить сущность к контексту
        /// </summary>
        /// <param name="entity">Добавляемая сущность</param>
        /// <returns>Количество добавленных объектов</returns>
        int Add([NotNull] TEntity entity);

        /// <summary>
        /// Добавить заданную коллекцию сущностей к контексту
        /// </summary>
        /// <param name="entities">Коллекция сущностей для добавления</param>
        /// <returns>Количество добавленных объектов</returns>
        int AddRange([NotNull] IEnumerable<TEntity> entities);

        /// <summary>
        /// Добавить сущность к контексту.
        /// Если сущность существует, то обновить сущность в контексте
        /// </summary>
        /// <param name="entity">Добавляемая/обновляемая сущность</param>
        /// <returns>Количество измененных/сохраненных объектов</returns>
        int AddOrUpdate([NotNull] TEntity entity);

        /// <summary>
        /// Добавить заданную коллекцию сущностей к контексту. 
        /// Если сущность существует, то обновить сущность в контексте
        /// </summary>
        /// <param name="entities">Коллекция сущностей для добавления/обновления</param>
        /// <returns>Количество измененных/сохраненных объектов</returns>
        int AddOrUpdate([NotNull] TEntity[] entities);

        /// <summary>
        /// Добавить сущность к контексту.
        /// Если сущность существует, то обновить сущность в контексте
        /// </summary>
        /// <param name="entity">Добавляемая/обновляемая сущность</param>
        /// <param name="identifier">Выражение, определяющее свойства, которые должны быть использованы при определении 
        /// надо ли провести операцию добавления или обновления.</param>
        /// <returns>Количество измененных/сохраненных объектов</returns>
        int AddOrUpdate([NotNull] TEntity entity, Expression<Func<TEntity, object>> identifier);

        /// <summary>
        /// Добавить заданную коллекцию сущностей к контексту. 
        /// Если сущность существует, то обновить сущность в контексте
        /// </summary>
        /// <param name="entities">Коллекция сущностей для добавления/обновления</param>
        /// <param name="identifier">Выражение, определяющее свойства, которые должны быть использованы при определении 
        /// надо ли провести операцию добавления или обновления.</param>
        /// <returns>Количество измененных/сохраненных объектов</returns>
        int AddOrUpdate([NotNull] TEntity[] entities, Expression<Func<TEntity, object>> identifier);

        /// <summary>
        /// Обновить сущность в контексте
        /// </summary>
        /// <param name="entity">Обновляемая сущность</param>
        /// <returns>Количество измененных/сохраненных объектов</returns>
        int Update([NotNull] TEntity entity);

        /// <summary>
        /// Удалить сущность в контексте
        /// </summary>
        /// <param name="entity">Удаляемая сущность</param>
        /// <returns>Количество удаленных объектов</returns>
        int Delete([NotNull] TEntity entity);

        /// <summary>
        /// Удалить сущность в контексте
        /// </summary>
        /// <param name="id">Идентификатор удаляемой сущности</param>
        /// <returns>Количество удаленных объектов</returns>
        int Delete(params object[] id);

        /// <summary>
        /// Удалить заданную коллекцию сущностей в контексте
        /// </summary>
        /// <param name="entities">Коллекция сущностей для удаления</param>
        /// <returns>Количество удаленных объектов</returns>
        int DeleteRange([NotNull] IEnumerable<TEntity> entities);

        /// <summary>
        /// Удалить сущности по условию в контексте
        /// </summary>
        /// <param name="filter">Условие отбора сущностей для удаления</param>
        /// <returns>Количество удаленных объектов</returns>
        int DeleteAll([NotNull] Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Получить количество всех (или по условию) элементов в коллекции
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <returns>Количество элементов в коллекции</returns>
        long Count([CanBeNull] Expression<Func<TEntity, bool>> filter = null);

        /// <summary>
        /// Запрос на получение сущностей из контекста
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="noTracking">Возвращаемые сущности не привязаны к сессии</param>
        /// <param name="orderBy">Условие сортировки</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Найденные сущности</returns>
        [NotNull]
        IList<TEntity> Query(
            [CanBeNull] Expression<Func<TEntity, bool>> filter,            
            [CanBeNull] Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool noTracking = false,
            params Expression<Func<TEntity, object>>[] include);

        /// <summary>
        /// Запрос на получение сущностей из контекста
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="orderBy">Условие сортировки</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Найденные сущности</returns>
        [NotNull]
        IList<TEntity> Query(
            [CanBeNull] Expression<Func<TEntity, bool>> filter,
            [CanBeNull] Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            params Expression<Func<TEntity, object>>[] include);

        /// <summary>
        /// Запрос на получение сущностей из контекста
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="noTracking">Возвращаемые сущности не привязаны к сессии</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Найденные сущности</returns>
        [NotNull]
        IList<TEntity> Query(
            [CanBeNull] Expression<Func<TEntity, bool>> filter,
            bool noTracking = false,
            params Expression<Func<TEntity, object>>[] include);

        /// <summary>
        /// Запрос на получение сущностей из контекста
        /// </summary>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Найденные сущности</returns>
        [NotNull]
        IList<TEntity> Query(
            [CanBeNull] Expression<Func<TEntity, bool>> filter,
            params Expression<Func<TEntity, object>>[] include);

        /// <summary>
        /// Запрос на получение сущностей из контекста
        /// </summary>
        /// <param name="callback">Условие отбора сущности</param>
        /// <param name="noTracking">Возвращаемая сущность не привязаны к сессии</param>
        /// <returns>Найденная сущность</returns>
        [NotNull]
        TEntity Query([NotNull] Func<IQueryable<TEntity>, TEntity> callback, bool noTracking = false);


        /// <summary>
        /// Запрос на получение проекции сущности из контекста
        /// </summary>
        /// <param name="callback">Условие отбора проекции</param>
        /// <param name="noTracking">Возвращаемая сущность не привязаны к сессии</param>
        /// <returns>Найденная проекция</returns>
        [NotNull]
        TResult Query<TResult>([NotNull] Func<IQueryable<TEntity>, TResult> callback, bool noTracking = false);

        /// <summary>
        /// Запрос на получение постраничного вывода сущностей из контекста
        /// Работает только с заданным условием сортировки
        /// </summary>
        /// <param name="pageNumber">Номер страницы (начинается с 1)</param>
        /// <param name="pageSize">Количество записей на странице (минимальное 1)</param>
        /// <param name="filter">Условие отбора сущностей</param>
        /// <param name="orderBy">Условие сортировки</param>
        /// <param name="include">Сущности включаемые в результат запроса</param>
        /// <returns>Найденные сущности</returns>
        [NotNull]
        IPagedList<TEntity> Paged(int pageNumber, int pageSize,
            [CanBeNull] Expression<Func<TEntity, bool>> filter,
            [NotNull] Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            params Expression<Func<TEntity, object>>[] include);

        #region Methods immediately executed, pass by tracking system

        /// <summary>
        /// Удалить сущности по условию в контексте.
        /// Метод выполняется немедленно, минуя систему трекинга.
        /// Изменения не будут отражаться на сущностях в текущем контексте.
        /// </summary>
        /// <param name="filter">Условие отбора сущностей для удаления</param>
        /// <returns>Количество удаленных объектов</returns>
        int DeleteImmediately([NotNull] Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Обновить сущности по условию в контексте.
        /// Метод выполняется немедленно, минуя систему трекинга.
        /// Изменения не будут отражаться на сущностях в текущем контексте.
        /// </summary>
        /// <param name="filter">Условие отбора сущностей для обновления</param>
        /// <param name="updater">Выражение указывает, какие поля необходимо обновить</param>
        /// <returns>Количество обновленных объектов</returns>
        int UpdateImmediately([NotNull] Expression<Func<TEntity, bool>> filter, [NotNull] Expression<Func<TEntity, TEntity>> updater);

        #endregion
    }
}
