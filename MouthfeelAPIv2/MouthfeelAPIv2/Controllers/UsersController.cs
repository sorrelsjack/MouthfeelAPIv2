using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MouthfeelAPIv2.Extensions;
using MouthfeelAPIv2.Models;
using MouthfeelAPIv2.Services;

namespace MouthfeelAPIv2.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser
        (
            [FromServices] IUsersService usersService,
            [FromBody] CreateUserRequest request
        )
        {
            await usersService.RegisterUser(request);
            return NoContent();
        }

        [HttpPost("authenticate")]
        public async Task<ActionResult<AuthenticateUserResponse>> Authenticate
        (
            [FromServices] IUsersService usersService,
            [FromBody] AuthenticateUserRequest request
        )
        {
            return await usersService.AuthenticateUser(request);
        }
    }
}
