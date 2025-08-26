using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace DataAccess.Generic
{
    public interface ISqlGenericRepository<TEntity, TContext> where TEntity : class
    {
        Task<bool> IsConnectedAsync();
        Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);

        Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> whereCondition = null, params Expression<Func<TEntity, object>>[] includes);

        Task<TEntity> GetByIdAsync(int id);

        Task<int?> CreateAsync(TEntity entity);

        Task<bool> UpdateByEntityAsync(TEntity entity);

        Task<bool> DeleteByIdAsync(int id);

    }
    public class SqlGenericRepository<TEntity, TContext> : ISqlGenericRepository<TEntity, TContext> where TEntity : class where TContext : DbContext
    {
        private readonly ISqlUnitOfWork<TContext> _sqlUnitOfWork;

        public SqlGenericRepository(ISqlUnitOfWork<TContext> sqlUnitOfWork)
        {
            _sqlUnitOfWork = sqlUnitOfWork;
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                var result = await _sqlUnitOfWork.Context.Set<TEntity>().AnyAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en IsConnectedAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
        {
            try
            {
                IQueryable<TEntity> query = _sqlUnitOfWork.Context.Set<TEntity>();
                if (includes != null)
                {
                    foreach (var include in includes)
                    {
                        query = query.Include(include);
                    }
                }
                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        public async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> whereCondition = null, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query =_sqlUnitOfWork.Context.Set<TEntity>();
            if (includes != null)
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }
            if (whereCondition != null)
            {
                query = query.Where(whereCondition);
            }
            return await query.ToListAsync();
        }

        public async Task<int?> CreateAsync(TEntity entity)
        {
            try
            {
                await _sqlUnitOfWork.Context.Set<TEntity>().AddAsync(entity);
                _sqlUnitOfWork.Commit();
                _sqlUnitOfWork.Context.Entry(entity).Reload();
                return Convert.ToInt32(entity.GetType().GetProperty("Id").GetValue(entity));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<TEntity> GetByIdAsync(int id)
        {
            return await _sqlUnitOfWork.Context.Set<TEntity>().FindAsync(id);
        }

        public async Task<bool> DeleteByIdAsync(int id)
        {
            try
            {
                TEntity entity = await GetByIdAsync(id);
                if (entity != null)
                {
                    _sqlUnitOfWork.Context.Set<TEntity>().Remove(entity);
                    _sqlUnitOfWork.Commit();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateByEntityAsync(TEntity updatedEntity)
        {
            try
            {
                TEntity? existingEntity = await GetByIdAsync(Convert.ToInt32(typeof(TEntity).GetProperty("Id").GetValue(updatedEntity)));
                if (existingEntity == null)
                {
                    return false;
                }
                else
                {
                    _sqlUnitOfWork.Context.Entry(existingEntity).CurrentValues.SetValues(updatedEntity);
                    _sqlUnitOfWork.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al actualizar la entidad.", ex);
            }
        }
    }
}
