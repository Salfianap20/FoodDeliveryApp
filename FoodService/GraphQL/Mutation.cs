using HotChocolate.AspNetCore.Authorization;
using FoodService.Models;

namespace FoodService.GraphQL
{
    public class Mutation
    {
        //Add
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Food> AddFoodAsync(
           FoodInput input,
            [Service] Project1Context context)
        {

            // EF
            var food = new Food
            {
                Name = input.Name,
                Stock = input.Stock,
                Price = input.Price,
                Created = DateTime.Now
            };

            var ret = context.Foods.Add(food);
            await context.SaveChangesAsync();

            return ret.Entity;
        }
        //Update 
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Food> UpdateFoodAsync(
            FoodInput input,
            [Service] Project1Context context)
        {
            var food = context.Foods.Where(o => o.Id == input.Id).FirstOrDefault();
            if (food != null)
            {
                food.Name = input.Name;
                food.Stock = input.Stock;
                food.Price = input.Price;

                context.Foods.Update(food);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(food);
        }

        //Delete
        [Authorize(Roles = new[] { "MANAGER" })]
        public async Task<Food> DeleteFoodByIdAsync(
            int id,
            [Service] Project1Context context)
        {
            var food = context.Foods.Where(o => o.Id == id).FirstOrDefault();
            if (food != null)
            {
                context.Foods.Remove(food);
                await context.SaveChangesAsync();
            }
            return await Task.FromResult(food);
        }
    }
}
