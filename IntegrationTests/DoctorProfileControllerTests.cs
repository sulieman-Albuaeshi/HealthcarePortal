using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Repository.Interfaces;
using Domain.Models;
using Service.DTOs;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Domain.Enums;
using Service.Interfaces;

namespace HealthcarePortal.IntegrationTests;

public class DoctorProfileControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly UtilityTest _utilityTest;

    public DoctorProfileControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _utilityTest = new UtilityTest(factory);
    }

    // Helper to create a doctor and get their profile
    private async Task<(TokenDto token, DoctorProfileDto profile)> CreateDoctorAndGetProfile(string email)
    {
        var token = await _utilityTest.RegisterDoctorAsAdminAsync(email);
        Assert.NotNull(token);

        var client = _utilityTest.CreateAuthenticatedClient(token);
        var profileResponse = await client.GetAsync("/api/Auth/profile");
        profileResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await profileResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(user);
        Assert.NotNull(user.DoctorProfile);

        return (token, user.DoctorProfile);
    }

    // ==================== GetAll Tests ====================
    [Fact]
    public async Task GetAll_AsAnyAuthenticatedUser_ReturnsOkWithProfiles()
    {
        // Arrange
        await _utilityTest.RegisterDoctorAsAdminAsync("doc_getall1@test.com");
        var patientToken = await _utilityTest.RegisterPatientAsync("patient_getall_docs@test.com");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync("/api/DoctorProfiles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profiles = await response.Content.ReadFromJsonAsync<IEnumerable<DoctorProfileDto>>();
        profiles.Should().NotBeNull();
        profiles.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/DoctorProfiles");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ==================== GetById Tests ====================
    [Fact]
    public async Task GetById_WithValidId_ReturnsOkWithProfile()
    {
        // Arrange
        var (_, doctorProfile) = await CreateDoctorAndGetProfile("doc_getbyid@test.com");
        var patientToken = await _utilityTest.RegisterPatientAsync("patient_getbyid_doc@test.com");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync($"/api/DoctorProfiles/{doctorProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<DoctorProfileDto>();
        profile.Should().NotBeNull();
        profile.Id.Should().Be(doctorProfile.Id);
        profile.FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("patient_getbyid_doc_invalid@test.com");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync($"/api/DoctorProfiles/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ==================== Create Tests ====================

    [Fact]
    public async Task Create_AsAdmin_WithValidData_ReturnsCreated()
    {
        // Arrange
        // To create a doctor profile, we first need a user with the 'Doctor' role.
        // Using the helper method ensures the user is created correctly via the API,
        // which avoids validation errors.
        var userToBecomeDoctor = await _utilityTest.CreateUserAsAdminAsync(
            "doc_profile_user@test.com", 
            "Password123!", 
            UserRole.Doctor);
        Assert.NotNull(userToBecomeDoctor);

        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);

        var client = _utilityTest.CreateAuthenticatedClient(adminToken);
        var createDto = new CreateDoctorProfileDto
        {
            Id = userToBecomeDoctor.Id,
            FirstName = "Gregory",
            LastName = "House",
            Specialization = "Nephrology",
            LicenseNumber = "LIC-CREATE-VALID",
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/DoctorProfiles/create", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdProfile = await response.Content.ReadFromJsonAsync<DoctorProfileDto>();
        createdProfile.Should().NotBeNull();
        createdProfile.FirstName.Should().Be("Gregory");
        createdProfile.Specialization.Should().Be("Nephrology");

        // Verify the location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location.ToString().Should().Contain($"/api/DoctorProfiles/{createdProfile.Id}");
    }

    [Fact]
    public async Task Create_AsAdmin_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);
        var createDto = new CreateDoctorProfileDto
        {
            Id = Guid.NewGuid(),
            FirstName = "", // Invalid
            LastName = "House",
            Specialization = "Nephrology",
            LicenseNumber = "LIC-CREATE-INVALID"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/DoctorProfiles/create", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_AsAdmin_ForUserWithExistingProfile_ReturnsBadConflict()
    {
        // Arrange
        // Create a doctor with a profile
        var (doctorToken, _) = await CreateDoctorAndGetProfile("doc_create_existing@test.com");

        // Get the user info for the created doctor
        var doctorClient = _utilityTest.CreateAuthenticatedClient(doctorToken);
        var profileResponse = await doctorClient.GetAsync("/api/Auth/profile");
        var doctorUser = await profileResponse.Content.ReadFromJsonAsync<UserDto>();
        Assert.NotNull(doctorUser);

        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        var createDto = new CreateDoctorProfileDto
        {
            Id = doctorUser.Id, // User already has a profile
            FirstName = "Another",
            LastName = "Profile",
            Specialization = "Dermatology",
            LicenseNumber = "LIC-DUPLICATE"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/DoctorProfiles/create", createDto);

        // Assert
        // This depends on service implementation. Assuming it prevents duplicates and returns BadRequest.
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Create_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("patient_creating_doc@test.com");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);
        var createDto = new CreateDoctorProfileDto
        {
            Id = Guid.NewGuid(),
            FirstName = "Gregory",
            LastName = "House",
            Specialization = "Nephrology",
            LicenseNumber = "LIC-CREATE-FORBID"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/DoctorProfiles/create", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createDto = new CreateDoctorProfileDto { Id = Guid.NewGuid() };

        // Act
        var response = await client.PostAsJsonAsync("/api/DoctorProfiles/create", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ==================== Update Tests ====================
    [Fact]
    public async Task Update_AsProfileOwner_WithValidData_ReturnsOk()
    {
        // Arrange
        var (doctorToken, doctorProfile) = await CreateDoctorAndGetProfile("doc_update_owner@test.com");
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);
        var updateDto = new UpdateDoctorProfileDto
        {
            Id = doctorProfile.Id,
            FirstName = "Janet",
            LastName = "Smithy",
            Specialization = "Pediatrics",
            LicenseNumber = doctorProfile.LicenseNumber
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/DoctorProfiles/{doctorProfile.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedProfile = await response.Content.ReadFromJsonAsync<DoctorProfileDto>();
        updatedProfile.Should().NotBeNull();
        updatedProfile.FirstName.Should().Be("Janet");
        updatedProfile.Specialization.Should().Be("Pediatrics");
    }

    [Fact]
    public async Task Update_AsAdmin_WithValidData_ReturnsOk()
    {
        // Arrange
        var (_, doctorProfile) = await CreateDoctorAndGetProfile("doc_update_admin@test.com");
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);
        var updateDto = new UpdateDoctorProfileDto
        {
            Id = doctorProfile.Id,
            FirstName = "Janelle",
            LastName = "Smith",
            Specialization = "Oncology",
            LicenseNumber = doctorProfile.LicenseNumber
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/DoctorProfiles/{doctorProfile.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedProfile = await response.Content.ReadFromJsonAsync<DoctorProfileDto>();
        updatedProfile.Should().NotBeNull();
        updatedProfile.FirstName.Should().Be("Janelle");
        updatedProfile.Specialization.Should().Be("Oncology");
    }

    [Fact]
    public async Task Update_AsOtherUser_ReturnsForbidden()
    {
        // Arrange
        var (_, doctorProfile) = await CreateDoctorAndGetProfile("doc_update_other@test.com");
        var otherUserToken = await _utilityTest.RegisterPatientAsync("patient_update_doc@test.com");
        Assert.NotNull(otherUserToken);
        var client = _utilityTest.CreateAuthenticatedClient(otherUserToken);
        var updateDto = new UpdateDoctorProfileDto
        {
            Id = doctorProfile.Id,
            FirstName = "Hacker",
            LastName = "Man",
            Specialization = "Hacking",
            LicenseNumber = "HACK123"
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/DoctorProfiles/{doctorProfile.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
    
    [Fact]
    public async Task Update_WithMismatchedIdInBody_ReturnsBadRequest()
    {
        // Arrange
        var (doctorToken, doctorProfile) = await CreateDoctorAndGetProfile("doc_update_mismatch@test.com");
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);
        var updateDto = new UpdateDoctorProfileDto
        {
            Id = Guid.Empty, // Mismatched ID
            FirstName = "Janet",
            LastName = "Smithy",
            Specialization = "Pediatrics",
            LicenseNumber = doctorProfile.LicenseNumber
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/DoctorProfiles/{doctorProfile.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ==================== GetBySpecialization Tests ====================
    [Fact]
    public async Task GetBySpecialization_WithExistingSpecialization_ReturnsOkWithProfiles()
    {
        // Arrange
        await _utilityTest.RegisterDoctorAsAdminAsync("doc_spec1@test.com");
        await _utilityTest.RegisterDoctorAsAdminAsync("doc_spec2@test.com");
        
        var patientToken = await _utilityTest.RegisterPatientAsync("patient_get_spec@test.com");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);
        var specialization = "Cardiology";

        // Act
        var response = await client.GetAsync($"/api/DoctorProfiles/specialization/{specialization}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profiles = await response.Content.ReadFromJsonAsync<IEnumerable<DoctorProfileDto>>();
        profiles.Should().NotBeNull();
        profiles.Should().HaveCount(2);
        profiles.Should().OnlyContain(p => p.Specialization == specialization);
    }

    [Fact]
    public async Task GetBySpecialization_WithNonExistingSpecialization_ReturnsOkWithEmptyList()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("patient_get_spec_empty@test.com");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);
        var specialization = "Astrology";

        // Act
        var response = await client.GetAsync($"/api/DoctorProfiles/specialization/{specialization}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profiles = await response.Content.ReadFromJsonAsync<IEnumerable<DoctorProfileDto>>();
        profiles.Should().NotBeNull();
        profiles.Should().BeEmpty();
    }
    
    // ==================== Delete Tests ====================
    [Fact]
    public async Task Delete_AsAdmin_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var (_, doctorProfile) = await CreateDoctorAndGetProfile("doc_delete_admin@test.com");
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.DeleteAsync($"/api/DoctorProfiles/{doctorProfile.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify it's gone
        var getResponse = await client.GetAsync($"/api/DoctorProfiles/{doctorProfile.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var (doctorToken, doctorProfile) = await CreateDoctorAndGetProfile("new_notadmin@test.com");
        var (doctorTokenToDelete, doctorProfileToDelete) = await CreateDoctorAndGetProfile("doc_delete_notadmin@test.com");
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);

        // Act
        var response = await client.DeleteAsync($"/api/DoctorProfiles/{doctorProfileToDelete.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Delete_AsAdmin_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.DeleteAsync($"/api/DoctorProfiles/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}