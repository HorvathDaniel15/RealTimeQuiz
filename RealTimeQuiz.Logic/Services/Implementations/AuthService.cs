using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
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
        ValidateRegisterRequest(request);
        
        var email = request.Email.Trim();
        var existnig = await _userManager.FindByEmailAsync(email);
        if (existnig is not null)
        {
            throw new BusinessValidationException("A user with this email already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = string.IsNullOrWhiteSpace(request.UserName) ? email : request.UserName,
            Email = request.Email,
        };
        
        var createResult = await _userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            throw new BusinessValidationException(createResult.Errors.Select(e => e.Description));
        }

        return new RegisterResult
        {
            UserId = user.Id,
            Email = user.Email ?? email,
        };
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ValidateLoginRequest(request);
        
        var email = request.Email.Trim();
        var user = await _userManager.FindByEmailAsync(email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            throw new ForbiddenOperationException("Invalid credentials.");
        }
        
        var claims = await BuildUserClaimsAsync(user);
        var tokenPair = _tokenService.GenerateTokenPair(claims, user.Id);

        await _refreshTokenStore.StoreAsync(
            user.Id,
            tokenPair.RefreshTokenJti,
            DateTime.UtcNow.Add(_jwtOptions.RefreshTokenExpiration));

        return new LoginResult
        {
            AccessToken = tokenPair.AccessToken,
            RefreshToken = tokenPair.RefreshToken,
            AccessTokenExpiresAtUtc = tokenPair.AccessTokenExpiresAt,
        };
    }

    public async Task<RefreshResult> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new BusinessValidationException("Refresh token is required.");
        }
        
        var principal = _tokenService.ValidateRefreshToken(request.RefreshToken);
        if (principal is null)
        {
            throw new ForbiddenOperationException("Invalid or expired refresh token.");
        }
        
        var userId =
            principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            principal.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var oldJti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(oldJti))
        {
            throw new ForbiddenOperationException("Invalid refresh token claims.");
        }
        
        var stillValid = await _refreshTokenStore.IsValidAsync(userId, oldJti);
        if (!stillValid)
        {
            await _refreshTokenStore.RevokeAllForUserAsync(userId);
            throw new ForbiddenOperationException("Refresh token reuse detected. All session revoked.");
        }
        
        await _refreshTokenStore.RevokeAsync(userId, oldJti);
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new ForbiddenOperationException("User no longer exists.");
        }
        
        var claims = await BuildUserClaimsAsync(user);
        var newTokenPair = _tokenService.GenerateTokenPair(claims, user.Id);
        
        await _refreshTokenStore.StoreAsync(
            user.Id,
            newTokenPair.RefreshTokenJti,
            DateTime.UtcNow.Add(_jwtOptions.RefreshTokenExpiration));

        return new RefreshResult
        {
            AccessToken = newTokenPair.AccessToken,
            RefreshToken = newTokenPair.RefreshToken,
            AccessTokenExpiresAtUtc = newTokenPair.AccessTokenExpiresAt,
        };
    }

    public async Task LogoutAsync(string currentUserId, LogoutRequest request, CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(currentUserId))
        {
            throw new BusinessValidationException("User ID is required.");
        }

        if (request.LogoutAllDevices)
        {
            await _refreshTokenStore.RevokeAllForUserAsync(currentUserId);
            return;
        }

        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            throw new BusinessValidationException("Refresh token is required.");
        }
        
        var principal = _tokenService.ValidateRefreshToken(request.RefreshToken);
        if (principal is null)
        {
            throw new ForbiddenOperationException("Invalid or expired refresh token.");
        }
        
        var userId =
            principal.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
            principal.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var jti = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(jti))
        {
            throw new ForbiddenOperationException("Invalid refresh token claims.");
        }

        if (!string.Equals(userId, currentUserId, StringComparison.Ordinal))
        {
            throw new ForbiddenOperationException("Cannot revoke token of another user.");
        }

        await _refreshTokenStore.RevokeAsync(userId, jti);
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