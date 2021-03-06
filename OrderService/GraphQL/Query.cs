using System.Security.Claims;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using OrderService.Models;

namespace OrderService.GraphQL
{
    public class Query
    {
        [Authorize(Roles = new[] {"MANAGER"})]
        public IQueryable<Order> GetOrder([Service] Project1Context context) =>
            context.Orders.Include(o => o.OrderDetails);

        [Authorize]
        public IQueryable<Order> GetOrdersByToken([Service] Project1Context context, ClaimsPrincipal claimsPrincipal)
        {
            var username = claimsPrincipal.Identity.Name;
            var user = context.Users.Where(o => o.Username == username).FirstOrDefault();
            if (user != null)
            {
                var orders = context.Orders.Where(o => o.UserId == user.Id).Include(o=>o.OrderDetails);
                return orders.AsQueryable();
            }
            return new List<Order>().AsQueryable();
        }
        [Authorize(Roles = new[] { "BUYER" })]
        public IQueryable<Order> GetTracking([Service] Project1Context context) =>
               context.Orders;
    }
}
