using Domain.Models;
using Repository.Interfaces;
using Service.DTOs;
using Service.Extensions;
using Service.Interfaces;

namespace Service.Implementations;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly IMedicalRecordRepository _medicalRecordRepository;

    public MedicalRecordService(IMedicalRecordRepository medicalRecordRepository)
    {
        _medicalRecordRepository = medicalRecordRepository;
    }
    public async Task<IEnumerable<MedicalRecordDto>> GetAllAsync(int pageNumber, int pageSize)
    {
        var records = await _medicalRecordRepository.GetAllAsync(pageNumber, pageSize);
        return records.Select(r => r.ToDto());
    }

    public async Task<MedicalRecordDto?> GetByIdAsync(Guid id)
    {
        var record = await _medicalRecordRepository.GetByIdAsync(id);
        return record?.ToDto();
    }

    public async Task<MedicalRecordDto> CreateAsync(CreateMedicalRecordDto dto)
    {
        var entity = dto.ToEntity();
        await _medicalRecordRepository.AddAsync(entity);
        await _medicalRecordRepository.SaveChangesAsync();
        return entity.ToDto();
    }

    public async Task<MedicalRecordDto?> UpdateAsync(UpdateMedicalRecordDto dto)
    {
        var entity = await _medicalRecordRepository.GetByIdAsync(dto.Id);
        if (entity == null)
            return null;

        dto.PatchValuesFrom(entity);
        await _medicalRecordRepository.UpdateAsync(entity);
        await _medicalRecordRepository.SaveChangesAsync();  
        return entity.ToDto();
    }

    public async Task DeleteAsync(Guid id)
    {
        await _medicalRecordRepository.DeleteAsync(id);
        await _medicalRecordRepository.SaveChangesAsync();
    }
    public async Task<IEnumerable<MedicalRecordDto>> GetByPatientIdAsync(Guid patientId)
    {
        var records = await _medicalRecordRepository.GetByPatientIdAsync(patientId);
        return records.Select(r => r.ToDto());
    }

    public async Task<IEnumerable<MedicalRecordDto>> GetByDoctorIdAsync(Guid doctorId)
    {
        var records = await _medicalRecordRepository.GetByDoctorIdAsync(doctorId);
        return records.Select(r => r.ToDto());
    }

    public async Task<IEnumerable<MedicalRecordDto>> GetByTitleAsync(string title)
    {
        var records = await _medicalRecordRepository.GetByTitleAsync(title);
        return records.Select(r => r.ToDto());
    }
}
