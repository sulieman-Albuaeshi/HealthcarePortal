using Domain.Models;
using Repository.Interfaces;
using Service.DTOs;
using Service.Extensions;
using Service.Interfaces;

namespace Service.Implementations;

public class DoctorProfileService : IDoctorProfileService
{
    private readonly IDoctorProfileRepository _doctorProfileRepository;

    public DoctorProfileService(IDoctorProfileRepository doctorProfileRepository)
    {
        _doctorProfileRepository = doctorProfileRepository;
    }
     public async Task<IEnumerable<DoctorProfileDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var profiles = await _doctorProfileRepository.GetAllAsync(pageNumber, pageSize);
        return profiles.Select(p => p.ToDto());
    }

    public async Task<DoctorProfileDto?> GetByIdAsync(Guid id)
    {
        var profile = await _doctorProfileRepository.GetByIdAsync(id);
        return profile?.ToDto();
    }

    public async Task<DoctorProfileDto> CreateAsync(CreateDoctorProfileDto dto)
    {
        var entity = dto.ToEntity();
        await _doctorProfileRepository.AddAsync(entity);
        await _doctorProfileRepository.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task<DoctorProfileDto?> UpdateAsync(UpdateDoctorProfileDto dto)
    {
        var entity = await _doctorProfileRepository.GetByIdAsync(dto.Id);
        if (entity == null)
            return null;

        dto.PatchValuesFrom(entity);
        await _doctorProfileRepository.UpdateAsync(entity);
        await _doctorProfileRepository.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _doctorProfileRepository.DeleteAsync(id);
        await _doctorProfileRepository.SaveChangesAsync();
    }

    public async Task<DoctorProfileDto?> GetByUserIdAsync(Guid userId)
    {
        var profile = await _doctorProfileRepository.GetByUserIdAsync(userId);
        return profile?.ToDto();
    }

    public async Task<DoctorProfileDto?> GetWithAppointmentsAsync(Guid id)
    {
        var profile = await _doctorProfileRepository.GetWithAppointmentsAsync(id);
        return profile?.ToDto();
    }

    public async Task<DoctorProfileDto?> GetWithMedicalRecordsAsync(Guid id)
    {
        var profile = await _doctorProfileRepository.GetWithMedicalRecordsAsync(id);
        return profile?.ToDto();
    }

    public async Task<IEnumerable<DoctorProfileDto>> GetBySpecializationAsync(string specialization)
    {
        var profiles = await _doctorProfileRepository.GetBySpecializationAsync(specialization);
        return profiles.Select(p => p.ToDto());
    }

    public async Task<Guid> GetUserIDByDoctorIdAsync(Guid doctorProfileId)
    {
        return await _doctorProfileRepository.GetUserIDByDoctorIdAsync(doctorProfileId);
    }
}
