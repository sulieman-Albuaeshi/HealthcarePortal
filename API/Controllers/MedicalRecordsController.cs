using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/MedicalRecords")]
[Authorize]
public class MedicalRecordsController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPatientProfileService _patientProfileService;
    private readonly IDoctorProfileService _doctorProfileService;

    public MedicalRecordsController(
        IMedicalRecordService medicalRecordService,
        IAuthorizationService authorizationService,
        IPatientProfileService patientProfileService,
        IDoctorProfileService doctorProfileService)
    {
        _medicalRecordService = medicalRecordService;
        _authorizationService = authorizationService;
        _patientProfileService = patientProfileService;
        _doctorProfileService = doctorProfileService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<MedicalRecordDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var records = await _medicalRecordService.GetAllAsync(pageNumber, pageSize);
        return Ok(records);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MedicalRecordDto>> GetById(Guid id)
    {
        var record = await _medicalRecordService.GetByIdAsync(id);
        if (record == null)
            return NotFound();  

        // check if the Patient is the owner of the record 
        var patientUserId = record.Patient?.Id ?? Guid.Empty;
        var patientAuthResult = await _authorizationService.AuthorizeAsync(User, patientUserId, new OwnResourceRequirement());
        if (patientAuthResult.Succeeded)
            return Ok(record);

        // check if the Doctor is the owner of the record
        if (record.Doctor != null)
        {
            var doctorUserId = record.Doctor.Id;
            var doctorAuthResult = await _authorizationService.AuthorizeAsync(User, doctorUserId, new OwnResourceRequirement());
            if (doctorAuthResult.Succeeded)
                return Ok(record);
        }

        return Forbid();
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MedicalRecordDto>> Create([FromBody] CreateMedicalRecordDto dto)
    {
        if (User.IsInRole("Doctor") && dto.DoctorId.HasValue)
        {
            var doctorUserId = await _doctorProfileService.GetUserIDByDoctorIdAsync(dto.DoctorId.Value);
            var authResult = await _authorizationService.AuthorizeAsync(User, doctorUserId, new OwnResourceRequirement());
            if (!authResult.Succeeded)
                return Forbid();
        }
        
        var createdRecord = await _medicalRecordService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdRecord.Id }, createdRecord);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<MedicalRecordDto>> Update(Guid id, [FromBody] UpdateMedicalRecordDto dto)
    {
        dto.Id = id;

        var existingRecord = await _medicalRecordService.GetByIdAsync(id);
        if (existingRecord == null)
            return NotFound();

        if (existingRecord.Doctor != null)
        {
            var doctorUserId = await _doctorProfileService.GetUserIDByDoctorIdAsync(existingRecord.Doctor.Id);
            var authResult = await _authorizationService.AuthorizeAsync(User, doctorUserId, new OwnResourceRequirement());
            if (!authResult.Succeeded)
                return Forbid();
        }

        var updatedRecord = await _medicalRecordService.UpdateAsync(dto);
        return Ok(updatedRecord);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existingRecord = await _medicalRecordService.GetByIdAsync(id);
        if (existingRecord == null)
            return NotFound();

        if (existingRecord.Doctor != null)
        {
            var doctorUserId = await _doctorProfileService.GetUserIDByDoctorIdAsync(existingRecord.Doctor.Id);
            var authResult = await _authorizationService.AuthorizeAsync(User, doctorUserId, new OwnResourceRequirement());
            if (!authResult.Succeeded)
                return Forbid();
        }

        await _medicalRecordService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<MedicalRecordDto>>> GetByPatientId(Guid patientId)
    {
        if (User.IsInRole("Patient"))
        {
            var userId = await _patientProfileService.GetUserIDByPatientIdAsync(patientId);
            var authResult = await _authorizationService.AuthorizeAsync(User, userId, new OwnResourceRequirement());
            if (!authResult.Succeeded)
                return Forbid();
        }

        var records = await _medicalRecordService.GetByPatientIdAsync(patientId);
        return Ok(records);
    }

    [HttpGet("doctor/{doctorId:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<MedicalRecordDto>>> GetByDoctorId(Guid doctorId)
    {
        var doctorUserId = await _doctorProfileService.GetUserIDByDoctorIdAsync(doctorId);
        var authResult = await _authorizationService.AuthorizeAsync(User, doctorUserId, new OwnResourceRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var records = await _medicalRecordService.GetByDoctorIdAsync(doctorId);
        return Ok(records);
    }
}