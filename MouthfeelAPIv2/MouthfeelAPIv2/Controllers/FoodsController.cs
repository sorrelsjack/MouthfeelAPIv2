using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.Constants;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Enums;
using MouthfeelAPIv2.Extensions;
using MouthfeelAPIv2.Models;
using MouthfeelAPIv2.Models.Foods;
using MouthfeelAPIv2.Services;

namespace MouthfeelAPIv2.Controllers
{
    [Route("api/foods")]
    [ApiController]
    public class FoodsController : ControllerBase
    {
        private readonly MouthfeelContext _context;

        public FoodsController(MouthfeelContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Food>>> GetFoods()
        {
            // TODO: Maybe convert from Db model to regular model
            // TODO: Like and dislike operations
            // TODO: Possible convert int ids to guids
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

            // Have param called query. Query can take array of strings: ingredients, attributes, name
            return (await foodsService.SearchFoods(query, searchFilter)).ToList();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FoodResponse>> GetFood
        (
            [FromServices] IFoodsService foodsService,
            int id
        ) => await foodsService.GetFoodDetails(id);

        // PUT: api/foods/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFood(int id, Food food)
        {
            if (id != food.Id)
            {
                return BadRequest();
            }

            _context.Entry(food).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // TODO: Return list of foods marked as "Liked"
        [HttpGet("liked")]
        public async Task<ActionResult<IEnumerable<FoodResponse>>> GetLikedFoods
        (
            [FromServices] IFoodsService foodsService
        ) => (await foodsService.GetLikedFoods()).ToList();

        [HttpGet("disliked")]
        public async Task<ActionResult<IEnumerable<FoodResponse>>> GetDislikedFoods
        (
            [FromServices] IFoodsService foodsService
        ) => (await foodsService.GetDislikedFoods()).ToList();

        [HttpPost("liked")]
        public async Task<ActionResult> AddLikedFood
        (
            [FromServices] IFoodsService foodsService,
            [FromBody] ManageFoodSentimentRequest request
        ) 
        {
            await foodsService.ManageFoodSentiment(request.FoodId, 1, Sentiment.Liked);
            return NoContent();
        }

        [HttpPost("disliked")]
        public async Task<ActionResult> AddDislikedFood
        (
            [FromServices] IFoodsService foodsService,
            [FromBody] ManageFoodSentimentRequest request
        )
        {
            await foodsService.ManageFoodSentiment(request.FoodId, 1, Sentiment.Disliked);
            return NoContent();
        }

        // TODO: Should get a food id, then single flavor object should be received and concatenated onto extant flavor list
        [HttpPost("{id}/flavors")]
        public async Task<ActionResult<Food>> AddFoodFlavor(int id, Food food)
        {
            if (id != food.Id)
            {
                return BadRequest();
            }

            return NoContent();
        }

        // POST: api/foods
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Food>> PostFood(
            [FromServices] IFoodsService foodsService,
            [FromBody] CreateFoodRequest food
        )
        {
            if (food.Name.IsNullOrWhitespace()) return BadRequest("A name must be entered.");
            if (food.ImageUrl.IsNullOrWhitespace()) return BadRequest("An image URL must be associated with a food.");

            await foodsService.AddFood(food);
            return NoContent();
        }

        // DELETE: api/foods/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Food>> DeleteFood(int id)
        {
            var food = await _context.Foods.FindAsync(id);
            if (food == null)
            {
                return NotFound();
            }

            _context.Foods.Remove(food);
            await _context.SaveChangesAsync();

            return food;
        }

        private bool FoodExists(int id)
        {
            return _context.Foods.Any(e => e.Id == id);
        }
    }
}
