using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.Constants;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Enums;
using MouthfeelAPIv2.Extensions;
using MouthfeelAPIv2.Models;
using MouthfeelAPIv2.Models.Foods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using FoodSearchType = MouthfeelAPIv2.Constants.FoodSearchType;

// TODO: Maybe convert votes in DB back to tiny int? Then do a conversion between bytes and ints
namespace MouthfeelAPIv2.Services
{
    public interface IFoodsService
    {
        Task<FoodResponse> GetFoodDetails(int foodId, int userId);
        Task<IEnumerable<FoodResponse>> SearchFoods(string query, IEnumerable<string> searchFilter, int userId);
        Task AddFood(CreateFoodRequest request, int userId);
        Task<int> GetFoodSentiment(int foodId, int userId);
        Task<IEnumerable<FoodResponse>> GetLikedFoods(int userId);
        Task<IEnumerable<FoodResponse>> GetDislikedFoods(int userId);
        Task ManageFoodSentiment(int foodId, int userId, Sentiment newSentiment);
        Task<IEnumerable<FoodResponse>> GetFoodsToTry(int userId);
        Task<bool> GetFoodToTryStatus(int foodId, int userId);
        Task AddOrRemoveFoodToTry(int foodId, int userId);
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

        public async Task<FoodResponse> GetFoodDetails(int foodId, int userId)
        {
            var food = await _mouthfeel.Foods.FindAsync(foodId);
            var sentiment = await GetFoodSentiment(foodId, userId);
            var toTry = await GetFoodToTryStatus(foodId, userId);
            var ingredients = await _ingredients.GetIngredients(foodId);
            var textures = await _textures.GetTextureVotes(foodId);
            var flavors = await _flavors.GetFlavorVotes(foodId);
            var misc = await _misc.GetMiscellaneousVotes(foodId);

            if (food == null)
                throw new ErrorResponse(HttpStatusCode.NotFound, ErrorMessages.FoodNotFound);

            return new FoodResponse(food, sentiment, toTry, ingredients, flavors, textures, misc);
        }

        private async Task<IEnumerable<FoodResponse>> GetManyFoodDetails(IEnumerable<int> foodIds, int userId) =>
            await Task.WhenAll(foodIds.Select(f => GetFoodDetails(f, userId)));

        public async Task<IEnumerable<FoodResponse>> SearchFoods(string query, IEnumerable<string> searchFilter, int userId)
        {
            var foods = new Food[] { };
            var foodsWithDetails = new FoodResponse[] { };

            // If attributes, query attributes tables, get ids of attributes, then go to vote tables, then get food ids from matching records
            if (searchFilter.Contains(FoodSearchType.Attributes))
            {
                var flavors = await _flavors.SearchFlavors(query);
                var f = flavors
                    .Join(_mouthfeel.FlavorVotes, flavor => flavor.Id, vote => vote.FlavorId, (flavor, vote) => new { flavor.Id, flavor.Name, vote.FoodId })
                    .Join(_mouthfeel.Foods, fv => fv.FoodId, food => food.Id, (fv, food) => new Food { Id = food.Id, Name = food.Name, ImageUrl = food.ImageUrl });

                var textures = await _textures.SearchTextures(query);
                var t = textures
                    .Join(_mouthfeel.TextureVotes, texture => texture.Id, vote => vote.TextureId, (texture, vote) => new { texture.Id, texture.Name, vote.FoodId })
                    .Join(_mouthfeel.Foods, tv => tv.FoodId, food => food.Id, (tv, food) => new Food { Id = food.Id, Name = food.Name, ImageUrl = food.ImageUrl });

                var misc = await _misc.SearchMiscellaneous(query);
                var m = misc
                    .Join(_mouthfeel.MiscellaneousVotes, mis => mis.Id, vote => vote.MiscId, (mis, vote) => new { mis.Id, mis.Name, vote.FoodId })
                    .Join(_mouthfeel.Foods, mv => mv.FoodId, food => food.Id, (mv, food) => new Food { Id = food.Id, Name = food.Name, ImageUrl = food.ImageUrl });

                foods = foods.Concat(f).Concat(t).Concat(m).ToArray();
            }

            if (searchFilter.Contains(FoodSearchType.Ingredients))
            {
                var ingredients = await _ingredients.SearchIngredients(query);
                var compositions = (await _mouthfeel.FoodCompositions.ToListAsync()).Where(c => ingredients.Any(i => i.Id == c.IngredientId));
                var matchingFoods = (await _mouthfeel.Foods.ToListAsync()).Where(f => compositions.Any(c => c.FoodId == f.Id));
                foods = foods.Concat(matchingFoods).ToArray();
            }

            if (searchFilter.Contains(FoodSearchType.Name))
            {
                var byName = _mouthfeel.Foods.Where(f => f.Name == query);
                foods = foods.Concat(byName).ToArray();
            }

            foods = foods.DistinctBy(f => f.Name).ToArray();

            return await GetManyFoodDetails(foods.Select(f => f.Id), userId);
        }

