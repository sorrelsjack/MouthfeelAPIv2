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
            return await _context.Foods.ToListAsync();
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
            var flavors = await flavorsService.GetFlavorVotesByFood(id);
            var misc = await miscService.GetMiscellaneousVotesByFood(id);

            if (food == null)
            {
                return NotFound();
            }

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
        public async Task<ActionResult<Food>> PostFood([FromServices] IFoodsService foodsService, Food food)
        {
            // I guess I made it so that you have to submit an image URL if you want to make a new food. Maybe change it later
            if (food.Name.IsNullOrWhitespace()) return BadRequest("A name must be entered.");
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
