using System.Security.Claims;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.GraphQL
{
    public class Query
    {
        [Authorize (Roles = new[] { "ADMIN" })] // dapat diakses kalau sudah login
        public IQueryable<UserData> GetUsers([Service] Project1Context context) =>
            context.Users.Select(p => new UserData()
            {
                Id = p.Id,
                FullName = p.FullName,
                Email = p.Email,
                Username = p.Username
            });
    }
}
