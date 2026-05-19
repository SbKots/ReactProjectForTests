using Microsoft.EntityFrameworkCore;
using TestManagement.Api.Application.Interfaces;
using TestManagement.Api.Infrastructure.Data;

namespace TestManagement.Api.Infrastructure.Repositories;

public class EfRepository<TEntity>(ApplicationDbContext dbContext) : IRepository<TEntity>
    where TEntity : class
{
    public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Set<TEntity>().FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
    }

    public void Remove(TEntity entity)
    {
        dbContext.Set<TEntity>().Remove(entity);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
