using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Repository.Interfaces;
using Service.DTOs;
using Service.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/DoctorProfiles")]
[Authorize]
public class DoctorProfilesController : ControllerBase
{
    private readonly IDoctorProfileService _doctorService;
    private readonly IAuthorizationService _authorizationService;

    public DoctorProfilesController(IDoctorProfileService doctorProfileService, IAuthorizationService authorizationService)
    {
        _doctorService = doctorProfileService;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<DoctorProfileDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var profiles = await _doctorService.GetAllAsync(pageNumber, pageSize);
        return Ok(profiles);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DoctorProfileDto>> GetById(Guid id)
    {
        var profile = await _doctorService.GetByIdAsync(id);
        if (profile == null)
        {
            return NotFound();
        }
        return Ok(profile);
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DoctorProfileDto>> Create([FromBody] CreateDoctorProfileDto dto)
    {
        var createdProfile = await _doctorService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdProfile.Id }, createdProfile);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DoctorProfileDto>> Update(Guid id, [FromBody] UpdateDoctorProfileDto dto)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid doctor profile ID");

        dto.Id = id;

        var authResult = await _authorizationService.AuthorizeAsync(User, dto.Id, new OwnResourceRequirement());
        if (!authResult.Succeeded)
            return Forbid();
        

        var updatedProfile = await _doctorService.UpdateAsync(dto);
        if (updatedProfile == null)
            return NotFound();

        return Ok(updatedProfile);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid doctor profile ID");

        var authResult = await _authorizationService.AuthorizeAsync(User, id, new OwnResourceRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        await _doctorService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("specialization/{specialization}")]
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<DoctorProfileDto>>> GetBySpecialization(string specialization)
    {
        if (string.IsNullOrEmpty(specialization))
            return BadRequest("Invalid specialization");

        var profiles = await _doctorService.GetBySpecializationAsync(specialization);
        return Ok(profiles);
    }

    [HttpGet("{id:guid}/appointments")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DoctorWithAppointmentDto>> GetWithAppointments(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid doctor profile ID");

        var authResult = await _authorizationService.AuthorizeAsync(User, id, new OwnResourceRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var profileWithAppointments = await _doctorService.GetWithAppointmentsAsync(id);
        if (profileWithAppointments == null)
        {
            return NotFound();
        }
        return Ok(profileWithAppointments);
    }

    [HttpGet("{id:guid}/Medical-records")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DoctorWithMedicalRecordDto>> GetWithMedicalRecords(Guid id)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid doctor profile ID");

        var authResult = await _authorizationService.AuthorizeAsync(User, id, new OwnResourceRequirement());
        if (!authResult.Succeeded)
            return Forbid();

        var profileWithMedicalRecords = await _doctorService.GetWithMedicalRecordsAsync(id);
        if (profileWithMedicalRecords == null)
        {
            return NotFound();
        }
        return Ok(profileWithMedicalRecords);
    }
}