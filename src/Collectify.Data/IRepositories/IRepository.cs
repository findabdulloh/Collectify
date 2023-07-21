using System.Linq.Expressions;

namespace Collectify.Data.IRepositories;

public interface IRepository<TEntity>
{
    Task<TEntity> InsertAsync(TEntity entity);
    Task<bool> DeleteManyAsync(Expression<Func<TEntity, bool>> expression);
    Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> expression);
    Task<TEntity> SelectAsync(Expression<Func<TEntity, bool>> expression);
    IQueryable<TEntity> SelectAll(Expression<Func<TEntity, bool>> expression = null);

    Task SaveAsync();
}