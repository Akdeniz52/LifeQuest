namespace LevelingSystem.API.Models.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid CharacterId { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public int Level { get; set; }
    public int CurrentXP { get; set; }
}
