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
using Attribute = MouthfeelAPIv2.DbModels.Attribute;
using FoodImage = MouthfeelAPIv2.Models.FoodImage;
using FoodSearchType = MouthfeelAPIv2.Constants.FoodSearchType;

namespace MouthfeelAPIv2.Services
{
    public interface IFoodsService
    {
        Task<bool> FoodExists(int foodId);
        Task<FoodResponse> GetFoodDetails(int foodId, int userId);
        Task<IEnumerable<FoodResponse>> SearchFoods(string query, IEnumerable<string> searchFilter, int userId);
        Task<FoodResponse> AddFood(CreateFoodRequest request, int userId);
        Task<int> GetFoodSentiment(int foodId, int userId);
        Task<IEnumerable<FoodResponse>> GetLikedFoods(int userId);
        Task<IEnumerable<FoodResponse>> GetDislikedFoods(int userId);
        Task<ManageFoodSentimentResponse> ManageFoodSentiment(int foodId, int userId, Sentiment newSentiment);
        Task<IEnumerable<FoodResponse>> GetFoodsToTry(int userId);
        Task<Dictionary<int, bool>> GetManyFoodToTryStatuses(IEnumerable<int> foodIds, int userId);
        Task<IEnumerable<FoodResponse>> GetRecommendedFoods(int userId);
        Task<bool> GetFoodToTryStatus(int foodId, int userId);
        Task AddOrRemoveFoodToTry(int foodId, int userId);
        Task<VotableAttribute> AddOrUpdateAttribute(AddOrUpdateVotableAttributeRequest request, int userId, VotableAttributeType type);
    }

    public class FoodsService : IFoodsService
    {
        private readonly IMouthfeelContextFactory _mouthfeelContextFactory;

        private readonly MouthfeelContext _mouthfeel;

        private readonly IIngredientsService _ingredients;

        private readonly IAttributesService _attributes;

        private readonly IImagesService _images;

        public FoodsService(
            IMouthfeelContextFactory mouthfeelContextFactory,
            IIngredientsService ingredients,
            IAttributesService attributes,
            IImagesService images
        )
        {
            _mouthfeelContextFactory = mouthfeelContextFactory;
            _mouthfeel = _mouthfeelContextFactory.CreateContext();
            _ingredients = ingredients;
            _attributes = attributes;
            _images = images;
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
            using (var scope = _mouthfeelContextFactory.CreateContext())
            {
                var food = await _mouthfeel.Foods.FindAsync(foodId);
                var images = await _images.DownloadImages(foodId);
                var sentiment = await GetFoodSentiment(foodId, userId);
                var toTry = await GetFoodToTryStatus(foodId, userId);
                var ingredients = await _ingredients.GetIngredients(foodId);
                var textures = await _attributes.GetVotes(foodId, userId, VotableAttributeType.Texture);
                var flavors = await _attributes.GetVotes(foodId, userId, VotableAttributeType.Flavor);
                var misc = await _attributes.GetVotes(foodId, userId, VotableAttributeType.Miscellaneous);

                if (food == null)
                    throw new ErrorResponse(HttpStatusCode.NotFound, ErrorMessages.FoodNotFound, DescriptiveErrorCodes.FoodNotFound);

                return new FoodResponse(food, images, sentiment, toTry, ingredients, flavors, textures, misc);
            }
        }

        private async Task<IEnumerable<FoodResponse>> GetManyFoodDetails(IEnumerable<int> foodIds, int userId) =>
            await Task.WhenAll(foodIds.Select(f => GetFoodDetails(f, userId)));

