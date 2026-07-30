using Service.DTOs;

namespace Service.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetAllAsync(int pageNumber, int pageSize);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserDto dto);
    Task<UserDto?> UpdateAsync(UserDto dto);
    Task DeleteAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<UserDto?> GetUserWithProfilesAsync(Guid userId);
}
