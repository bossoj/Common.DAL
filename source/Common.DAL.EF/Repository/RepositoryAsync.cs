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
using System.Threading.Tasks;

namespace Common.DAL.EF
{
    /// <summary>
    /// Класс асинхронного репозитория для сущности типа {TEntity}
    /// </summary>
    /// <typeparam name="TEntity">Cущность доменной модели</typeparam>
    public class RepositoryAsync<TEntity> : Repository<TEntity>, IRepositoryAsync<TEntity>
        where TEntity : class, IEntity
    {
        public RepositoryAsync(IDbContextProvider dbContextProvider)
            : base(dbContextProvider)
        {
        }

        #region IRepositoryAsync<TEntity> Members

        public async Task<List<TEntity>> GetAllAsync(bool noTracking = false)
        {
            if (!noTracking)
            {
                return await DbSet.ToListAsync();
            }
            else
            {
                return await DbSet.AsNoTracking().ToListAsync();
            }
        }

        public async Task<TEntity> FindAsync(params object[] id)
        {
            return await DbSet.FindAsync(id);
        }

        public Task<int> AddRangeAsync(IEnumerable<TEntity> entities)
        {
            DbSet.AddRange(entities);

            return DbContext.SaveChangesAsync();
        }

        public async Task<int> AddOrUpdateAsync(TEntity[] entities)
        {
            DbSet.AddOrUpdate(entities);

            return await DbContext.SaveChangesAsync(); 
        }

        public async Task<int> AddOrUpdateAsync(TEntity[] entities, Expression<Func<TEntity, object>> identifier)
        {
            DbSet.AddOrUpdate(identifier, entities);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteAllAsync(Expression<Func<TEntity, bool>> filter)
        {
            DbSet.RemoveRange(await DbSet.Where(filter).ToListAsync());

            return await DbContext.SaveChangesAsync();
        }

        public async Task<long> CountAsync(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = DbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query.LongCountAsync();
        }

        public async Task<IList<TEntity>> QueryAsync(
            Expression<Func<TEntity, bool>> filter,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
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

            if (orderBy != null)
            {
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public async Task<IList<TEntity>> QueryAsync(
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
                return await orderBy(query).ToListAsync();
            }
            else
            {
                return await query.ToListAsync();
            }
        }

        public async Task<IList<TEntity>> QueryAsync(
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

            return await query.ToListAsync();
        }

        public async Task<IList<TEntity>> QueryAsync(
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

            return await query.ToListAsync();
        }

        public async Task<TEntity> QueryAsync(Func<IQueryable<TEntity>, Task<TEntity>> callback, bool noTracking = false)
        {
            IQueryable<TEntity> query = DbSet;

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            return await callback(query);
        }

        public async Task<TResult> QueryAsync<TResult>(Func<IQueryable<TEntity>, Task<TResult>> callback, bool noTracking = false)
        {
            IQueryable<TEntity> query = DbSet;

            if (noTracking)
            {
                query = query.AsNoTracking();
            }

            return await callback(query);
        }

        public async Task<IPagedList<TEntity>> PagedAsync(int pageNumber, int pageSize, 
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

            var result = await orderBy(query).ToListAsync();

            return result.ToPagedList(pageNumber, pageSize);
        }

        #endregion

        #region Methods immediately executed, pass by tracking system

        public async Task<int> DeleteImmediatelyAsync(Expression<Func<TEntity, bool>> filter)
        {
            return await DbSet.Where(filter).DeleteAsync();
        }

        public async Task<int> UpdateImmediatelyAsync(Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, TEntity>> updater)
        {
            return await DbSet.Where(filter).UpdateAsync(updater);
        }

        #endregion
    }
}
