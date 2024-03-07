using Microsoft.EntityFrameworkCore;

namespace Repository;

public class EntityFrameworkRepository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    private readonly DbSet<T> _dbSet;
    private readonly bool _autoSaveChanges;

    public EntityFrameworkRepository(DbContext context, bool autoSaveChanges = true)
    {
        _context = context;
        _dbSet = _context.Set<T>();
        _autoSaveChanges = autoSaveChanges;
    }

    protected EntityFrameworkRepository()
    {
        throw new NotImplementedException();
    }

    public T GetById(int id)
    {
        return _dbSet.Find(id);
    }
  
    public void Add(T entity)
    {
        _dbSet.Add(entity);
        if(_autoSaveChanges)
            _context.SaveChanges();
    }
  
    public void Update(T entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        if(_autoSaveChanges)
            _context.SaveChanges();
    }
  
    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
        if(_autoSaveChanges)
            _context.SaveChanges();
    }
  
    public IEnumerable<T> GetAll()
    {
        return _dbSet.ToList();
    }
}