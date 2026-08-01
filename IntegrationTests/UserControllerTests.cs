using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.Enums;
using FluentAssertions;
using Service.DTOs;
using Xunit;

namespace HealthcarePortal.IntegrationTests;

public class UserControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly UtilityTest _utilityTest;

    public UserControllerTests(CustomWebApplicationFactory factory)
    {
        _utilityTest = new UtilityTest(factory);
        _factory = factory;
    }
    // ==================== GetAll Tests ====================

    [Fact]
    public async Task GetAll_AsAdmin_ReturnsOkWithUsers()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.GetAsync("/api/Users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
        users.Should().NotBeNull();
        users.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAll_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/Users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("user_getall_nonadmin@test.com", "Password123!");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync("/api/Users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== GetById Tests ====================

    [Fact]
    public async Task GetById_AsAdmin_WithValidId_ReturnsOkWithUser()
    {
        // Arrange
        var createdUser = await _utilityTest.CreateUserAsAdminAsync("user_getbyid_valid@test.com", "Password123!", UserRole.Doctor);
        Assert.NotNull(createdUser);

        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.GetAsync($"/api/Users/{createdUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user.Id.Should().Be(createdUser.Id);
        user.Email.Should().Be("user_getbyid_valid@test.com");
    }

    [Fact]
    public async Task GetById_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.GetAsync($"/api/Users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetById_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/api/Users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetById_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("user_getbyid_nonadmin@test.com", "Password123!");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync($"/api/Users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== Create Tests ====================

    [Fact]
    public async Task Create_AsAdmin_WithValidData_ReturnsCreated()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.PostAsJsonAsync("/api/Users/create", new CreateUserDto
        {
            Email = "user_create_valid@test.com",
            PasswordHash = "Password123!",
            Role = UserRole.Doctor
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user.Id.Should().NotBeEmpty();
        user.Email.Should().Be("user_create_valid@test.com");
        user.Role.Should().Be(UserRole.Doctor);
    }

    [Fact]
    public async Task Create_AsAdmin_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.PostAsJsonAsync("/api/Users/create", new CreateUserDto
        {
            Email = "invalid-email",
            PasswordHash = "Password123!",
            Role = UserRole.Doctor
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_AsAdmin_WithShortPassword_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.PostAsJsonAsync("/api/Users/create", new CreateUserDto
        {
            Email = "user_create_short_pw@test.com",
            PasswordHash = "short",
            Role = UserRole.Doctor
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_AsAdmin_WithInvalidRole_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.PostAsJsonAsync("/api/Users/create", new CreateUserDto
        {
            Email = "user_create_invalid_role@test.com",
            PasswordHash = "Password123!",
            Role = (UserRole)byte.MaxValue // Invalid role
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Users/create", new CreateUserDto
        {
            Email = "user_create_no_auth@test.com",
            PasswordHash = "Password123!",
            Role = UserRole.Doctor
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("user_create_nonadmin@test.com", "Password123!");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.PostAsJsonAsync("/api/Users/create", new CreateUserDto
        {
            Email = "user_create_nonadmin_target@test.com",
            PasswordHash = "Password123!",
            Role = UserRole.Doctor
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== Update Tests ====================

    [Fact]
    public async Task Update_AsAdmin_WithValidData_ReturnsOk()
    {
        // Arrange
        var createdUser = await _utilityTest.CreateUserAsAdminAsync("user_update_valid@test.com", "Password123!", UserRole.Doctor);
        Assert.NotNull(createdUser);

        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        var updatedUser = new UserDto
        {
            Id = createdUser.Id,
            Email = createdUser.Email,
            PasswordHash = "NewPassword123!",
            Role = UserRole.Doctor,
            IsActive = true,
            CreatedAt = createdUser.CreatedAt
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/Users/{createdUser.Id}", updatedUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Update_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        var nonExistentId = Guid.NewGuid();

        var updateUser = new UserDto
        {
            Id = nonExistentId,
            Email = "nonexistent@test.com",
            PasswordHash = "Password123!",
            Role = UserRole.Doctor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/Users/{nonExistentId}", updateUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_AsAdmin_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        var updateUser = new UserDto
        {
            Id = Guid.Empty,
            Email = "user_update_empty_id@test.com",
            PasswordHash = "Password123!",
            Role = UserRole.Doctor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var response = await client.PutAsJsonAsync($"/api/Users/{Guid.Empty}", updateUser);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.PutAsJsonAsync($"/api/Users/{Guid.NewGuid()}", new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "user_update_no_auth@test.com",
            PasswordHash = "Password123!",
            Role = UserRole.Doctor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("user_update_nonadmin@test.com", "Password123!");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.PutAsJsonAsync($"/api/Users/{Guid.NewGuid()}", new UserDto
        {
            Id = Guid.NewGuid(),
            Email = "user_update_nonadmin_target@test.com",
            PasswordHash = "Password123!",
            Role = UserRole.Doctor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== Delete Tests ====================

    [Fact]
    public async Task Delete_AsAdmin_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var createdUser = await _utilityTest.CreateUserAsAdminAsync("user_delete_valid@test.com", "Password123!", UserRole.Doctor);
        Assert.NotNull(createdUser);

        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.DeleteAsync($"/api/Users/{createdUser.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_AsAdmin_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.DeleteAsync($"/api/Users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_AsAdmin_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.DeleteAsync($"/api/Users/{Guid.Empty}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.DeleteAsync($"/api/Users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Delete_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("user_delete_nonadmin@test.com", "Password123!");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.DeleteAsync($"/api/Users/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ==================== GetByEmail Tests ====================

    [Fact]
    public async Task GetByEmail_AsAdmin_WithValidEmail_ReturnsOk()
    {
        // Arrange
        var createdUser = await _utilityTest.CreateUserAsAdminAsync("user_getbyemail_valid@test.com", "Password123!", UserRole.Doctor);
        Assert.NotNull(createdUser);

        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.GetAsync("/api/Users/email?email=user_getbyemail_valid@test.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user.Email.Should().Be("user_getbyemail_valid@test.com");
    }

    [Fact]
    public async Task GetByEmail_AsAdmin_WithNonExistentEmail_ReturnsNotFound()
    {
        // Arrange
        var adminToken = await _utilityTest.LoginAsAdminAsync();
        Assert.NotNull(adminToken);
        var client = _utilityTest.CreateAuthenticatedClient(adminToken!);

        // Act
        var response = await client.GetAsync("/api/Users/email?email=nonexistent@test.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByEmail_WithoutAuthentication_ReturnsUnauthorized()
    {
        // Act
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/Users/email?email=test@test.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetByEmail_AsNonAdmin_ReturnsForbidden()
    {
        // Arrange
        var patientToken = await _utilityTest.RegisterPatientAsync("user_getbyemail_nonadmin@test.com", "Password123!");
        Assert.NotNull(patientToken);
        var client = _utilityTest.CreateAuthenticatedClient(patientToken);

        // Act
        var response = await client.GetAsync("/api/Users/email?email=test@test.com");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
