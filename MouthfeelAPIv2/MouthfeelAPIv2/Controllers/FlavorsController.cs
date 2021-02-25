using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;

namespace MouthfeelAPIv2.Controllers
{
    [Route("api/flavors")]
    [ApiController]
    public class FlavorsController : ControllerBase
    {
        private readonly MouthfeelContext _context;

        public FlavorsController(MouthfeelContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Flavor>>> GetTextures()
        {
            // TODO: Maybe convert from Db model to regular model
            return await _context.Flavors.ToListAsync();
        }
    }
}
