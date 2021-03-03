using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<FoodResponse>> GetFood
        (
            [FromServices] ITexturesService texturesService, 
            [FromServices] IFlavorsService flavorsService, 
            [FromServices] IMiscellaneousService miscService,
            int id
        )
        {
            // TODO: Make some changes so that flavor and misc are like textures
            var food = await _context.Foods.FindAsync(id);
            var textures = await texturesService.GetTextureVotes(id);
            var flavors = await flavorsService.GetFlavorVotes(id);
            var misc = await miscService.GetMiscellaneousVotes(id);

            if (food == null)
                return NotFound();

            return new FoodResponse(food, flavors, textures, misc);
        }

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
        public async Task<ActionResult<IEnumerable<Food>>> GetLikedFoods() 
        {
            return null;
        }

        [HttpGet("disliked")]
        public async Task<ActionResult<IEnumerable<Food>>> GetDislikedFoods()
        {
            return null;
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
            [FromServices] IFlavorsService flavorsService,
            [FromServices] IMiscellaneousService miscService,
            [FromServices] ITexturesService texturesService,
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
