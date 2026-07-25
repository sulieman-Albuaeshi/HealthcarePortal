using Domain.Models;
using Repository.Interfaces;
using Service.DTOs;
using Service.Extensions;
using Service.Interfaces;

namespace Service.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var users = await _userRepository.GetAllAsync(pageNumber, pageSize);
        return users.Select(u => u.ToDto());
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user?.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        var entity = dto.ToEntity();
        await _userRepository.AddAsync(entity);
        await _userRepository.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task<UserDto?> UpdateAsync(UserDto dto)
    {
        var entity = await _userRepository.GetByIdAsync(dto.Id);
        if (entity == null)
            return null;

        dto.PatchValuesFrom(entity);
        await _userRepository.UpdateAsync(entity);
        await _userRepository.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _userRepository.DeleteAsync(id);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user?.ToDto();
    }

    public async Task<UserDto?> GetUserWithProfilesAsync(Guid userId)
    {
        var user = await _userRepository.GetUserWithProfilesAsync(userId);
        return user?.ToDto();
    }

    public async Task<UserDto?> GetUserByEmailAndPasswordAsync(string email, string passwordHash)
    {
        var user = await _userRepository.GetUserByEmailAndPassword(email, passwordHash);
        return user?.ToDto();
    }
}
