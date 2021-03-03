using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.Constants;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Enums;
using MouthfeelAPIv2.Models;
using MouthfeelAPIv2.Models.Foods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace MouthfeelAPIv2.Services
{
    public interface IFoodsService
    {
        Task<FoodResponse> GetFoodDetails(int id);
        Task AddFood(CreateFoodRequest request);
    }

    public class FoodsService : IFoodsService
    {
        private readonly MouthfeelContext _mouthfeel;

        private readonly IIngredientsService _ingredients;

        private readonly IFlavorsService _flavors;

        private readonly IMiscellaneousService _misc;

        private readonly ITexturesService _textures;

        public FoodsService(
            MouthfeelContext mouthfeel,
            IIngredientsService ingredients,
            IFlavorsService flavors,
            IMiscellaneousService misc,
            ITexturesService textures
        )
        {
            _mouthfeel = mouthfeel;
            _ingredients = ingredients;
            _flavors = flavors;
            _misc = misc;
            _textures = textures;
        }

        public async Task<FoodResponse> GetFoodDetails(int id)
        {
            var food = await _mouthfeel.Foods.FindAsync(id);
            var ingredients = await _ingredients.GetIngredients(id);
            var textures = await _textures.GetTextureVotes(id);
            var flavors = await _flavors.GetFlavorVotes(id);
            var misc = await _misc.GetMiscellaneousVotes(id);

            if (food == null)
                throw new ErrorResponse(HttpStatusCode.NotFound, ErrorMessages.FoodNotFound);

            return new FoodResponse(food, ingredients, flavors, textures, misc);
        }

        public async Task AddFood(CreateFoodRequest request)
        {
            var foods = await _mouthfeel.Foods.ToListAsync();

            if (foods.Any(f => String.Equals(f.Name, request.Name, StringComparison.OrdinalIgnoreCase))) 
                throw new ErrorResponse(HttpStatusCode.BadRequest, "A food with that name already exists.");

            if (request.Flavors.Any())
            {
                var flavors = await _mouthfeel.Flavors.ToListAsync();
                if (!request.Flavors.All(f => flavors.Select(fl => fl.Id).Contains(f)))
                    throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.FlavorDoesNotExist);
            }

            if (request.Miscellaneous.Any())
            {
                var misc = await _mouthfeel.Miscellaneous.ToListAsync();
                if (!request.Miscellaneous.All(m => misc.Select(ms => ms.Id).Contains(m)))
                    throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.MiscellaneousDoesNotExist);
            }

            if (request.Textures.Any())
            {
                var textures = await _mouthfeel.Textures.ToListAsync();
                if (!request.Textures.All(t => textures.Select(tx => tx.Id).Contains(t)))
                    throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.TextureDoesNotExist);
            }

            var food = new Food 
            { 
                Name = request.Name, 
                ImageUrl = request.ImageUrl 
            };

            _mouthfeel.Foods.Add(food);
            _mouthfeel.SaveChanges();

            var foodId = (await _mouthfeel.Foods.FirstOrDefaultAsync(f => f.Name == food.Name)).Id;

            // TODO: Change userId later
            var flavorTasks = request.Flavors?.Select(f => _flavors.ManageFlavorVote(f, 1, foodId, VoteState.Up));
            var miscTasks = request.Miscellaneous?.Select(m => _misc.ManageMiscellaneousVote(m, 1, foodId, VoteState.Up));
            var textureTasks = request.Textures?.Select(t => _textures.ManageTextureVote(t, 1, foodId, VoteState.Up));

            // TODO: Probably need error handling here
            foreach (var flavor in flavorTasks)
                await flavor;

            foreach (var misc in miscTasks)
                await misc;

            foreach (var texture in textureTasks)
                await texture;
        }
    }
}
