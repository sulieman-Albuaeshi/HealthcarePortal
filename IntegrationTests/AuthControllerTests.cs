using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
 using System.Text.Json.Nodes;
using FluentAssertions;
using Service.DTOs;
using Xunit;

namespace HealthcarePortal.IntegrationTests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly UtilityTest _utilityTest;
    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _utilityTest = new UtilityTest(factory);
    }

    // ==================== Login Tests ====================

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var email = "login_valid@test.com";
        var password = "Password123!";
        await _utilityTest.RegisterPatientAsync(email, password);

        // Act
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
        {
            Email = email,
            Password = password
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadFromJsonAsync<TokenDto>();
        token.Should().NotBeNull();
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var email = "login_invalid_pw@test.com";
        await _utilityTest.RegisterPatientAsync(email, "Password123!");

        // Act
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
        {
            Email = email,
            Password = "WrongPassword123!"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
        {
            Email = "nonexistent@test.com",
            Password = "Password123!"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyEmail_ReturnsBadRequest()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
        {
            Email = "",
            Password = "Password123!"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ReturnsBadRequest()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
        {
            Email = "login_empty_pw@test.com",
            Password = ""
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInvalidEmailFormat_ReturnsBadRequest()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
        {
            Email = "invalid-email",
            Password = "Password123!"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ==================== Register Patient Tests ====================

    [Fact]
    public async Task RegisterPatient_WithValidData_ReturnsOkWithToken()
    {
        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/register-patient", new RegisterPatientDto
        {
            Email = "patient_valid@test.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhoneNumber = "1234567890",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadFromJsonAsync<TokenDto>();
        token.Should().NotBeNull();
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterPatient_WithDuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = "patient_dup@test.com";
        await _utilityTest.RegisterPatientAsync(email, "Password123!");

        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/register-patient", new RegisterPatientDto
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1985, 5, 15),
            PhoneNumber = "0987654321",
            EmergencyContact = "Another Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterPatient_WithInvalidEmail_ReturnsBadRequest()
    {
        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/register-patient", new RegisterPatientDto
        {
            Email = "invalid-email",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhoneNumber = "1234567890",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterPatient_WithShortPassword_ReturnsBadRequest()
    {
        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/register-patient", new RegisterPatientDto
        {
            Email = "patient_short_pw@test.com",
            Password = "short",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhoneNumber = "1234567890",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterPatient_WithMissingFirstName_ReturnsBadRequest()
    {
        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/register-patient", new RegisterPatientDto
        {
            Email = "patient_no_fn@test.com",
            Password = "Password123!",
            FirstName = "",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhoneNumber = "1234567890",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterPatient_WithFutureDateOfBirth_ReturnsBadRequest()
    {
        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/register-patient", new RegisterPatientDto
        {
            Email = "patient_future_dob@test.com",
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            PhoneNumber = "1234567890",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterPatient_WithExistingDoctorEmail_UpgradesToPatientAndDoctor()
    {
        // Arrange - Register a doctor first (requires Admin)
        var email = "doctor_then_patient@test.com";
        await _utilityTest.RegisterDoctorAsAdminAsync(email, "Password123!");

        // Act - Register a patient with the same email
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/register-patient", new RegisterPatientDto
        {
            Email = email,
            Password = "Password123!",
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhoneNumber = "1234567890",
            EmergencyContact = "Emergency Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var token = await response.Content.ReadFromJsonAsync<TokenDto>();
        token.Should().NotBeNull();
        token.AccessToken.Should().NotBeNullOrEmpty();
    }

    // ==================== Register Doctor Tests ====================

    [Fact]
    public async Task RegisterDoctor_WithValidDataAsAdmin_ReturnsOkWithToken()
    {
        // Act
        var token = await _utilityTest.RegisterDoctorAsAdminAsync("doctor_valid@test.com", "Password123!");

        // Assert
        token.Should().NotBeNull();
        token.AccessToken.Should().NotBeNullOrEmpty();
        token.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RegisterDoctor_WithDuplicateEmailAsAdmin_ReturnsBadRequest()
    {
        // Arrange
        var email = "doctor_dup@test.com";
        await _utilityTest.RegisterDoctorAsAdminAsync(email, "Password123!");

        // Act
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        var response = await client.PostAsJsonAsync("/api/Auth/register-doctor", new RegisterDoctorDto
        {
            Email = email,
            Password = "Password123!",
            FirstName = "Another",
            LastName = "Doctor",
            Specialization = "Neurology",
            LicenseNumber = "LIC99999"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterDoctor_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/register-doctor", new RegisterDoctorDto
        {
            Email = "doctor_no_auth@test.com",
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Smith",
            Specialization = "Cardiology",
            LicenseNumber = "LIC12345"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterDoctor_WithoutAdminRole_ReturnsForbidden()
    {
        // Arrange - Register and login as a patient (non-admin)
        var patientToken = await _utilityTest.RegisterPatientAsync("patient_for_doctor@test.com", "Password123!");
        Assert.NotNull(patientToken);

        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/register-doctor", new RegisterDoctorDto
        {
            Email = "doctor_non_admin@test.com",
            Password = "Password123!",
            FirstName = "Jane",
            LastName = "Smith",
            Specialization = "Cardiology",
            LicenseNumber = "LIC12345"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RegisterDoctor_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/register-doctor", new RegisterDoctorDto
        {
            Email = "invalid-email",
            Password = "short",
            FirstName = "",
            LastName = "",
            Specialization = "",
            LicenseNumber = ""
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ==================== Refresh Token Tests ====================

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsOkWithNewToken()
    {
        // Arrange - Login to get a valid refresh token
        var email = "refresh_valid@test.com";
        var token = await _utilityTest.RegisterPatientAsync(email, "Password123!");
        Assert.NotNull(token);

        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", new RefreshTokenRequestDto
        {
            RefreshToken = token.RefreshToken
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var newToken = await response.Content.ReadFromJsonAsync<TokenDto>();
        newToken.Should().NotBeNull();
        newToken.AccessToken.Should().NotBeNullOrEmpty();
        newToken.RefreshToken.Should().NotBeNullOrEmpty();
        newToken.RefreshToken.Should().NotBe(token.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ReturnsUnauthorized()
    {
        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", new RefreshTokenRequestDto
        {
            RefreshToken = "invalid-refresh-token"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RefreshToken_WithEmptyToken_ReturnsBadRequest()
    {
        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/refresh-token", new RefreshTokenRequestDto
        {
            RefreshToken = ""
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ==================== Logout Tests ====================

    [Fact]
    public async Task Logout_WithValidToken_ReturnsNotImplemented()
    {
        // Arrange - Login to get a valid token
        var token = await _utilityTest.RegisterPatientAsync("logout_valid@test.com", "Password123!");
        Assert.NotNull(token);

        var client = _utilityTest.CreateAuthenticatedClient(token);

        // Act
        var response = await client.PostAsJsonAsync("/api/Auth/logout", new RefreshTokenRequestDto
        {
            RefreshToken = token.RefreshToken
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotImplemented);
    }

    [Fact]
    public async Task Logout_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/logout", new RefreshTokenRequestDto
        {
            RefreshToken = "some-refresh-token"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    
    [Fact]
    public async Task GetUserProfile_WhenUserIsAdmin_ReturnsUserWithoutProfiles()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken);

        // Act
        var response = await client.GetAsync("/api/Auth/profile");
 
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<JsonObject>();
        user.Should().NotBeNull();
        user["email"]!.GetValue<string>().Should().Be("admin@test.com");
        user["patientProfile"]?.GetValue<object>().Should().BeNull();
        user["doctorProfile"]?.GetValue<object>().Should().BeNull();
    }

    [Fact]
    public async Task GetUserProfile_WhenUserIsPatient_ReturnsUserWithPatientProfile()
    {
        // Arrange
        var email = "profile_patient@test.com";
        var patientToken = await _utilityTest.RegisterPatientAsync(email);
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync("/api/Auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<JsonObject>();
        user.Should().NotBeNull();
        user["email"]!.GetValue<string>().Should().Be(email);
        user["patientProfile"].Should().NotBeNull();
        user["patientProfile"]?["firstName"]!.GetValue<string>().Should().Be("John");
        user["doctorProfile"]?.GetValue<object>().Should().BeNull();
    }

    [Fact]
    public async Task GetUserProfile_WhenUserIsDoctor_ReturnsUserWithDoctorProfile()
    {
        // Arrange
        var email = "profile_doctor@test.com";
        var doctorToken = await _utilityTest.RegisterDoctorAsAdminAsync(email);
        Assert.NotNull(doctorToken);
        var client = _utilityTest.CreateAuthenticatedClient(doctorToken);

        // Act
        var response = await client.GetAsync("/api/Auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<JsonObject>();
        user.Should().NotBeNull();
        user["email"]!.GetValue<string>().Should().Be(email);
        user["patientProfile"]?.GetValue<object>().Should().BeNull();
        user["doctorProfile"].Should().NotBeNull();
        user["doctorProfile"]?["firstName"]!.GetValue<string>().Should().Be("Jane");
        user["doctorProfile"]?["specialization"]!.GetValue<string>().Should().Be("Cardiology");
    }

    [Fact]
    public async Task GetUserProfile_WhenUserIsPatientAndDoctor_ReturnsUserWithBothProfiles()
    {
        // Arrange
        var email = "profile_both@test.com";
        var password = "Password123!";
        await _utilityTest.RegisterDoctorAsAdminAsync(email, password);
        await _utilityTest.RegisterPatientAsync(email, password);
        var token = await _utilityTest.LoginAsync(email, password);
        Assert.NotNull(token);
        var client = _utilityTest.CreateAuthenticatedClient(token);

        // Act
        var response = await client.GetAsync("/api/Auth/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<JsonObject>();
        user.Should().NotBeNull();
        user["email"]!.GetValue<string>().Should().Be(email);
        user["patientProfile"]?.Should().NotBeNull();
        user["patientProfile"]?["firstName"]!.GetValue<string>().Should().Be("John");
        user["doctorProfile"]?.Should().NotBeNull();
        user["doctorProfile"]?["firstName"]!.GetValue<string>().Should().Be("Jane");
    }

    [Fact]
    public async Task GetUserProfile_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Arrange
        var token = new TokenDto
        {
            AccessToken = "invalid-access-token",
            RefreshToken = "invalid-refresh-token"
        };

        var client = _utilityTest.CreateAuthenticatedClient(token);
        var response = await client.GetAsync("/api/Auth/profile/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

}