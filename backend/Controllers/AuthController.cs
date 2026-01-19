using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LevelingSystem.API.Data;
using LevelingSystem.API.Models.Entities;
using LevelingSystem.API.Models.DTOs;

namespace LevelingSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.CharacterName))
            {
                return BadRequest(new { message = "All fields are required" });
            }

            // Check if user already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Email already registered" });
            }

            // Hash password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Create user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // Create character
            var character = new Character
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = request.CharacterName,
                Level = 1,
                CurrentXP = 0,
                TotalXP = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Characters.Add(character);

            // Initialize default stats
            var defaultStats = await _context.StatDefinitions
                .Where(s => s.IsActive && s.UnlockLevel == 1)
                .ToListAsync();

            foreach (var statDef in defaultStats)
            {
                var characterStat = new CharacterStat
                {
                    Id = Guid.NewGuid(),
                    CharacterId = character.Id,
                    StatDefinitionId = statDef.Id,
                    CurrentValue = 0,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CharacterStats.Add(characterStat);
            }

            // Create welcome system message
            var welcomeMessage = new SystemMessage
            {
                Id = Guid.NewGuid(),
                CharacterId = character.Id,
                MessageType = MessageType.Achievement,
                Title = "[ SYSTEM ]",
                Content = $"Welcome, {character.Name}. Your journey begins now.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.SystemMessages.Add(welcomeMessage);

            await _context.SaveChangesAsync();

            _logger.LogInformation("New user registered: {Email}", user.Email);

            // Generate token
            var token = GenerateJwtToken(user.Id, user.Email, character.Id);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                CharacterId = character.Id,
                CharacterName = character.Name,
                Level = character.Level,
                CurrentXP = character.CurrentXP
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        try
        {
            // Find user
            var user = await _context.Users
                .Include(u => u.Character)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("User logged in: {Email}", user.Email);

            // Generate token
            var token = GenerateJwtToken(user.Id, user.Email, user.Character!.Id);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                CharacterId = user.Character.Id,
                CharacterName = user.Character.Name,
                Level = user.Character.Level,
                CurrentXP = user.Character.CurrentXP
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    private string GenerateJwtToken(Guid userId, string email, Guid characterId)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim("CharacterId", characterId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationMinutes"]!)),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
