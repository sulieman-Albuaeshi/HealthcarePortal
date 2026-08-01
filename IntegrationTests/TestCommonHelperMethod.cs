using Service.DTOs;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Domain.Enums;

namespace HealthcarePortal.IntegrationTests;

public class UtilityTest
{
    private readonly CustomWebApplicationFactory _factory;
    public UtilityTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ==================== Helper Methods ====================
    public async Task<TokenDto?> LoginAsync(string email, string password)
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/Auth/login", new LoginDto
        {
            Email = email,
            Password = password
        });

        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        return await response.Content.ReadFromJsonAsync<TokenDto>();
    }

    public async Task<TokenDto?> LoginAsAdminAsync()
    {
        return await LoginAsync("admin@test.com", "Admin123!");
    }
    public async Task<TokenDto?> RegisterPatientAsync(string email, string password = "Password123!")
    {
        var _client = _factory.CreateClient();
        var response = await _client.PostAsJsonAsync("/api/Auth/register-patient", new RegisterPatientDto
        {
            Email = email,
            Password = password,
            FirstName = "John",
            LastName = "Doe",
            DateOfBirth = new DateOnly(1990, 1, 1),
            PhoneNumber = "1234567890",
            EmergencyContact = "Emergency Contact"
        });
        var errorContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(errorContent); // Log the error content for debugging

        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        return await response.Content.ReadFromJsonAsync<TokenDto>();
    }
    public HttpClient CreateAuthenticatedClient(TokenDto token)
    {
        var _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        return _client;
    }
    public async Task<TokenDto?> RegisterDoctorAsAdminAsync(string email, string password = "Password123!")
    {
        var adminToken = await LoginAsAdminAsync();
        Assert.NotNull(adminToken);

        var _client = _factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken.AccessToken);

        var response = await _client.PostAsJsonAsync("/api/Auth/register-doctor", new RegisterDoctorDto
        {
            Email = email,
            Password = password,
            FirstName = "Jane",
            LastName = "Smith",
            Specialization = "Cardiology",
            LicenseNumber = "LIC12345"
        });

        if (response.StatusCode != HttpStatusCode.OK)
            return null;

        return await response.Content.ReadFromJsonAsync<TokenDto>();
    }

    public async Task<UserDto?> CreateUserAsAdminAsync(
        string email,
        string passwordHash = "Password123!",
        UserRole role = UserRole.Doctor)
    {
        var adminToken = await LoginAsAdminAsync();
        Assert.NotNull(adminToken);

        var client = CreateAuthenticatedClient(adminToken!);

        var response = await client.PostAsJsonAsync("/api/Users/create", new CreateUserDto
        {
            Email = email,
            PasswordHash = passwordHash,
            Role = role
        });

        if (response.StatusCode != HttpStatusCode.Created)
            return null;

        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

}
