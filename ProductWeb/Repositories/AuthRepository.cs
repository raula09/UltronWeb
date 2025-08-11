using ProductWeb.Models;
using ProductWeb.Services;
using ProductWeb.Data;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace ProductWeb.Repositories
{
    public class AuthRepository
    {
        private readonly MongoDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        private readonly IMongoCollection<User> _users;

        public AuthRepository(
            MongoDbContext context,
            IPasswordHasher<User> passwordHasher,
            JwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;

            _users = _context.Collection<User>("Users");
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _users.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<string> RegisterAsync(string name, string email, string password, string role = "User")
        {
            var existingUser = await GetUserByEmailAsync(email);
            if (existingUser != null)
                throw new Exception("User with that email already exists.");

            var user = new User
            {
                Username = name,
                Email = email,
                Role = role,
                PasswordHash = _passwordHasher.HashPassword(null, password),
                CreatedAt = DateTime.UtcNow
            };

            await _users.InsertOneAsync(user);

            return _jwtTokenGenerator.GenerateToken(user);
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                throw new Exception("Invalid email or password.");

            var result = _passwordHasher.VerifyHashedPassword(null, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
                throw new Exception("Invalid email or password.");

            return _jwtTokenGenerator.GenerateToken(user);
        }

        public async Task<bool> PromoteToAdminAsync(string email)
        {
            var user = await GetUserByEmailAsync(email);
            if (user == null)
                return false;

            var update = Builders<User>.Update.Set(u => u.Role, "Admin");
            await _users.UpdateOneAsync(u => u.Email == email, update);
            return true;
        }
    }
}
