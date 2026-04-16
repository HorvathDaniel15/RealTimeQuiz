using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Claims;
using JWTOauth2.Core.Interfaces;
using JWTOauth2.Core.Options;
using Microsoft.AspNetCore.Identity;
using RealTimeQuiz.Logic.Contracts.Auth.Requests;
using RealTimeQuiz.Logic.Contracts.Auth.Responses;
using RealTimeQuiz.Logic.Exceptions;
using RealTimeQuiz.Logic.Services.Interfaces;
using RealTimeQuiz.Model.Entities;

namespace RealTimeQuiz.Logic.Services.Implementations;

public class AuthService : IAuthService
{
    private const int MinPasswordLength = 8;
    
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenStore _refreshTokenStore;
    private readonly JwtAuthOptions _jwtOptions;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IRefreshTokenStore refreshTokenStore,
        JwtAuthOptions jwtOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _refreshTokenStore = refreshTokenStore;
        _jwtOptions = jwtOptions;
    }
    
    
    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<RefreshResult> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private static void ValidateRegisterRequest(RegisterRequest request)
    {
        if (request is null)
        {
            throw new BusinessValidationException("Request is required.");
        }
        
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }
        
        else if (!new EmailAddressAttribute().IsValid(request.Email.Trim()))
        {
            errors.Add("Email format is invalid.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }

        else if (request.Password.Length < MinPasswordLength)
        {
            errors.Add($"Password must be at least {MinPasswordLength} characters.");
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }
    }

    private static void ValidateLoginRequest(LoginRequest request)
    {
        if (request is null)
        {
            throw new BusinessValidationException("Request is required.");
        }
        
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("Password is required.");
        }

        if (errors.Count > 0)
        {
            throw new BusinessValidationException(errors);
        }
    }

    private async Task<IReadOnlyList<Claim>> BuildUserClaimsAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email));
        }
        
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        
        return claims;
    }
}