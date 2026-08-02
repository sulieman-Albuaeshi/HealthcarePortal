using System.Net;
using System.Net.Http.Json;
using Domain.Enums;
using FluentAssertions;
using Service.DTOs;

namespace HealthcarePortal.IntegrationTests;

public class MedicalRecordsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly UtilityTest _utilityTest;

    public MedicalRecordsControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _utilityTest = new UtilityTest(factory);
    }

    private async Task<(UserDto patientUser, TokenDto patientToken, UserDto doctorUser, TokenDto doctorToken)> CreatePatientAndDoctorUsers()
    {
        var patientEmail = $"patient_{Guid.NewGuid():N}@test.com";
        var patientToken = await _utilityTest.RegisterPatientAsync(patientEmail);
        Assert.NotNull(patientToken);
        var patientClient = _utilityTest.CreateAuthenticatedClient(patientToken);
        var patientProfileResponse = await patientClient.GetAsync("/api/Auth/profile");
        var patientUser = await patientProfileResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(patientUser?.PatientProfile);

        var doctorEmail = $"doctor_{Guid.NewGuid():N}@test.com";
        var doctorToken = await _utilityTest.RegisterDoctorAsAdminAsync(doctorEmail);
        Assert.NotNull(doctorToken);
        var doctorClient = _utilityTest.CreateAuthenticatedClient(doctorToken);
        var doctorProfileResponse = await doctorClient.GetAsync("/api/Auth/profile");
        var doctorUser = await doctorProfileResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(doctorUser?.DoctorProfile);

        return (patientUser!, patientToken!, doctorUser!, doctorToken!);
    }

    private async Task<MedicalRecordDto> CreateRecordAsAdminAsync(Guid patientId, Guid doctorId)
    {
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var createDto = new CreateMedicalRecordDto
        {
            PatientId = patientId,
            DoctorId = doctorId,
            Title = $"Record_{Guid.NewGuid():N}",
            Description = "Test medical record",
            Type = RecordType.Note,
            RecordDate = DateTime.UtcNow
        };

        var response = await client.PostAsJsonAsync("/api/MedicalRecords/create", createDto);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var record = await response.Content.ReadFromJsonAsync<MedicalRecordDto>();
        Assert.NotNull(record);
        return record!;
    }

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsOkWithMedicalRecords()
    {
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        await CreateRecordAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);

        var response = await client.GetAsync("/api/MedicalRecords?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await response.Content.ReadFromJsonAsync<List<MedicalRecordDto>>();
        records.Should().NotBeNull().And.NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_AsNonAdmin_ReturnsForbidden()
    {
        var (_, _, _, doctorToken) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);

        var response = await client.GetAsync("/api/MedicalRecords?pageNumber=1&pageSize=10");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_AsAdmin_ReturnsOk()
    {
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var createdRecord = await CreateRecordAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var response = await client.GetAsync($"/api/MedicalRecords/{createdRecord.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var record = await response.Content.ReadFromJsonAsync<MedicalRecordDto>();
        record.Should().NotBeNull();
        record!.Id.Should().Be(createdRecord.Id);
        record.Title.Should().Be(createdRecord.Title);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var response = await client.GetAsync($"/api/MedicalRecords/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_AsAdmin_ReturnsCreated()
    {
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var createDto = new CreateMedicalRecordDto
        {
            PatientId = patientUser.PatientProfile!.Id,
            DoctorId = doctorUser.DoctorProfile!.Id,
            Title = $"Admin record {Guid.NewGuid():N}",
            Description = "Created by admin",
            Type = RecordType.Diagnosis,
            RecordDate = DateTime.UtcNow
        };

        var response = await client.PostAsJsonAsync("/api/MedicalRecords/create", createDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var record = await response.Content.ReadFromJsonAsync<MedicalRecordDto>();
        record.Should().NotBeNull();
        record!.Title.Should().Be(createDto.Title);
        record.Description.Should().Be(createDto.Description);
    }

    [Fact]
    public async Task Create_AsDoctorOwner_ReturnsCreated()
    {
        var (patientUser, _, doctorUser, doctorToken) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);
        var createDto = new CreateMedicalRecordDto
        {
            PatientId = patientUser.PatientProfile!.Id,
            DoctorId = doctorUser.DoctorProfile!.Id,
            Title = $"Doctor record {Guid.NewGuid():N}",
            Description = "Created by doctor",
            Type = RecordType.Prescription,
            RecordDate = DateTime.UtcNow
        };

        var response = await client.PostAsJsonAsync("/api/MedicalRecords/create", createDto);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Update_AsAdmin_ReturnsOk()
    {
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var createdRecord = await CreateRecordAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var updateDto = new UpdateMedicalRecordDto
        {
            Id = createdRecord.Id,
            Title = "Updated title",
            Description = "Updated description",
            Type = RecordType.LabResult,
            IsDelete = false,
            UpdatedAt = DateTime.UtcNow
        };

        var response = await client.PutAsJsonAsync($"/api/MedicalRecords/{createdRecord.Id}", updateDto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var record = await response.Content.ReadFromJsonAsync<MedicalRecordDto>();
        record.Should().NotBeNull();
        record!.Title.Should().Be("Updated title");
        record.Type.Should().Be(RecordType.LabResult);
    }

    [Fact]
    public async Task Delete_AsAdmin_ReturnsNoContent()
    {
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var createdRecord = await CreateRecordAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var response = await client.DeleteAsync($"/api/MedicalRecords/{createdRecord.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetByPatientId_AsPatientOwner_ReturnsOk()
    {
        var (patientUser, patientToken, doctorUser, _) = await CreatePatientAndDoctorUsers();
        var createdRecord = await CreateRecordAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        var response = await client.GetAsync($"/api/MedicalRecords/patient/{patientUser.PatientProfile.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await response.Content.ReadFromJsonAsync<List<MedicalRecordDto>>();
        records.Should().NotBeNull().And.NotBeEmpty();
        records!.Should().Contain(record => record.Id == createdRecord.Id);
    }

    [Fact]
    public async Task GetByPatientId_AsOtherPatient_ReturnsForbidden()
    {
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        await CreateRecordAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);

        var (_, otherPatientToken, _, _) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(otherPatientToken);

        var response = await client.GetAsync($"/api/MedicalRecords/patient/{patientUser.PatientProfile.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetByDoctorId_AsDoctorOwner_ReturnsOk()
    {
        var (patientUser, _, doctorUser, doctorToken) = await CreatePatientAndDoctorUsers();
        var createdRecord = await CreateRecordAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);

        var response = await client.GetAsync($"/api/MedicalRecords/doctor/{doctorUser.DoctorProfile.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await response.Content.ReadFromJsonAsync<List<MedicalRecordDto>>();
        records.Should().NotBeNull().And.NotBeEmpty();
        records!.Should().Contain(record => record.Id == createdRecord.Id);
    }

    [Fact]
    public async Task GetByDoctorId_AsOtherDoctor_ReturnsForbidden()
    {
        var (patientUser, _, doctorUser, _) = await CreatePatientAndDoctorUsers();
        await CreateRecordAsAdminAsync(patientUser.PatientProfile!.Id, doctorUser.DoctorProfile!.Id);

        var (_, _, _, otherDoctorToken) = await CreatePatientAndDoctorUsers();
        var client = _utilityTest.CreateAuthenticatedClient(otherDoctorToken);

        var response = await client.GetAsync($"/api/MedicalRecords/doctor/{doctorUser.DoctorProfile.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}