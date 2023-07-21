using Collectify.Data.DbContexts;
using Collectify.Data.IRepositories;
using Collectify.Domain.Commons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Collectify.Data.Repositories;

public class Repository<TEntity> : IRepository<TEntity> where TEntity : Auditable
{
    private DbSet<TEntity> dbSet;
    private AppDbContext dbContext;

    public Repository(AppDbContext dbContext)
    {
        dbSet = dbContext.Set<TEntity>();
        this.dbContext = dbContext;
    }


    public async Task<bool> DeleteAsync(Expression<Func<TEntity, bool>> expression)
    {
        var entity = await dbSet.FirstOrDefaultAsync(expression);

        if (entity is not null)
        {
            dbSet.Remove(entity);
            return true;
        }

        return false;
    }

    public async Task<bool> DeleteManyAsync(Expression<Func<TEntity, bool>> expression)
    {
        var entities = await dbSet.Where(expression).ToListAsync();

        foreach (var entity in entities)
            dbSet.Remove(entity);

        return entities.Count > 0;
    }

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        EntityEntry<TEntity> entry = await dbSet.AddAsync(entity);

        return entry.Entity;
    }

    public Task SaveAsync()
        => dbContext.SaveChangesAsync();

    public IQueryable<TEntity> SelectAll(Expression<Func<TEntity, bool>> expression = null)
        => expression is null ? dbSet : dbSet.Where(expression);

    public async Task<TEntity> SelectAsync(Expression<Func<TEntity, bool>> expression)
        => await dbSet.FirstOrDefaultAsync(expression);
}