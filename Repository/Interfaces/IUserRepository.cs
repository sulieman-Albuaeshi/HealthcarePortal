using Domain.Models;

namespace Repository.Interfaces;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetUserWithProfilesAsync(Guid userId);
}