        public async Task AddFood(CreateFoodRequest request, int userId)
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

            var flavorTasks = request.Flavors?.Select(f => _flavors.ManageFlavorVote(f, userId, foodId, VoteState.Up));
            var miscTasks = request.Miscellaneous?.Select(m => _misc.ManageMiscellaneousVote(m, userId, foodId, VoteState.Up));
            var textureTasks = request.Textures?.Select(t => _textures.ManageTextureVote(t, userId, foodId, VoteState.Up));

            // TODO: Probably need error handling here
            foreach (var flavor in flavorTasks)
                await flavor;

            foreach (var misc in miscTasks)
                await misc;

            foreach (var texture in textureTasks)
                await texture;
        }

        public async Task<int> GetFoodSentiment(int foodId, int userId) 
            => (await _mouthfeel.FoodSentiments.ToListAsync())
                .Where(s => s.UserId == userId)
                .Where(s => s.FoodId == foodId)?
                .FirstOrDefault()?.Sentiment ?? 0;

        private async Task<IEnumerable<FoodResponse>> GetFoodsBySentiment(int userId, Sentiment sentiment)
        {
            var foods = await _mouthfeel.Foods.ToListAsync();
            var f = foods
                .Join(_mouthfeel.FoodSentiments, food => food.Id, sentiment => sentiment.FoodId, (food, sentiment) => new { food.Id, food.Name, food.ImageUrl, sentiment.UserId, sentiment.Sentiment })
                .Where(f => f.UserId == userId)
                .Where(f => f.Sentiment == (int)sentiment);

            var detailsTask = f.Select(f => GetFoodDetails(f.Id, userId));
            var foodWithDetails = await Task.WhenAll(detailsTask);

            return foodWithDetails;
        }

        public async Task<IEnumerable<FoodResponse>> GetLikedFoods(int userId)
            => await GetFoodsBySentiment(userId, Sentiment.Liked);

        public async Task<IEnumerable<FoodResponse>> GetDislikedFoods(int userId)
            => await GetFoodsBySentiment(userId, Sentiment.Disliked);

        public async Task ManageFoodSentiment(int foodId, int userId, Sentiment newSentiment)
        {
            var sentiment = new FoodSentiment { FoodId = foodId, UserId = userId, Sentiment = (int)newSentiment };
            var existingSentimentFromUser = _mouthfeel.FoodSentiments.FirstOrDefault(s => s.FoodId == foodId && s.UserId == userId);

            if (existingSentimentFromUser != null)
            {
                if (newSentiment == Sentiment.Neutral)
                    _mouthfeel.FoodSentiments.Remove(existingSentimentFromUser);
                else
                    existingSentimentFromUser.Sentiment = (int)newSentiment;
            }

            else
                _mouthfeel.FoodSentiments.Add(sentiment);

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<IEnumerable<FoodResponse>> GetFoodsToTry(int userId)
        {
            var toTry = await (_mouthfeel.FoodsToTry.Where(f => f.UserId == userId)).ToListAsync();
            return await GetManyFoodDetails(toTry.Select(f => f.Id), userId);
        }

        public async Task<bool> GetFoodToTryStatus(int foodId, int userId)
        {
            var toTry = await (_mouthfeel.FoodsToTry.Where(f => f.UserId == userId)).ToListAsync();
            return toTry.Any(f => f.FoodId == foodId && f.UserId == userId);
        }

        public async Task AddOrRemoveFoodToTry(int foodId, int userId)
        {
            var toTry = _mouthfeel.FoodsToTry.Where(f => f.UserId == userId);
            var existing = toTry.FirstOrDefault(f => f.Id == foodId && f.UserId == userId);

            if (toTry.Any(f => f.Id == foodId)) _mouthfeel.FoodsToTry.Remove(existing);
            else _mouthfeel.FoodsToTry.Add(new FoodToTry 
            { 
                FoodId = foodId,
                UserId = userId
            });

            await _mouthfeel.SaveChangesAsync();
        }
    }
}
