using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Enums;
using MouthfeelAPIv2.Services;
using Attribute = MouthfeelAPIv2.DbModels.Attribute;

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
        public async Task<ActionResult<IEnumerable<Attribute>>> GetFlavors
        (
            [FromServices] IAttributesService _attributes
        ) => (await _attributes.GetAttributes(VotableAttributeType.Flavor)).ToList();
    }
}
