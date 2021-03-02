﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;

namespace MouthfeelAPIv2.Controllers
{
    [Route("api/misc")]
    [ApiController]
    public class MiscellaneousController : ControllerBase
    {
        private readonly MouthfeelContext _context;

        public MiscellaneousController(MouthfeelContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Miscellaneous>>> GetTextures()
        {
            // TODO: Maybe convert from Db model to regular model
            return await _context.Miscellaneous.OrderBy(m => m.Name).ToListAsync();
        }
    }
}
