using Microsoft.EntityFrameworkCore;
using Domain.Models;
using Repository.Interfaces;

namespace Repository.Implementations;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(DbContext context) : base(context)
    {
    }

    public async Task<User?> GetUserByEmailAndPassword(string email, string passwordHash)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetUserWithProfilesAsync(Guid userId)
    {  
        return await _dbSet
            .Include(u => u.DoctorProfile)
            .Include(u => u.PatientProfile)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

}
