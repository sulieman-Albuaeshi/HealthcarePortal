using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Service.DTOs;

namespace HealthcarePortal.IntegrationTests;

public class AppointmentControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly UtilityTest _utilityTest;

    public AppointmentControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _utilityTest = new UtilityTest(factory);
    }

    // ==================== Helper Methods ====================

    private async Task<(UserDto patientUser, TokenDto patientToken, UserDto doctorUser, TokenDto doctorToken)> CreatePatientAndDoctorUsers()
    {
        // Create Patient
        var patientEmail = $"patient_{Guid.NewGuid():N}@test.com";
        var patientToken = await _utilityTest.RegisterPatientAsync(patientEmail);
        Assert.NotNull(patientToken);
        var patientClient = _utilityTest.CreateAuthenticatedClient(patientToken);
        var patientProfileResponse = await patientClient.GetAsync("/api/Auth/profile");
        var patientUser = await patientProfileResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(patientUser?.PatientProfile);

        // Create Doctor
        var doctorEmail = $"doctor_{Guid.NewGuid():N}@test.com";
        var doctorToken = await _utilityTest.RegisterDoctorAsAdminAsync(doctorEmail);
        Assert.NotNull(doctorToken);
        var doctorClient = _utilityTest.CreateAuthenticatedClient(doctorToken);
        var doctorProfileResponse = await doctorClient.GetAsync("/api/Auth/profile");
        var doctorUser = await doctorProfileResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(doctorUser?.DoctorProfile);

        return (patientUser, patientToken, doctorUser, doctorToken);
    }

    private async Task<AppointmentDto> CreateAppointmentAsAdminAsync(Guid patientId, Guid doctorId)
    {
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);
        var createDto = new CreateAppointmentDto
        {
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduledAt = DateTime.UtcNow.AddDays(7),
            DurationMinutes = 30,
            Status = Domain.Enums.AppointmentStatus.Scheduled,
            Notes = "Test appointment"
        };

        var response = await client.PostAsJsonAsync("/api/Appointments/create", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDto>();
        Assert.NotNull(appointment);
        return appointment;
    }

    // ==================== GetAll Tests ====================

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsOk()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);
        
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);

        // Act
        var response = await client.GetAsync("/api/Appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var appointments = await response.Content.ReadFromJsonAsync<List<AppointmentDto>>();
        appointments.Should().NotBeNull().And.NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var (_, _, _, doctorToken) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);

        // Act
        var response = await client.GetAsync("/api/Appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== GetById Tests ====================

    [Fact]
    public async Task GetById_AsPatientOwner_ReturnsOk()
    {
        // Arrange
        var (patientUser, patientToken, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var appointment = await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync($"/api/Appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AppointmentDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(appointment.Id);
    }

    [Fact]
    public async Task GetById_AsDoctorOwner_ReturnsOk()
    {
        // Arrange
        var (patientUser, _, doctorUser, doctorToken) = await CreatePatientAndDoctorUsers();
        var appointment = await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);

        // Act
        var response = await client.GetAsync($"/api/Appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AppointmentDto>();
        result.Should().NotBeNull();
        result!.Id.Should().Be(appointment.Id);
    }

    [Fact]
    public async Task GetById_AsAdmin_ReturnsOk()
    {
        // Arrange
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var appointment = await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.GetAsync($"/api/Appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var appointment = await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);

        var (_, otherPatientToken, _, _) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(otherPatientToken);

        // Act
        var response = await client.GetAsync($"/api/Appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== Create Tests ====================

    [Fact]
    public async Task Create_AsDoctor_WithValidData_ReturnsCreated()
    {
        // Arrange
        var (patientUser, _, doctorUser, doctorToken) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);
        var createDto = new CreateAppointmentDto
        {
            PatientId = patientUser.PatientProfile!.Id,
            DoctorId = doctorUser.DoctorProfile!.Id,
            ScheduledAt = DateTime.UtcNow.AddDays(5),
            DurationMinutes = 45,
            Status = Domain.Enums.AppointmentStatus.Scheduled
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Appointments/create", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var appointment = await response.Content.ReadFromJsonAsync<AppointmentDto>();
        appointment.Should().NotBeNull();
        appointment!.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Create_AsPatient_ReturnsForbidden()
    {
        // Arrange
        var (patientUser, patientToken, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);
        var createDto = new CreateAppointmentDto
        {
            PatientId = patientUser.PatientProfile!.Id,
            DoctorId = doctorUser.DoctorProfile!.Id,
            ScheduledAt = DateTime.UtcNow.AddDays(5)
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/Appointments/create", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== Update Tests ====================

    [Fact]
    public async Task Update_AsDoctorOwner_ReturnsOk()
    {
        // Arrange
        var (patientUser, _, doctorUser, doctorToken) = await CreatePatientAndDoctorUsers();
        var appointment = await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);
        var updateDto = new UpdateAppointmentDto
        {
            Id = appointment.Id,
            Status = Domain.Enums.AppointmentStatus.Completed,
            Notes = "Updated notes.",
            UpdatedBy = doctorUser.Id
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/Appointments/{appointment.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedAppointment = await response.Content.ReadFromJsonAsync<AppointmentDto>();
        updatedAppointment.Should().NotBeNull();
        updatedAppointment!.Status.Should().Be(Domain.Enums.AppointmentStatus.Completed);
        updatedAppointment.Notes.Should().Be("Updated notes.");
    }

    [Fact]
    public async Task Update_AsPatient_ReturnsForbidden()
    {
        // Arrange
        var (patientUser, patientToken, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var appointment = await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);
        var updateDto = new UpdateAppointmentDto
        {
            Notes = "Patient trying to update",
            UpdatedBy = patientUser.Id
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/Appointments/{appointment.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== Delete Tests ====================

    [Fact]
    public async Task Delete_AsDoctorOwner_ReturnsNoContent()
    {
        // Arrange
        var (patientUser, _, doctorUser, doctorToken) = await CreatePatientAndDoctorUsers();
        var appointment = await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);

        // Act
        var response = await client.DeleteAsync($"/api/Appointments/{appointment.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await client.GetAsync($"/api/Appointments/{appointment.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ==================== GetByPatientId Tests ====================

    [Fact]
    public async Task GetByPatientId_AsPatientOwner_ReturnsOk()
    {
        // Arrange
        var (patientUser, patientToken, doctorUser, _) = await CreatePatientAndDoctorUsers();
        await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync($"/api/Appointments/patient/{patientUser.PatientProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var appointments = await response.Content.ReadFromJsonAsync<List<AppointmentDto>>();
        appointments.Should().NotBeNull().And.HaveCount(1);
        appointments!.First().Patient!.Id.Should().Be(patientUser.PatientProfile.Id);
    }

    [Fact]
    public async Task GetByPatientId_AsOtherPatient_ReturnsForbidden()
    {
        // Arrange
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);

        var (_, otherPatientToken, _, _) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(otherPatientToken);

        // Act
        var response = await client.GetAsync($"/api/Appointments/patient/{patientUser.PatientProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== GetByDoctorId Tests ====================

    [Fact]
    public async Task GetByDoctorId_AsDoctorOwner_ReturnsOk()
    {
        // Arrange
        var (patientUser, _, doctorUser, doctorToken) = await CreatePatientAndDoctorUsers();
        await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);

        // Act
        var response = await client.GetAsync($"/api/Appointments/doctor/{doctorUser.DoctorProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var appointments = await response.Content.ReadFromJsonAsync<List<AppointmentDto>>();
        appointments.Should().NotBeNull().And.HaveCount(1);
        appointments!.First().Doctor!.Id.Should().Be(doctorUser.DoctorProfile.Id);
    }

    [Fact]
    public async Task GetByDoctorId_AsOtherDoctor_ReturnsForbidden()
    {
        // Arrange
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        await CreateAppointmentAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);

        var (_, _, _, otherDoctorToken) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(otherDoctorToken);

        // Act
        var response = await client.GetAsync($"/api/Appointments/doctor/{doctorUser.DoctorProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}