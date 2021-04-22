using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.Constants;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Enums;
using MouthfeelAPIv2.Extensions;
using MouthfeelAPIv2.Helpers;
using MouthfeelAPIv2.Models;
using MouthfeelAPIv2.Models.Foods;
using MouthfeelAPIv2.Services;

namespace MouthfeelAPIv2.Controllers
{
    [Authorize]
    [Route("api/foods")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        private readonly MouthfeelContext _context;

        public FoodsController(MouthfeelContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Food>>> GetFoods()
        {
            return await _context.Foods.OrderBy(f => f.Name).ToListAsync();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<FoodResponse>>> SearchFoods
        (
            [FromServices] IFoodsService foodsService,
            [FromQuery] string query,
            [FromQuery] string filter
        )
        {
            var searchTypes = FoodSearchType.GetAllTypes();
            var searchFilter = filter.Split(",").Where(f => FoodSearchType.GetAllTypes().Contains(f.ToLower())).ToList();

            return (await foodsService.SearchFoods(query, searchFilter, IdentityHelper.GetIdFromUser(User))).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FoodResponse>> GetFood
        (
            [FromServices] IFoodsService foodsService,
            int id
        ) => await foodsService.GetFoodDetails(id, IdentityHelper.GetIdFromUser(User));

        [HttpGet("liked")]
        public async Task<ActionResult<IEnumerable<FoodResponse>>> GetLikedFoods
        (
            [FromServices] IFoodsService foodsService
        ) => (await foodsService.GetLikedFoods(IdentityHelper.GetIdFromUser(User))).ToList();

        [HttpGet("disliked")]
        public async Task<ActionResult<IEnumerable<FoodResponse>>> GetDislikedFoods
        (
            [FromServices] IFoodsService foodsService
        ) => (await foodsService.GetDislikedFoods(IdentityHelper.GetIdFromUser(User))).ToList();

        [HttpPost("sentiment")]
        public async Task<ActionResult> ManageFoodSentiment
        (
            [FromServices] IFoodsService foodsService,
            [FromBody] ManageFoodSentimentRequest request
        )
        {
            await foodsService.ManageFoodSentiment(request.FoodId, IdentityHelper.GetIdFromUser(User), request.Sentiment);
            return NoContent();
        }

        // TODO; Recommended
        [HttpGet("recommended")]
        public async Task<ActionResult> GetRecommendedFoods
        (
            [FromServices] IFoodsService foodsService
        )
        {
            return null;
        }

        [HttpGet("to-try")]
        public async Task<ActionResult<IEnumerable<FoodResponse>>> GetFoodsToTry
        (
            [FromServices] IFoodsService foodsService
        ) => (await foodsService.GetFoodsToTry(IdentityHelper.GetIdFromUser(User))).ToList();

        [HttpPost("to-try")]
        public async Task<ActionResult> AddFoodToTry
        (
            [FromServices] IFoodsService foodsService,
            [FromBody] FoodToTry food
        )
        {
            food.UserId = IdentityHelper.GetIdFromUser(User);
            await foodsService.AddOrRemoveFoodToTry(food.FoodId, food.UserId);
            return NoContent();
        }

        [HttpDelete("to-try")]
        public async Task<ActionResult> DeleteFoodToTry
        (
            [FromServices] IFoodsService foodsService,
            [FromBody] FoodToTry food
        )
        {
            food.UserId = IdentityHelper.GetIdFromUser(User);
            await foodsService.AddOrRemoveFoodToTry(food.FoodId, food.UserId);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Food>> PostFood(
            [FromServices] IFoodsService foodsService,
            [FromBody] CreateFoodRequest food
        )
        {
            if (food.Name.IsNullOrWhitespace()) return BadRequest("A name must be entered.");
            if (food.ImageUrl.IsNullOrWhitespace()) return BadRequest("An image URL must be associated with a food.");

            await foodsService.AddFood(food, IdentityHelper.GetIdFromUser(User));
            return NoContent();
        }

        [HttpPost("{id}/flavors")]
        public async Task<ActionResult<VotableAttribute>> AddOrUpdateFoodFlavor
        (
            [FromServices] IFoodsService foodsService,
            [FromBody] AddOrUpdateVotableAttributeRequest request
        ) 
        {
            return await foodsService.AddOrUpdateAttribute(request, IdentityHelper.GetIdFromUser(User), VotableAttributeType.Flavor);
        } 

        [HttpPost("{id}/textures")]
        public async Task<ActionResult<VotableAttribute>> AddOrUpdateFoodTexture
        (
            [FromServices] IFoodsService foodsService,
            [FromBody] AddOrUpdateVotableAttributeRequest request
        )
        {
            return await foodsService.AddOrUpdateAttribute(request, IdentityHelper.GetIdFromUser(User), VotableAttributeType.Texture);
        }

        [HttpPost("{id}/miscellaneous")]
        public async Task<ActionResult<VotableAttribute>> AddOrUpdateFoodMiscellaneousAttribute
        (
            [FromServices] IFoodsService foodsService,
            [FromBody] AddOrUpdateVotableAttributeRequest request
        )
        {
            return await foodsService.AddOrUpdateAttribute(request, IdentityHelper.GetIdFromUser(User), VotableAttributeType.Miscellaneous);
        }
    }
}
