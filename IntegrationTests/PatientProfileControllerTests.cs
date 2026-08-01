using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Service.DTOs;
using Xunit;

namespace HealthcarePortal.IntegrationTests;

public class PatientProfileControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly UtilityTest _utilityTest;

    public PatientProfileControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _utilityTest = new UtilityTest(factory);
    }

    // Helper to create a patient and get their profile
    private async Task<(TokenDto token, PatientProfileDto profile)> CreatePatientAndGetProfile(string email)
    {
        var token = await _utilityTest.RegisterPatientAsync(email);
        Assert.NotNull(token);

        var client = _utilityTest.CreateAuthenticatedClient(token);
        var profileResponse = await client.GetAsync("/api/Auth/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await profileResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.NotNull(user.PatientProfile);

        return (token, user.PatientProfile);
    }

    // ==================== GetAll Tests ====================

    [Fact]
    public async Task GetAll_AsAnyAuthenticatedUser_ReturnsOkWithProfiles()
    {
        // Arrange
        await _utilityTest.RegisterPatientAsync("patient_getall1@test.com");
        await _utilityTest.RegisterPatientAsync("patient_getall2@test.com");
        var patientToken = await _utilityTest.RegisterPatientAsync("patient_getall_viewer@test.com");
        Assert.NotNull(patientToken);

        var adminToken = await _utilityTest.LoginAsAdminAsync();    
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.GetAsync("/api/PatientProfiles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profiles = await response.Content.ReadFromJsonAsync<IEnumerable<PatientProfileDto>>();
        profiles.Should().NotBeNull();
        profiles.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/PatientProfiles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ==================== GetById Tests ====================

    [Fact]
    public async Task GetById_AsAdmin_WithValidId_ReturnsOkWithProfile()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_getbyid_admin@test.com");
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<PatientProfileDto>();
        profile.Should().NotBeNull();
        profile.Id.Should().Be(patientProfile.Id);
        profile.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetById_AsProfileOwner_WithValidId_ReturnsOkWithProfile()
    {
        // Arrange
        var (patientToken, patientProfile) = await CreatePatientAndGetProfile("patient_getbyid_owner@test.com");
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<PatientProfileDto>();
        profile.Should().NotBeNull();
        profile.Id.Should().Be(patientProfile.Id);
        profile.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetById_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_getbyid_other1@test.com");
        var otherPatientToken = await _utilityTest.RegisterPatientAsync("patient_getbyid_other2@test.com");
        Assert.NotNull(otherPatientToken);
        var client = _utilityTest.CreateAuthenticatedClient(otherPatientToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/PatientProfiles/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ==================== Create Tests ====================

    [Fact]
    public async Task Create_AsAdmin_WithValidData_ReturnsCreated()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Create a user first to get a UserId
        var createdUser = await _utilityTest.CreateUserAsAdminAsync("patient_create_user@test.com", "Password123!", Domain.Enums.UserRole.Patient);
        Assert.NotNull(createdUser);

        // Act
        var response = await client.PostAsJsonAsync("/api/PatientProfiles/create", new CreatePatientProfileDto
        {
            Id = createdUser.Id,
            FirstName = "Alice",
            LastName = "Wonderland",
            DateOfBirth = new DateOnly(1995, 3, 15),
            PhoneNumber = "5551234567",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var profile = await response.Content.ReadFromJsonAsync<PatientProfileDto>();
        profile.Should().NotBeNull();
        profile.Id.Should().NotBeEmpty();
        profile.FirstName.Should().Be("Alice");
        profile.LastName.Should().Be("Wonderland");
    }

    [Fact]
    public async Task Create_AsAdmin_WithMissingFirstName_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var createdUser = await _utilityTest.CreateUserAsAdminAsync("patient_create_missing_fn_user@test.com", "Password123!", Domain.Enums.UserRole.Patient);
        Assert.NotNull(createdUser);

        // Act
        var response = await client.PostAsJsonAsync("/api/PatientProfiles/create", new CreatePatientProfileDto
        {
            Id = createdUser.Id,
            FirstName = "",
            LastName = "Wonderland",
            DateOfBirth = new DateOnly(1995, 3, 15),
            PhoneNumber = "5551234567",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_AsAdmin_WithFutureDateOfBirth_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var createdUser = await _utilityTest.CreateUserAsAdminAsync("patient_create_future_dob_user@test.com", "Password123!", Domain.Enums.UserRole.Patient);
        Assert.NotNull(createdUser);

        // Act
        var response = await client.PostAsJsonAsync("/api/PatientProfiles/create", new CreatePatientProfileDto
        {
            Id = createdUser.Id,
            FirstName = "Alice",
            LastName = "Wonderland",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            PhoneNumber = "5551234567",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_AsAdmin_WithEmptyUserId_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/PatientProfiles/create", new CreatePatientProfileDto
        {
            Id = Guid.Empty,
            FirstName = "Alice",
            LastName = "Wonderland",
            DateOfBirth = new DateOnly(1995, 3, 15),
            PhoneNumber = "5551234567",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/PatientProfiles/create", new CreatePatientProfileDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Wonderland",
            DateOfBirth = new DateOnly(1995, 3, 15),
            PhoneNumber = "5551234567",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("patient_create_nonadmin@test.com");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/PatientProfiles/create", new CreatePatientProfileDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Wonderland",
            DateOfBirth = new DateOnly(1995, 3, 15),
            PhoneNumber = "5551234567",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== Update Tests ====================

    [Fact]
    public async Task Update_AsProfileOwner_WithValidData_ReturnsOk()
    {
        // Arrange
        var (patientToken, patientProfile) = await CreatePatientAndGetProfile("patient_update_owner@test.com");
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        var updateDto = new UpdatePatientProfileDto
        {
            Id = patientProfile.Id,
            FirstName = "Alicia",
            LastName = "Wonderland",
            DateOfBirth = patientProfile.DateOfBirth,
            PhoneNumber = "5559999999",
            EmergencyContact = "Updated Contact",
            IsDeleted = false
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/PatientProfiles/{patientProfile.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedProfile = await response.Content.ReadFromJsonAsync<PatientProfileDto>();
        updatedProfile.Should().NotBeNull();
        updatedProfile.FirstName.Should().Be("Alicia");
        updatedProfile.PhoneNumber.Should().Be("5559999999");
    }

    [Fact]
    public async Task Update_AsAdmin_WithValidData_ReturnsOk()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_update_admin@test.com");
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var updateDto = new UpdatePatientProfileDto
        {
            Id = patientProfile.Id,
            FirstName = "Alicia",
            LastName = "Smith",
            DateOfBirth = patientProfile.DateOfBirth,
            PhoneNumber = "5558888888",
            EmergencyContact = "Admin Updated Contact",
            IsDeleted = false
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/PatientProfiles/{patientProfile.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedProfile = await response.Content.ReadFromJsonAsync<PatientProfileDto>();
        updatedProfile.Should().NotBeNull();
        updatedProfile.FirstName.Should().Be("Alicia");
        updatedProfile.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task Update_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_update_other1@test.com");
        var otherPatientToken = await _utilityTest.RegisterPatientAsync("patient_update_other2@test.com");
        Assert.NotNull(otherPatientToken);
        var client = _utilityTest.CreateAuthenticatedClient(otherPatientToken);

        var updateDto = new UpdatePatientProfileDto
        {
            Id = patientProfile.Id,
            FirstName = "Hacker",
            LastName = "Man",
            DateOfBirth = patientProfile.DateOfBirth,
            PhoneNumber = "0000000000",
            EmergencyContact = "Hacked Contact",
            IsDeleted = false
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/PatientProfiles/{patientProfile.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var nonExistentId = Guid.NewGuid();

        var updateDto = new UpdatePatientProfileDto
        {
            Id = nonExistentId,
            FirstName = "Alice",
            LastName = "Wonderland",
            DateOfBirth = new DateOnly(1995, 3, 15),
            PhoneNumber = "5551234567",
            EmergencyContact = "Emergency Contact",
            IsDeleted = false
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/PatientProfiles/{nonExistentId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_AsAdmin_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var updateDto = new UpdatePatientProfileDto
        {
            Id = Guid.Empty,
            FirstName = "Alice",
            LastName = "Wonderland",
            DateOfBirth = new DateOnly(1995, 3, 15),
            PhoneNumber = "5551234567",
            EmergencyContact = "Emergency Contact",
            IsDeleted = false
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/PatientProfiles/{Guid.Empty}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/api/PatientProfiles/{Guid.NewGuid()}", new UpdatePatientProfileDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Alice",
            LastName = "Wonderland",
            DateOfBirth = new DateOnly(1995, 3, 15),
            PhoneNumber = "5551234567",
            EmergencyContact = "Emergency Contact",
            IsDeleted = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ==================== Delete Tests ====================

    [Fact]
    public async Task Delete_AsAdmin_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_delete_admin@test.com");
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.DeleteAsync($"/api/PatientProfiles/{patientProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's gone
        var getResponse = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsProfileOwner_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var (patientToken, patientProfile) = await CreatePatientAndGetProfile("patient_delete_owner@test.com");
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.DeleteAsync($"/api/PatientProfiles/{patientProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's gone
        var getResponse = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_delete_other1@test.com");
        var otherPatientToken = await _utilityTest.RegisterPatientAsync("patient_delete_other2@test.com");
        Assert.NotNull(otherPatientToken);
        var client = _utilityTest.CreateAuthenticatedClient(otherPatientToken);

        // Act
        var response = await client.DeleteAsync($"/api/PatientProfiles/{patientProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.DeleteAsync($"/api/PatientProfiles/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsAdmin_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.DeleteAsync($"/api/PatientProfiles/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/api/PatientProfiles/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ==================== GetWithAppointments Tests ====================

    [Fact]
    public async Task GetWithAppointments_AsAdmin_WithValidId_ReturnsOk()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_appointments_admin@test.com");
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profileWithAppointments = await response.Content.ReadFromJsonAsync<PatientWithAppointmentsDto>();
        profileWithAppointments.Should().NotBeNull();
        profileWithAppointments.Id.Should().Be(patientProfile.Id);
        profileWithAppointments.Appointments.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWithAppointments_AsProfileOwner_WithValidId_ReturnsOk()
    {
        // Arrange
        var (patientToken, patientProfile) = await CreatePatientAndGetProfile("patient_appointments_owner@test.com");
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profileWithAppointments = await response.Content.ReadFromJsonAsync<PatientWithAppointmentsDto>();
        profileWithAppointments.Should().NotBeNull();
        profileWithAppointments.Id.Should().Be(patientProfile.Id);
    }

    [Fact]
    public async Task GetWithAppointments_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_appointments_other1@test.com");
        var otherPatientToken = await _utilityTest.RegisterPatientAsync("patient_appointments_other2@test.com");
        Assert.NotNull(otherPatientToken);
        var client = _utilityTest.CreateAuthenticatedClient(otherPatientToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetWithAppointments_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{Guid.NewGuid()}/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWithAppointments_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/PatientProfiles/{Guid.NewGuid()}/appointments");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ==================== GetWithMedicalRecords Tests ====================

    [Fact]
    public async Task GetWithMedicalRecords_AsAdmin_WithValidId_ReturnsOk()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_medical_admin@test.com");
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}/medical-records");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profileWithMedicalRecords = await response.Content.ReadFromJsonAsync<PatientWithMedicalRecordDto>();
        profileWithMedicalRecords.Should().NotBeNull();
        profileWithMedicalRecords.Id.Should().Be(patientProfile.Id);
        profileWithMedicalRecords.MedicalRecords.Should().NotBeNull();
    }

    [Fact]
    public async Task GetWithMedicalRecords_AsProfileOwner_WithValidId_ReturnsOk()
    {
        // Arrange
        var (patientToken, patientProfile) = await CreatePatientAndGetProfile("patient_medical_owner@test.com");
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}/medical-records");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profileWithMedicalRecords = await response.Content.ReadFromJsonAsync<PatientWithMedicalRecordDto>();
        profileWithMedicalRecords.Should().NotBeNull();
        profileWithMedicalRecords.Id.Should().Be(patientProfile.Id);
    }

    [Fact]
    public async Task GetWithMedicalRecords_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (_, patientProfile) = await CreatePatientAndGetProfile("patient_medical_other1@test.com");
        var otherPatientToken = await _utilityTest.RegisterPatientAsync("patient_medical_other2@test.com");
        Assert.NotNull(otherPatientToken);
        var client = _utilityTest.CreateAuthenticatedClient(otherPatientToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{patientProfile.Id}/medical-records");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetWithMedicalRecords_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.GetAsync($"/api/PatientProfiles/{Guid.NewGuid()}/medical-records");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetWithMedicalRecords_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/PatientProfiles/{Guid.NewGuid()}/medical-records");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
