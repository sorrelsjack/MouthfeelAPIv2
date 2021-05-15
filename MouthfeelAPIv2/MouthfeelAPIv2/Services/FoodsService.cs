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

namespace MouthfeelAPIv2.Services
{
    public interface IFoodsService
    {
        Task<bool> FoodExists(int foodId);
        Task<FoodResponse> GetFoodDetails(int foodId, int userId);
        Task<IEnumerable<FoodResponse>> SearchFoods(string query, IEnumerable<string> searchFilter, int userId);
        Task AddFood(CreateFoodRequest request, int userId);
        Task<int> GetFoodSentiment(int foodId, int userId);
        Task<IEnumerable<FoodResponse>> GetLikedFoods(int userId);
        Task<IEnumerable<FoodResponse>> GetDislikedFoods(int userId);
        Task<ManageFoodSentimentResponse> ManageFoodSentiment(int foodId, int userId, Sentiment newSentiment);
        Task<IEnumerable<FoodResponse>> GetFoodsToTry(int userId);
        Task<IEnumerable<FoodResponse>> GetRecommendedFoods(int userId);
        Task<bool> GetFoodToTryStatus(int foodId, int userId);
        Task AddOrRemoveFoodToTry(int foodId, int userId);
        Task<VotableAttribute> AddOrUpdateAttribute(AddOrUpdateVotableAttributeRequest request, int userId, VotableAttributeType type);
    }

    public class FoodsService : IFoodsService
    {
        private readonly MouthfeelContext _mouthfeel;

        private readonly IIngredientsService _ingredients;

        private readonly IAttributesService _attributes;

        public FoodsService(
            MouthfeelContext mouthfeel,
            IIngredientsService ingredients,
            IAttributesService attributes
        )
        {
            _mouthfeel = mouthfeel;
            _ingredients = ingredients;
            _attributes = attributes;
        }

        public async Task<bool> FoodExists(int foodId)
            => (await _mouthfeel.Foods.ToListAsync()).Any(f => f.Id == foodId);

        private async Task<IEnumerable<FoodResponse>> GetAllFoods(int userId)
        {
            var foodIds = await _mouthfeel.Foods.Select(f => f.Id).ToListAsync();
            return await GetManyFoodDetails(foodIds, userId);
        }

        public async Task<FoodResponse> GetFoodDetails(int foodId, int userId)
        {
            var food = await _mouthfeel.Foods.FindAsync(foodId);
            var sentiment = await GetFoodSentiment(foodId, userId);
            var toTry = await GetFoodToTryStatus(foodId, userId);
            var ingredients = await _ingredients.GetIngredients(foodId);
            var textures = await _attributes.GetVotes(foodId, userId, VotableAttributeType.Texture);
            var flavors = await _attributes.GetVotes(foodId, userId, VotableAttributeType.Flavor);
            var misc = await _attributes.GetVotes(foodId, userId, VotableAttributeType.Miscellaneous);

            if (food == null)
                throw new ErrorResponse(HttpStatusCode.NotFound, ErrorMessages.FoodNotFound, DescriptiveErrorCodes.FoodNotFound);

            return new FoodResponse(food, sentiment, toTry, ingredients, flavors, textures, misc);
        }

        private async Task<IEnumerable<FoodResponse>> GetManyFoodDetails(IEnumerable<int> foodIds, int userId) =>
            await Task.WhenAll(foodIds.Select(f => GetFoodDetails(f, userId)));

        /*private async Task<FoodSummaryResponse> GetFoodSummary(int foodId, int userId)
        {
            var food = await _mouthfeel.Foods.FindAsync(foodId);
            var sentiment = await GetFoodSentiment(foodId, userId);
            var toTry = await GetFoodToTryStatus(foodId, userId);
            var topThree = await _attributes.GetTopThree(foodId, userId);

            if (food == null)
                throw new ErrorResponse(HttpStatusCode.NotFound, ErrorMessages.FoodNotFound, DescriptiveErrorCodes.FoodNotFound);

            return new FoodSummaryResponse(food, sentiment, toTry, topThree);
        }*/

        public async Task<IEnumerable<FoodResponse>> SearchFoods(string query, IEnumerable<string> searchFilter, int userId)
        {
            var foods = new Food[] { };
            var foodsWithDetails = new FoodResponse[] { };

            // If attributes, query attributes tables, get ids of attributes, then go to vote tables, then get food ids from matching records
            if (searchFilter.Contains(FoodSearchType.Attributes))
            {
                var flavors = await _attributes.SearchAttributes(query, VotableAttributeType.Flavor);
                var f = flavors
                    .Join(_mouthfeel.AttributeVotes, flavor => flavor.Id, vote => vote.AttributeId, (flavor, vote) => new { flavor.Id, flavor.Name, vote.FoodId })
                    .Join(_mouthfeel.Foods, fv => fv.FoodId, food => food.Id, (fv, food) => new Food { Id = food.Id, Name = food.Name, ImageUrl = food.ImageUrl });

                var textures = await _attributes.SearchAttributes(query, VotableAttributeType.Texture);
                var t = textures
                    .Join(_mouthfeel.AttributeVotes, texture => texture.Id, vote => vote.AttributeId, (texture, vote) => new { texture.Id, texture.Name, vote.FoodId })
                    .Join(_mouthfeel.Foods, tv => tv.FoodId, food => food.Id, (tv, food) => new Food { Id = food.Id, Name = food.Name, ImageUrl = food.ImageUrl });

                var misc = await _attributes.SearchAttributes(query, VotableAttributeType.Miscellaneous);
                var m = misc
                    .Join(_mouthfeel.AttributeVotes, mis => mis.Id, vote => vote.AttributeId, (mis, vote) => new { mis.Id, mis.Name, vote.FoodId })
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
                throw new ErrorResponse(HttpStatusCode.BadRequest, "A food with that name already exists.", DescriptiveErrorCodes.FoodAlreadyExists);

            if (request.Flavors.Any())
            {
                var flavors = await _attributes.GetAttributes(VotableAttributeType.Flavor);
                if (!request.Flavors.All(f => flavors.Select(fl => fl.Id).Contains(f)))
                    throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.FlavorDoesNotExist, DescriptiveErrorCodes.FlavorDoesNotExist);
            }

            if (request.Miscellaneous.Any())
            {
                var misc = await _attributes.GetAttributes(VotableAttributeType.Miscellaneous);
                if (!request.Miscellaneous.All(m => misc.Select(ms => ms.Id).Contains(m)))
                    throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.MiscellaneousDoesNotExist, DescriptiveErrorCodes.MiscellaneousDoesNotExist);
            }

