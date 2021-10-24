using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Helpers;
using MouthfeelAPIv2.Models;
using MouthfeelAPIv2.Services;

namespace MouthfeelAPIv2.Controllers
{
    [Route("api/comments")]
    [ApiController]
    public class CommentsController : ControllerBase
    {
        private readonly MouthfeelContext _context;

        public CommentsController(MouthfeelContext context)
        {
            _context = context;
        }

        // TODO: Do we want one for "by user"? Could be displayed on Settings screen

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<CommentResponse>>> GetCommentsByFood
        (
            [FromServices] ICommentsService commentsService,
            int id
        ) => (await commentsService.GetCommentsByFood(id, IdentityHelper.GetIdFromUser(User))).ToList();

        [HttpPost]
        public async Task<ActionResult<CommentResponse>> CreateComment 
        (
            [FromServices] ICommentsService commentsService,
            [FromBody] CreateCommentRequest request
        )
        {
            request.UserId = IdentityHelper.GetIdFromUser(User);
            return await commentsService.CreateComment(request);
        }

        [HttpPut]
        public async Task<ActionResult<CommentResponse>> ManageCommentVote
        (
            [FromServices] ICommentsService commentsService,
            [FromBody] ManageCommentVoteRequest request
        )
        {
            request.UserId = IdentityHelper.GetIdFromUser(User);
            return await commentsService.ManageCommentVote(request.CommentId, request.UserId, request.FoodId, request.Vote);
        }

        // TODO: Delete comment
    }
}