        private async Task<IEnumerable<FoodSummaryResponse>> GetManyFoodSummaries(IEnumerable<int> foodIds, int userId)
        {
            using (var scope = _mouthfeelContextFactory.CreateContext())
            {
                var joined = _mouthfeel.Foods.Where(f => foodIds.Contains(f.Id)).ToList()
                    .GroupJoin(_mouthfeel.FoodSentiments.AsEnumerable().Where(se => se.UserId == userId).ToList(), f => f.Id, s => s.FoodId, (food, sent) => new { Food = food, Sent = sent }).ToList()
                    .GroupJoin(_mouthfeel.FoodsToTry.AsEnumerable().Where(tT => tT.UserId == userId).ToList(), f => f.Food.Id, t => t.FoodId, (food, toTry) => new { food.Food, food.Sent, ToTry = toTry }).ToList()
                    .GroupJoin(_mouthfeel.FoodImages.AsEnumerable(), f => f.Food.Id, i => i.FoodId, (food, images) => new { food.Food, food.Sent, food.ToTry, Images = images.Select(im => new FoodImage { Id = im.Id, FoodId = im.FoodId, Image = im.Image, UserId = im.UserId  }) }).ToList();
                var topThrees = await _attributes.GetManyTopThrees(foodIds);

                if (!joined.Any())
                    throw new ErrorResponse(HttpStatusCode.NotFound, ErrorMessages.FoodNotFound, DescriptiveErrorCodes.FoodNotFound);

                return joined.Select(j => new FoodSummaryResponse
                (
                    j.Food, 
                    j.Images.Where(i => i.FoodId == j.Food.Id), 
                    j.Sent.FirstOrDefault(s => s.FoodId == j.Food.Id)?.Sentiment ?? 0, 
                    j.ToTry.Any(t => t.FoodId == j.Food.Id), 
                    topThrees.Where(kvp => kvp.Key == j.Food.Id).SelectMany(kvp => kvp.Value))
                );
            }
        }

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

            // TODO: fix 'second operation' error here
            if (searchFilter.Contains(FoodSearchType.Name))
            {
                var byName = (await _mouthfeel.Foods.ToListAsync()).Where(f => f.Name.ToLower().Contains(query.ToLower()));
                foods = foods.Concat(byName).ToArray();
            }

            foods = foods.DistinctBy(f => f.Name).ToArray();

            return await GetManyFoodDetails(foods.Select(f => f.Id), userId);
        }

        public async Task<FoodResponse> AddFood(CreateFoodRequest request, int userId)
        {
            var foods = await _mouthfeel.Foods.ToListAsync();

            if (foods.Any(f => String.Equals(f.Name, request.Name, StringComparison.OrdinalIgnoreCase))) 
                throw new ErrorResponse(HttpStatusCode.BadRequest, "A food with that name already exists.", DescriptiveErrorCodes.FoodAlreadyExists);

            if (request.Flavors?.Any() ?? false)
            {
                var flavors = await _attributes.GetAttributes(VotableAttributeType.Flavor);
                if (!request.Flavors.All(f => flavors.Select(fl => fl.Id).Contains(f)))
                    throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.FlavorDoesNotExist, DescriptiveErrorCodes.FlavorDoesNotExist);
            }

