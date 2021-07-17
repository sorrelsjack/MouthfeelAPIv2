using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Models.Foods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Services
{
    public interface IIngredientsService
    {
        Task<IEnumerable<FoodIngredient>> GetIngredients(int foodId);
        Task<IEnumerable<FoodIngredient>> GetManyIngredients(IEnumerable<int> foodId);
        Task<IEnumerable<Ingredient>> SearchIngredients(string query);
    }

    public class IngredientsService : IIngredientsService
    {
        private readonly MouthfeelContext _mouthfeel;

        public IngredientsService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task<IEnumerable<FoodIngredient>> GetIngredients(int foodId)
        {
            var compositions = (await _mouthfeel.FoodCompositions.ToListAsync()).Where(i => i.FoodId == foodId);
            var ingredients = await _mouthfeel.Ingredients.ToListAsync();

            return ingredients.Join(compositions, ingredient => ingredient.Id, composition => composition.IngredientId, (ingredient, composition) => 
                new FoodIngredient
                {
                    Name = ingredient.Name,
                    Quantity = composition.Quantity
                });
        }

        public async Task<IEnumerable<FoodIngredient>> GetManyIngredients(IEnumerable<int> foodIds)
        {
            var compositions = (await _mouthfeel.FoodCompositions.ToListAsync()).Where(i => foodIds.Any(f => f == i.FoodId));
            var ingredients = await _mouthfeel.Ingredients.ToListAsync();

            return ingredients.Join(compositions, ingredient => ingredient.Id, composition => composition.IngredientId, (ingredient, composition) =>
                new FoodIngredient
                {
                    Name = ingredient.Name,
                    Quantity = composition.Quantity
                });
        }

        public async Task<IEnumerable<Ingredient>> SearchIngredients(string query) => _mouthfeel.Ingredients.Where(i => i.Name == query);
    }
}
