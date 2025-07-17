using Microsoft.EntityFrameworkCore;
using PrepenAPI.Data;
using PrepenAPI.DTOs;
using PrepenAPI.Models;

namespace PrepenAPI.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<bool> SuspendUserAsync(int id, bool suspend);
        Task<bool> DeleteUserAsync(int id);
    }

    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    IsSuspended = u.IsSuspended,
                    IsDeleted = u.IsDeleted
                })
                .ToListAsync();

            return users;
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id && !u.IsDeleted)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    IsSuspended = u.IsSuspended,
                    IsDeleted = u.IsDeleted
                })
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task<bool> SuspendUserAsync(int id, bool suspend)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                return false;

            user.IsSuspended = suspend;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);

            if (user == null)
                return false;

            user.IsDeleted = true; // Soft delete
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