            if (request.Textures.Any())
            {
                var textures = await _attributes.GetAttributes(VotableAttributeType.Texture);
                if (!request.Textures.All(t => textures.Select(tx => tx.Id).Contains(t)))
                    throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.TextureDoesNotExist, DescriptiveErrorCodes.TextureDoesNotExist);
            }

            var food = new Food 
            { 
                Name = request.Name, 
                ImageUrl = request.ImageUrl 
            };

            _mouthfeel.Foods.Add(food);
            await _mouthfeel.SaveChangesAsync();

            var foodId = (await _mouthfeel.Foods.FirstOrDefaultAsync(f => f.Name == food.Name)).Id;

            var flavorTasks = request.Flavors?.Select(f => _attributes.ManageVote(f, userId, foodId, VotableAttributeType.Flavor));
            var miscTasks = request.Miscellaneous?.Select(m => _attributes.ManageVote(m, userId, foodId, VotableAttributeType.Miscellaneous));
            var textureTasks = request.Textures?.Select(t => _attributes.ManageVote(t, userId, foodId, VotableAttributeType.Texture));

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

            var foodWithDetails = Enumerable.Empty<FoodResponse>();

            foreach (var details in detailsTask)
            {
                var res = await details;
                foodWithDetails = foodWithDetails.Append(res);
            }

            return foodWithDetails;
        }

        public async Task<IEnumerable<FoodResponse>> GetLikedFoods(int userId)
            => await GetFoodsBySentiment(userId, Sentiment.Liked);

        public async Task<IEnumerable<FoodResponse>> GetDislikedFoods(int userId)
            => await GetFoodsBySentiment(userId, Sentiment.Disliked);

        public async Task<ManageFoodSentimentResponse> ManageFoodSentiment(int foodId, int userId, Sentiment newSentiment)
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
            return new ManageFoodSentimentResponse { FoodId = foodId, UserId = userId, Sentiment = newSentiment };
        }

        public async Task<IEnumerable<FoodResponse>> GetFoodsToTry(int userId)
        {
            var toTry = await (_mouthfeel.FoodsToTry.Where(f => f.UserId == userId)).ToListAsync();
            return await GetManyFoodDetails(toTry.Select(f => f.FoodId), userId);
        }

        public async Task<IEnumerable<FoodResponse>> GetRecommendedFoods(int userId)
        {
            IEnumerable<FoodResponse> GetCommonAttributes(IEnumerable<FoodResponse> source, IEnumerable<FoodResponse> toCompare)
            {
                var sourceIds = source.SelectMany(s => s.Flavors.Select(f => f.Id));
                var toCompareIds = source.SelectMany(s => s.Flavors.Select(f => f.Id));

                var common = sourceIds.Intersect(toCompareIds);

                // do same for textures and misc

                return null;
            }

            var allFoods = await GetAllFoods(userId);

            var liked = allFoods.Where(f => f.Sentiment == (int)Sentiment.Liked);
            var disliked = allFoods.Where(f => f.Sentiment == (int)Sentiment.Disliked);

            var withoutSentiment = allFoods.Except(liked).Except(disliked);

            return withoutSentiment;
            // TODO: Compare with other foods. Consider recommending a food if it has at least one attribute in common. However, if a food contains a combo of attributes that a previously disliked food has, abort
        }

        public async Task<bool> GetFoodToTryStatus(int foodId, int userId)
        {
            var toTry = await (_mouthfeel.FoodsToTry.Where(f => f.UserId == userId)).ToListAsync();
            return toTry.Any(f => f.FoodId == foodId && f.UserId == userId);
        }

        public async Task AddOrRemoveFoodToTry(int foodId, int userId)
        {
            var toTry = _mouthfeel.FoodsToTry.Where(f => f.UserId == userId);
            var existing = toTry.FirstOrDefault(f => f.FoodId == foodId);

            if (existing != null) _mouthfeel.FoodsToTry.Remove(existing);
            else _mouthfeel.FoodsToTry.Add(new FoodToTry 
            { 
                FoodId = foodId,
                UserId = userId
            });

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<VotableAttribute> AddOrUpdateAttribute(AddOrUpdateVotableAttributeRequest request, int userId, VotableAttributeType type)
            => await _attributes.ManageVote(request.AttributeId, userId, request.FoodId, type);
    }
}
