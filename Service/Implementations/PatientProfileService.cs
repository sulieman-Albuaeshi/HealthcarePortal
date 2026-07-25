using Domain.Models;
using Repository.Interfaces;
using Service.DTOs;
using Service.Extensions;
using Service.Interfaces;

namespace Service.Implementations;

public class PatientProfileService : IPatientProfileService
{
    private readonly IPatientProfileRepository _patientProfileRepository;

    public PatientProfileService(IPatientProfileRepository patientProfileRepository)
    {
        _patientProfileRepository = patientProfileRepository;
    }
    public async Task<IEnumerable<PatientProfileDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var profiles = await _patientProfileRepository.GetAllAsync(pageNumber, pageSize);
        return profiles.Select(p => p.ToDto());
    }

    public async Task<PatientProfileDto?> GetByIdAsync(Guid id)
    {
        var profile = await _patientProfileRepository.GetByIdAsync(id);
        return profile?.ToDto();
    }

    public async Task<PatientProfileDto> CreateAsync(CreatePatientProfileDto dto)
    {
        var entity = dto.ToEntity();
        await _patientProfileRepository.AddAsync(entity);
        await _patientProfileRepository.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task<PatientProfileDto?> UpdateAsync(UpdatePatientProfileDto dto)
    {
        var entity = await _patientProfileRepository.GetByIdAsync(dto.Id);
        if (entity == null)
            return null;

        dto.PatchValuesFrom(entity);
        await _patientProfileRepository.UpdateAsync(entity);
        await _patientProfileRepository.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _patientProfileRepository.DeleteAsync(id);
        await _patientProfileRepository.SaveChangesAsync();
    }

    public async Task<PatientProfileDto?> GetByUserIdAsync(Guid userId)
    {
        var profile = await _patientProfileRepository.GetByUserIdAsync(userId);
        return profile?.ToDto();
    }

    public async Task<PatientProfileDto?> GetWithAppointmentsAsync(Guid id)
    {
        var profile = await _patientProfileRepository.GetWithAppointmentsAsync(id);
        return profile?.ToDto();
    }

    public async Task<PatientProfileDto?> GetWithMedicalRecordsAsync(Guid id)
    {
        var profile = await _patientProfileRepository.GetWithMedicalRecordsAsync(id);
        return profile?.ToDto();
    }
}
