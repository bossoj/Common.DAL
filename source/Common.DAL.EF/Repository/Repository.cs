using Common.DAL.Interface;
using Common.Entity;
using EntityFramework.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Expressions;

namespace Common.DAL.EF
{
    /// <summary>
    /// Класс репозитория для сущности типа {TEntity}
    /// </summary>
    /// <typeparam name="TEntity">Cущность доменной модели</typeparam>
    public class Repository<TEntity> : RepositoryBase, IRepository<TEntity>
        where TEntity : class, IEntity
    {
        public Repository(IDbContextProvider dbContextProvider)
            : base(dbContextProvider)
        {
        }

        protected virtual DbSet<TEntity> DbSet
        {
            get { return DbContext.Set<TEntity>(); }
        }

        #region IRepository<TEntity> Members

        public Type ObjectType
        {
            get { return typeof(TEntity); }
        }

        public IList<TEntity> GetAll(bool noTracking = false)
        {
            if (!noTracking)
            {
                return DbSet.ToList();
            }
            else
            {
                return DbSet.AsNoTracking().ToList();
            }
        }

        public TEntity Find(params object[] id)
        {
            return DbSet.Find(id);
        }

        public int Add(TEntity entity)
        {
            DbSet.Add(entity);

           return DbContext.SaveChanges();
        }

        public int AddRange(IEnumerable<TEntity> entities)
        {
            DbSet.AddRange(entities);

            return DbContext.SaveChanges();
        }

        public int AddOrUpdate(TEntity entity)
        {
            DbSet.AddOrUpdate(entity);

            return DbContext.SaveChanges(); 
        }

        public int AddOrUpdate(TEntity[] entities)
        {
            DbSet.AddOrUpdate(entities);

            return DbContext.SaveChanges(); 
        }

        public int AddOrUpdate(TEntity entity, Expression<Func<TEntity, object>> identifier)
        {
            DbSet.AddOrUpdate(identifier, entity);

            return DbContext.SaveChanges();
        }

        public int AddOrUpdate(TEntity[] entities, Expression<Func<TEntity, object>> identifier)
        {
            DbSet.AddOrUpdate(identifier, entities);

            return DbContext.SaveChanges();
        }

        public int Update(TEntity entity)
        {
            DbContext.Entry(entity).State = EntityState.Modified;

            return DbContext.SaveChanges();
        }

        public int Delete(TEntity entity)
        {
            DbSet.Attach(entity);
            DbSet.Remove(entity);

            return DbContext.SaveChanges();
        }

        public int Delete(params object[] id)
        {
            TEntity entity = Find(id);

            if (entity == null)
                return 0;

            DbSet.Remove(entity);

            return DbContext.SaveChanges();
        }

        public int DeleteRange(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                DbSet.Attach(entity);                                
            }
            
            DbSet.RemoveRange(entities);

            return DbContext.SaveChanges();
        }

        public int DeleteAll(Expression<Func<TEntity, bool>> filter)
        {
            DbSet.RemoveRange(DbSet.Where(filter).ToList());

            return DbContext.SaveChanges();
        }

        public long Count(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = DbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.LongCount();
        }

        public IList<TEntity> Query(
            Expression<Func<TEntity, bool>> filter, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool noTracking = false,
            params Expression<Func<TEntity, object>>[] include
            )
        {
            IQueryable<TEntity> query = DbSet;

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }  

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public IList<TEntity> Query(
            Expression<Func<TEntity, bool>> filter, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, 
            params Expression<Func<TEntity, object>>[] include)
        {
            IQueryable<TEntity> query = DbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public IList<TEntity> Query(
            Expression<Func<TEntity, bool>> filter, 
            bool noTracking = false, 
            params Expression<Func<TEntity, object>>[] include)
        {
            IQueryable<TEntity> query = DbSet;

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }

            return query.ToList();
        }

        public IList<TEntity> Query(
            Expression<Func<TEntity, bool>> filter, 
            params Expression<Func<TEntity, object>>[] include)
        {
            IQueryable<TEntity> query = DbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }

            return query.ToList();
        }


        public TEntity Query(Func<IQueryable<TEntity>, TEntity> callback, bool noTracking = false)
        {
            IQueryable<TEntity> query = DbSet;

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            return callback(query);
        }


        public TResult Query<TResult>(Func<IQueryable<TEntity>, TResult> callback, bool noTracking = false)
        {
            IQueryable<TEntity> query = DbSet;

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            return callback(query);
        }


        public IPagedList<TEntity> Paged(int pageNumber, int pageSize, 
            Expression<Func<TEntity, bool>> filter, 
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy,
            params Expression<Func<TEntity, object>>[] include)
        {
            IQueryable<TEntity> query = DbSet.AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            if (include != null)
            {
                foreach (var includeExpression in include)
                {
                    query = query.Include(includeExpression);
                }
            }

            return orderBy(query).ToPagedList(pageNumber, pageSize);
        }

        #endregion

        #region Methods immediately executed, pass by tracking system

        public int DeleteImmediately(Expression<Func<TEntity, bool>> filter)
        {
            return DbSet.Where(filter).Delete();
        }

        public int UpdateImmediately(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updater)
        {
            return DbSet.Where(filter).Update(updater);
        }

        #endregion
    }
}
