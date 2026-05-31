using System.Linq.Expressions;
using Carevionix.Data;
using Carevionix.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _set;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var query = _set.AsQueryable();
        if (predicate is not null)
        {
            query = query.Where(predicate);
        }

        return await query.ToListAsync();
    }

    public async Task<T> AddAsync(T entity)
    {
        _set.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(T entity)
    {
        _set.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        _set.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public IQueryable<T> Query() => _set.AsQueryable();
}
