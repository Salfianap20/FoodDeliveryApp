﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.Models;

namespace UserService.GraphQL
{
    public class Mutation
    {
        //Register User
        public async Task<UserData> RegisterUserAsync(
            RegisterUser input,
            [Service] Project1Context context)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };
           /* //Memberikan role buyer secara otomatis
            var memberRole = context.Roles.Where(m => m.Name == "BUYER").FirstOrDefault();
            if (memberRole == null)
                throw new Exception("Invalid Role");
            var userRole = new UserRole
            {
                RoleId = memberRole.Id,
                UserId = newUser.Id
            };
            newUser.UserRoles.Add(userRole);*/
            // EF
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }
        //Login
        public async Task<UserToken> LoginAsync(
            LoginUser input,
            [Service] IOptions<TokenSettings> tokenSettings, // setting token
            [Service] Project1Context context) // EF
        {
            var user = context.Users.Where(o => o.Username == input.Username).FirstOrDefault();
            if (user == null)
            {
                return await Task.FromResult(new UserToken(null, null, "Username or password was invalid"));
            }
            bool valid = BCrypt.Net.BCrypt.Verify(input.Password, user.Password);
            if (valid)
            {
                // generate jwt token
                var securitykey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSettings.Value.Key));
                var credentials = new SigningCredentials(securitykey, SecurityAlgorithms.HmacSha256);

                // jwt payload
                var claims = new List<Claim>();
                claims.Add(new Claim(ClaimTypes.Name, user.Username));

                var userRoles = context.UserRoles.Where(o => o.UserId == user.Id).ToList();
                foreach (var userRole in userRoles)
                {
                    var role = context.Roles.Where(o => o.Id == userRole.RoleId).FirstOrDefault();
                    if (role != null)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.Name));
                    }
                }

                var expired = DateTime.Now.AddHours(24);
                var jwtToken = new JwtSecurityToken(
                    issuer: tokenSettings.Value.Issuer,
                    audience: tokenSettings.Value.Audience,
                    expires: expired,
                    claims: claims, // jwt payload
                    signingCredentials: credentials // signature
                );

                return await Task.FromResult(
                    new UserToken(new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    expired.ToString(), null));
                //return new JwtSecurityTokenHandler().WriteToken(jwtToken);
            }

            return await Task.FromResult(new UserToken(null, null, Message: "Username or password was invalid"));
        }
        //Update 
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> UpdateUserAsync(
           UserData input,
           [Service] Project1Context context)
        {
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                user.FullName = input.FullName;
                user.Email = input.Email;
                user.Username = input.Username;

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(user);
        }
        //Delete
        [Authorize(Roles = new[] { "ADMIN" })]
        public async Task<User> DeleteUserByIdAsync(
            int id,
            [Service] Project1Context context)
        {
            var user = context.Users.Where(o => o.Id == id).Include(o => o.UserRoles).Include(o => o.Orders).FirstOrDefault();
            if (user != null)
            {
                context.Users.Remove(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(user);
        }
        //Change password by token
        [Authorize]
        public async Task<User> ChangePasswordByUserAsync(
           UserChangePassword input,
           [Service] Project1Context context)
        {
            var user = context.Users.Where(o => o.Id == input.Id).FirstOrDefault();
            if (user != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(input.Password);

                context.Users.Update(user);
                await context.SaveChangesAsync();
            }

            return await Task.FromResult(user);
        }
        //Add Profile User
        [Authorize]
        public async Task<Profile> AddProfileAsync(
            ProfilesInput input,
            [Service] Project1Context context)
        {
            //EF
            var profile = new Profile
            {
                UserId = input.UserId,
                Name = input.Name,
                Address = input.Address,
                City = input.City,
                Phone = input.Phone

            };

            var ret = context.Profiles.Add(profile);
            await context.SaveChangesAsync();

            return ret.Entity;
        }
        /*=====================================COURIER==========================================*/
        /*public async Task<UserData> AddCourierAsync(
            RegisterUser input,
            [Service] Project1Context context)
        {
            var user = context.Users.Where(o => o.Username == input.UserName).FirstOrDefault();
            if (user != null)
            {
                return await Task.FromResult(new UserData());
            }
            var newUser = new User
            {
                FullName = input.FullName,
                Email = input.Email,
                Username = input.UserName,
                Password = BCrypt.Net.BCrypt.HashPassword(input.Password) // encrypt password
            };
            //Memberikan role courier secara otomatis
             var memberRole = context.Roles.Where(m => m.Name == "COURIER").FirstOrDefault();
             if (memberRole == null)
                 throw new Exception("Invalid Role");
             var userRole = new UserRole
             {
                 RoleId = memberRole.Id,
                 UserId = newUser.Id
             };
             newUser.UserRoles.Add(userRole);
            // EF
            var ret = context.Users.Add(newUser);
            await context.SaveChangesAsync();

            return await Task.FromResult(new UserData
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                FullName = newUser.FullName
            });
        }*/

        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Courier> AddCourierAsync(
           CourierInput input,
            [Service] Project1Context context)
        {

            // EF
            var courier = new Courier
            {
                CourierName = input.CourierName,
                PhoneNumber = input.PhoneNumber
            };

            var ret = context.Couriers.Add(courier);
            await context.SaveChangesAsync();

            return ret.Entity;
        }
        //Update 
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Courier> UpdateCourierAsync(
            CourierInput input,
            [Service] Project1Context context)
        {
            var courier = context.Couriers.Where(o => o.Id == input.Id).FirstOrDefault();
            if (courier != null)
            {
                courier.CourierName = input.CourierName;
                courier.PhoneNumber = input.PhoneNumber;

                context.Couriers.Update(courier);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(courier);
        }

        //Delete
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Courier> DeleteCourierByIdAsync(
            int id,
            [Service] Project1Context context)
        {
            var courier = context.Couriers.Where(o => o.Id == id).FirstOrDefault();
            if (courier != null)
            {
                context.Couriers.Remove(courier);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(courier);
        }
    }
}