            if (request.Miscellaneous?.Any() ?? false)
            {
                var misc = await _attributes.GetAttributes(VotableAttributeType.Miscellaneous);
                if (!request.Miscellaneous.All(m => misc.Select(ms => ms.Id).Contains(m)))
                    throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.MiscellaneousDoesNotExist, DescriptiveErrorCodes.MiscellaneousDoesNotExist);
            }

            if (request.Textures?.Any() ?? false)
            {
                var textures = await _attributes.GetAttributes(VotableAttributeType.Texture);
                if (!request.Textures.All(t => textures.Select(tx => tx.Id).Contains(t)))
                    throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.TextureDoesNotExist, DescriptiveErrorCodes.TextureDoesNotExist);
            }

            var food = new Food 
            { 
                Name = request.Name, 
                ImageUrl = ""
            };

            _mouthfeel.Foods.Add(food);
            await _mouthfeel.SaveChangesAsync();

            var createdFood = await _mouthfeel.Foods.FirstOrDefaultAsync(f => f.Name == food.Name);
            var foodId = createdFood.Id;

            var imageRequest = new CreateFoodImageRequest
            {
                UserId = userId,
                FoodId = foodId,
                Image = request.Image
            };
            var image = await _images.UploadImage(imageRequest);

            var flavorTasks = request.Flavors?.Select(f => _attributes.ManageVote(f, userId, foodId, VotableAttributeType.Flavor));
            var miscTasks = request.Miscellaneous?.Select(m => _attributes.ManageVote(m, userId, foodId, VotableAttributeType.Miscellaneous));
            var textureTasks = request.Textures?.Select(t => _attributes.ManageVote(t, userId, foodId, VotableAttributeType.Texture));

            // TODO: Probably need error handling here
            if (flavorTasks != null)
            {
                foreach (var flavor in flavorTasks)
                    await flavor;
            }
            if (miscTasks != null)
            {
                foreach (var misc in miscTasks)
                    await misc;
            }
            if (textureTasks != null)
            {
                foreach (var texture in textureTasks)
                    await texture;
            }

            return await GetFoodDetails(foodId, userId);
        }

        public async Task<int> GetFoodSentiment(int foodId, int userId) 
            => _mouthfeel.FoodSentiments
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

        // TODO: This is throwing an error, which is very cool
        // TODO: Optimize this. It works, but it's slow
        public async Task<IEnumerable<FoodResponse>> GetRecommendedFoods(int userId)
        {
            IEnumerable<FoodResponse> GetCommonAttributes(IEnumerable<FoodResponse> source, IEnumerable<FoodResponse> toCompare)
            {
                var sourceFlavorIds = source.SelectMany(s => s.Flavors.Select(f => f.Id));
                var toCompareFlavorIds = toCompare.SelectMany(s => s.Flavors.Select(f => f.Id));

                var sourceTextureIds = source.SelectMany(s => s.Textures.Select(f => f.Id));
                var toCompareTextureIds = toCompare.SelectMany(s => s.Textures.Select(f => f.Id));

                var sourceMiscIds = source.SelectMany(s => s.Miscellaneous.Select(f => f.Id));
                var toCompareMiscIds = toCompare.SelectMany(s => s.Miscellaneous.Select(f => f.Id));

                var commonFlavorIds = sourceFlavorIds.Intersect(toCompareFlavorIds);
                var commonTextureIds = sourceTextureIds.Intersect(toCompareTextureIds);
                var commonMiscIds = sourceMiscIds.Intersect(toCompareMiscIds);

                var combinedFlavors = toCompare
                    .Where(a => a.Flavors.Any(f => commonFlavorIds.Contains(f.Id)));

                var combinedTextures = toCompare
                    .Where(a => a.Textures.Any(f => commonTextureIds.Contains(f.Id)));

                var combinedMisc = toCompare
                    .Where(a => a.Miscellaneous.Any(f => commonMiscIds.Contains(f.Id)));

                return combinedFlavors.Concat(combinedTextures).Concat(combinedMisc);
            }

            // TODO: 'second operation' bs
            var allFoods = await GetAllFoods(userId);

            var liked = allFoods.Where(f => f.Sentiment == (int)Sentiment.Liked);
            var disliked = allFoods.Where(f => f.Sentiment == (int)Sentiment.Disliked);
            var toTry = allFoods.Where(f => f.ToTry == true);

            var withoutSentiment = allFoods.Except(liked).Except(disliked).Except(toTry);

            var comparedWithLiked = GetCommonAttributes(liked, withoutSentiment);
            var comparedWithDisliked = GetCommonAttributes(disliked, withoutSentiment);

            return comparedWithLiked.Except(comparedWithDisliked);
        }

        public async Task<bool> GetFoodToTryStatus(int foodId, int userId)
        {
            var toTry = _mouthfeel.FoodsToTry.Where(f => f.UserId == userId);
            return toTry.Any(f => f.FoodId == foodId && f.UserId == userId);
        }

        public async Task<Dictionary<int, bool>> GetManyFoodToTryStatuses(IEnumerable<int> foodIds, int userId)
        {
            var records = new Dictionary<int, bool>();

            var forUser = await (_mouthfeel.FoodsToTry.Where(f => f.UserId == userId)).ToListAsync();
            var toTry = forUser.Where(f => foodIds.Contains(f.FoodId) && f.UserId == userId);

            foreach (var food in toTry)
            {
                records.Add(food.FoodId, true);
            }

            foreach (var id in foodIds)
            {
                if (!toTry.Any(t => t.FoodId == id)) records.Add(id, false);
            }

            return records;
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
