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
[Route("api/PatientProfiles")]
[Authorize]
public class PatientProfilesController : ControllerBase
{
    private readonly IPatientProfileService _patientProfileService;
    private readonly IAuthorizationService _authorizationService;

    public PatientProfilesController(IPatientProfileService patientProfileService, IAuthorizationService authorizationService)
    {
        _patientProfileService = patientProfileService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PatientProfileDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var profiles = await _patientProfileService.GetAllAsync(pageNumber, pageSize);
        return Ok(profiles);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PatientProfileDto>> GetById(Guid id)
    {
        var profile = await _patientProfileService.GetByIdAsync(id);
        if (profile == null)
            return NotFound();

        if (User.IsInRole("Doctor"))
            return Ok(profile);

        var userId = await _patientProfileService.GetUserIDByPatientIdAsync(id);
        if (userId == Guid.Empty)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, userId, new OwnResourceRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        return Ok(profile);
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PatientProfileDto>> Create([FromBody] CreatePatientProfileDto dto)
    {
        var createdProfile = await _patientProfileService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdProfile.Id }, createdProfile);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PatientProfileDto>> Update(Guid id, [FromBody] UpdatePatientProfileDto dto)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid patient profile ID");

        dto.Id = id;

        var userId = await _patientProfileService.GetUserIDByPatientIdAsync(id);
        if (userId == Guid.Empty)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, userId, new OwnResourceRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var updatedProfile = await _patientProfileService.UpdateAsync(dto);
        if (updatedProfile == null)
            return NotFound();

        return Ok(updatedProfile);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Patient")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid patient profile ID");

        var userId = await _patientProfileService.GetUserIDByPatientIdAsync(id);
        if (userId == Guid.Empty)
            return NotFound();

        var authResult = await _authorizationService.AuthorizeAsync(User, userId, new OwnResourceRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _patientProfileService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/appointments")]
    [Authorize(Roles = "Admin,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PatientWithAppointmentsDto>> GetWithAppointments(Guid id)
    {
        var profileWithAppointments = await _patientProfileService.GetWithAppointmentsAsync(id);
        if (profileWithAppointments == null) 
            return NotFound();

        var userId = await _patientProfileService.GetUserIDByPatientIdAsync(id);
        var authResult = await _authorizationService.AuthorizeAsync(User, userId, new OwnResourceRequirement());
        if (!authResult.Succeeded) return Forbid();

        return Ok(profileWithAppointments);
    }

    [HttpGet("{id:guid}/medical-records")]
    [Authorize(Roles = "Admin,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PatientWithMedicalRecordDto>> GetWithMedicalRecords(Guid id)
    {
        var profileWithMedicalRecords = await _patientProfileService.GetWithMedicalRecordsAsync(id);
        if (profileWithMedicalRecords == null) return NotFound();

        var userId = await _patientProfileService.GetUserIDByPatientIdAsync(id);
        var authResult = await _authorizationService.AuthorizeAsync(User, userId, new OwnResourceRequirement());
        if (!authResult.Succeeded) return Forbid();

        return Ok(profileWithMedicalRecords);
    }
}