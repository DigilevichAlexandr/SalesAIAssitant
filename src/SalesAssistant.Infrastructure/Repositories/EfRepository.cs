using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SalesAssistant.Application.Abstractions;
using SalesAssistant.Infrastructure.Persistence;

namespace SalesAssistant.Infrastructure.Repositories;

public class EfRepository<T> : IRepository<T> where T : class
{
    private readonly AppDbContext _dbContext;

    public EfRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => _dbContext.Set<T>().FirstOrDefaultAsync(predicate, cancellationToken);

    public Task<List<T>> ListAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => _dbContext.Set<T>().Where(predicate).ToListAsync(cancellationToken);

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => _dbContext.Set<T>().AddAsync(entity, cancellationToken).AsTask();

    public void Update(T entity) => _dbContext.Set<T>().Update(entity);
}
