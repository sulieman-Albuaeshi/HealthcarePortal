using Microsoft.AspNetCore.Authorization;
using API.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.DTOs;
using Service.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/Appointments")]
[Authorize]
public class AppointmentsController : ControllerBase
    {
    private readonly IAppointmentService _appointmentService; 
    private readonly IAuthorizationService _authorizationService;

    public AppointmentsController(IAppointmentService appointmentService, IAuthorizationService authorizationService)
    {
        _appointmentService = appointmentService;
        _authorizationService = authorizationService;
    }   

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var appointments = await _appointmentService.GetAllAsync(pageNumber, pageSize);
        return Ok(appointments);
    }


    [HttpGet("{id:guid}")]
    //policy: "Admin,Doctor,Patient"
    [Authorize(Roles = "Admin,Doctor,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AppointmentDto>> GetById(Guid id)
    {
        var appointment = await _appointmentService.GetByIdWithDoctorAndPatientAsync(id);

        if (appointment == null)
            return NotFound();

        var authDoctorResult = await _authorizationService.AuthorizeAsync(
            User, appointment?.Doctor?.Id, new OwnResourceRequirement());

        var authPatientResult = await _authorizationService.AuthorizeAsync(
            User, appointment?.Patient?.Id, new OwnResourceRequirement());

        if (!authDoctorResult.Succeeded && !authPatientResult.Succeeded)
            return Forbid();

        return Ok(appointment);
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentDto dto)
    {
        var createdAppointment = await _appointmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = createdAppointment.Id }, createdAppointment);
    }


    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)] 
    public async Task<ActionResult<AppointmentDto>> Update(Guid id, [FromBody] UpdateAppointmentDto dto)
    {
        if (id == Guid.Empty)
            return BadRequest("Invalid appointment ID");
        
        dto.Id = id;

        var existingAppointment = await _appointmentService.GetByIdWithDoctorAndPatientAsync(id);
        if (existingAppointment == null)
            return NotFound();  

        var authDoctorResult = await _authorizationService.AuthorizeAsync(
            User, existingAppointment?.Doctor?.Id, new OwnResourceRequirement());

        if (!authDoctorResult.Succeeded)
            return Forbid();

        var updatedAppointment = await _appointmentService.UpdateAsync(dto);
        if (updatedAppointment == null)
            return NotFound();

        return Ok(updatedAppointment);
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
            return BadRequest("Invalid appointment ID");

        var existing = await _appointmentService.GetByIdWithDoctorAndPatientAsync(id);
        if (existing == null)
            return NotFound();

        var authDoctorResult = await _authorizationService.AuthorizeAsync(
            User, existing?.Doctor?.Id, new OwnResourceRequirement());

        if (!authDoctorResult.Succeeded)
            return Forbid();

        await _appointmentService.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = "Admin,Patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]   
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByPatient(Guid patientId)
    {
        var authPatientResult = await _authorizationService.AuthorizeAsync(
            User, patientId, new OwnResourceRequirement());

        if (!authPatientResult.Succeeded)
            return Forbid();

        var appointments = await _appointmentService.GetByPatientIdAsync(patientId);
        return Ok(appointments);
    }

    [HttpGet("doctor/{doctorId:guid}")]
    [Authorize(Roles = "Admin,Doctor")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByDoctor(Guid doctorId)
    {
        var authDoctorResult = await _authorizationService.AuthorizeAsync(
            User, doctorId, new OwnResourceRequirement());

        if (!authDoctorResult.Succeeded)
            return Forbid();

        var appointments = await _appointmentService.GetByDoctorIdAsync(doctorId);
        return Ok(appointments);
    }


    [HttpGet("date")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetByDate([FromQuery] DateTime date)
    {
        var appointments = await _appointmentService.GetByDateAsync(date);
        return Ok(appointments);
    }


    [HttpGet("upcoming")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<AppointmentDto>>> GetUpcoming()
    {
        var appointments = await _appointmentService.GetUpcomingAppointmentsAsync();
        return Ok(appointments);
    }
}
