using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FoodDatabase.App.Services.Interfaces;

namespace FoodDatabase.App.Data.Repositories
{
    /// <summary>
    /// Entity Framework-basierte Repository-Implementierung für generische CRUD-Operationen.
    /// Speichert nach jeder Mutation sofort, da Services nicht selbst SaveChangesAsync aufrufen.
    /// </summary>
    /// <typeparam name="T">Der Entity-Typ (muss eine Klasse sein).</typeparam>
    public class EfRepository<T> : IRepository<T> where T : class
    {
        private readonly FoodDatabaseContext _context;

        public EfRepository(FoodDatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<T> GetByIdAsync(int id)
        {
            return (await _context.Set<T>().FindAsync(id))!;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T> AddAsync(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Set<T>().Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> CreateAsync(T entity)
        {
            return await AddAsync(entity);
        }

        public async Task<T> UpdateAsync(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Set<T>().FindAsync(id);
            if (entity is null)
                return false;

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
