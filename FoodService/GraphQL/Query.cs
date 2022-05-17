using System.Security.Claims;
using HotChocolate.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using FoodService.Models;

namespace FoodService.GraphQL
{
    public class Query
    {
        [Authorize]
        public IQueryable<Food> GetFoods([Service] Project1Context context, ClaimsPrincipal claimsPrincipal)
        {
            var userName = claimsPrincipal.Identity.Name;

            // check manager role ?
            var managerRole = claimsPrincipal.Claims.Where(o => o.Type == ClaimTypes.Role && o.Value == "MANAGER").FirstOrDefault();
            var product = context.Users.Where(o => o.Username == userName).FirstOrDefault();
            if (product != null)
            {
                if (managerRole != null)
                    return context.Foods;

                var products = context.Foods.Where(o => o.Id == product.Id);
                return products.AsQueryable();
            }
            return new List<Food>().AsQueryable();
        }
    }
}
