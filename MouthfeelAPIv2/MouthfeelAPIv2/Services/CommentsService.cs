using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.Constants;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Enums;
using MouthfeelAPIv2.Extensions;
using MouthfeelAPIv2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Services
{
    public interface ICommentsService
    {
        Task<CommentResponse> CreateComment(CreateCommentRequest request);
        Task DeleteComment();
        Task<CommentVote> ManageCommentVote(int commentId, int userId, int foodId, VoteState voteState);
        Task<IEnumerable<CommentResponse>> GetCommentsByFood(int foodId, int userId);
        Task<IEnumerable<CommentResponse>> GetCommentsByUser(int userId);
    }

    public class CommentsService : ICommentsService
    {
        private readonly MouthfeelContext _mouthfeel;
        private readonly IUsersService _users;

        public CommentsService(MouthfeelContext mouthfeel, IUsersService users)
        {
            _mouthfeel = mouthfeel;
            _users = users;
        }

        public async Task<CommentResponse> CreateComment(CreateCommentRequest request)
        {
            if (request.Body.IsNullOrWhitespace()) throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.CommentMustHaveBody, DescriptiveErrorCodes.CommentMissingBody);

            // TODO: Need validation here... Does user exist? Does food exist?

            var comment = new Comment
            {
                UserId = request.UserId,
                FoodId = request.FoodId,
                Body = request.Body,
                DateTime = DateTime.UtcNow
            };

            await _mouthfeel.Comments.AddAsync(comment);

            await _mouthfeel.SaveChangesAsync();

            await ManageCommentVote(comment.Id, request.UserId, request.FoodId, VoteState.Up);
            var userDetails = await _users.GetUserDetails(request.UserId);

            return new CommentResponse(comment, userDetails, 1, (int)VoteState.Up);
        }

        // TODO: Delete comment
        public async Task DeleteComment()
        {
           
        }


        public async Task<CommentVote> ManageCommentVote(int commentId, int userId, int foodId, VoteState voteState)
        {
            var commentVotes = await _mouthfeel.CommentVotes.ToListAsync();
            var comments = await GetCommentsByFood(foodId, userId);

            // TODO: Verify the food exists
            // TODO: Verify the user exists

            if (!comments.Any(c => c.Id == commentId))
                throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.CommentDoesNotExist, DescriptiveErrorCodes.CommentDoesNotExist);

            var existingVoteByUser = commentVotes.FirstOrDefault(v => v.CommentId == commentId && v.UserId == userId);
            var newVote = new CommentVote { CommentId = commentId, UserId = userId, Vote = (int)voteState };

            if (existingVoteByUser != null)
            {
                if (voteState == VoteState.Neutral)
                    _mouthfeel.CommentVotes.Remove(existingVoteByUser);
                else
                    existingVoteByUser.Vote = (int)voteState;
            }

            else
                _mouthfeel.CommentVotes.Add(newVote);

            await _mouthfeel.SaveChangesAsync();

            return newVote;
        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsByFood(int foodId, int userId)
        {
            var comments = (await _mouthfeel.Comments.ToListAsync()).Where(c => c.FoodId == foodId);
            var tallyTasks = comments.Select(c => TallyCommentVotes(c, userId));

            var tallied = Enumerable.Empty<CommentResponse>();

            foreach (var tally in tallyTasks)
            {
                var res = await tally;
                tallied = tallied.Append(res);
            }

            return tallied;
        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsByUser(int userId)
        {
            var comments = (await _mouthfeel.Comments.ToListAsync()).Where(c => c.UserId == userId);
            var tallyTasks = comments.Select(c => TallyCommentVotes(c, userId));

            var tallied = Enumerable.Empty<CommentResponse>();

            foreach (var tally in tallyTasks)
            {
                var res = await tally;
                tallied = tallied.Append(res);
            }

            return tallied;
        }

        private async Task<CommentResponse> TallyCommentVotes(Comment comment, int userId)
        {
            var commentVotes = (await _mouthfeel.CommentVotes.ToListAsync()).Where(c => c.CommentId == comment.Id);
            var userVote = commentVotes.FirstOrDefault(v => v.UserId == userId)?.Vote ?? 0;
            var userDetails = await _users.GetUserDetails(userId);

            return new CommentResponse(comment, userDetails, commentVotes.Sum(c => c.Vote), userVote);
        }
    }
}
