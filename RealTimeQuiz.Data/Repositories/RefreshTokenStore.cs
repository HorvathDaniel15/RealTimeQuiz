using JWTOauth2.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace RealTimeQuiz.Data.Repositories;

public class RefreshTokenStore : IRefreshTokenStore
{
    private const string RefreshTokenProvider = "JWTOauth2";
    private readonly AppDbContext _dbContext;

    public RefreshTokenStore(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task StoreAsync(string userId, string jti, DateTime expiresAt)
    {
        var expiresAtUtc = expiresAt.Kind == DateTimeKind.Utc
            ? expiresAt
            : expiresAt.ToUniversalTime();
        
        var userTokenSet = _dbContext.Set<IdentityUserToken<string>>();

        var existing = await userTokenSet
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.LoginProvider == RefreshTokenProvider &&
                x.Name == jti);

        if (existing is null)
        {
            userTokenSet.Add(new IdentityUserToken<string>
            {
                UserId = userId,
                LoginProvider = RefreshTokenProvider,
                Name = jti,
                Value = expiresAtUtc.ToString("O")
            });
        }
        else
        {
            existing.Value = expiresAtUtc.ToString("O");
        }
        
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> IsValidAsync(string userId, string jti)
    {
        var userTokenSet = _dbContext.Set<IdentityUserToken<string>>();

        var token = await userTokenSet
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.LoginProvider == RefreshTokenProvider &&
                x.Name == jti);

        if (token is null || string.IsNullOrWhiteSpace(token.Value))
        {
            return false;
        }

        if (!DateTime.TryParse(
                token.Value,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out var expiresAt))
        {
            return false;
        }
        
        var expiresAtUtc = expiresAt.Kind == DateTimeKind.Utc
            ? expiresAt
            : expiresAt.ToUniversalTime();
        
        return expiresAtUtc > DateTime.UtcNow;
    }

    public async Task RevokeAsync(string userId, string jti)
    {
        var userTokenSet = _dbContext.Set<IdentityUserToken<string>>();

        var token = await userTokenSet
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.LoginProvider == RefreshTokenProvider &&
                x.Name == jti);
        
        if (token is null)
        {
            return;
        }
        
        userTokenSet.Remove(token);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(string userId)
    {
        var userTokenSet = _dbContext.Set<IdentityUserToken<string>>();
        
        var tokens = await userTokenSet
            .Where(x=>
                x.UserId == userId &&
                x.LoginProvider == RefreshTokenProvider)
            .ToListAsync();

        if (tokens.Count == 0)
        {
            return;
        }
        
        userTokenSet.RemoveRange(tokens);
        await _dbContext.SaveChangesAsync();
    }
}