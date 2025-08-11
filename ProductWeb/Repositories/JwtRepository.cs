using ProductWeb.Models;
using ProductWeb.Data;
using MongoDB.Driver;
using System.Security.Cryptography;

public class JwtRepository
{
    private readonly MongoDbContext _context;
    private readonly IMongoCollection<User> _users;

    public JwtRepository(MongoDbContext context)
    {
        _context = context;
        _users = _context.Collection<User>("Users");
    }

    public RefreshToken GenerateRefreshToken(string ipAddress, int daysValid = 30)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(daysValid),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = ipAddress
        };
    }

    public async Task AddRefreshTokenAsync(string userId, RefreshToken refreshToken)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Id, userId);
        var update = Builders<User>.Update.Push(u => u.RefreshTokens, refreshToken);
        await _users.UpdateOneAsync(filter, update);
    }

    public async Task<User?> GetByRefreshTokenAsync(string token)
    {
        var filter = Builders<User>.Filter.ElemMatch(u => u.RefreshTokens, rt =>
            rt.Token == token && rt.Revoked == false);

        return await _users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task RevokeRefreshTokenAsync(string userId, string token)
    {
        var filter = Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(u => u.Id, userId),
            Builders<User>.Filter.ElemMatch(u => u.RefreshTokens, rt => rt.Token == token));

        var update = Builders<User>.Update.Set("RefreshTokens.$.Revoked", true);
        await _users.UpdateOneAsync(filter, update);
    }

    public async Task ReplaceRefreshTokenAsync(string userId, string oldToken, RefreshToken newToken)
    {
        await RevokeRefreshTokenAsync(userId, oldToken);
        await AddRefreshTokenAsync(userId, newToken);
    }
}
