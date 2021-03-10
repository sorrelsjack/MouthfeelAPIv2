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
        Task CreateComment(CreateCommentRequest request);
        Task DeleteComment();
        Task ManageCommentVote(int commentId, int userId, int foodId, VoteState voteState);
        Task<IEnumerable<CommentResponse>> GetCommentsByFood(int foodId, int userId);
        Task<IEnumerable<CommentResponse>> GetCommentsByUser(int userId);
    }

    public class CommentsService : ICommentsService
    {
        private readonly MouthfeelContext _mouthfeel;

        public CommentsService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task CreateComment(CreateCommentRequest request)
        {
            if (request.Body.IsNullOrWhitespace()) throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.CommentMustHaveBody);

            // TODO: Need validation here... Does user exist? Does food exist?

            var comment = new Comment
            {
                UserId = request.UserId,
                FoodId = request.FoodId,
                Body = request.Body,
                DateTime = DateTime.UtcNow
            };

            _mouthfeel.Comments.Add(comment);

            await _mouthfeel.SaveChangesAsync();

            await ManageCommentVote(comment.Id, request.UserId, request.FoodId, VoteState.Up);

        }

        // TODO: Delete comment
        public async Task DeleteComment()
        {
           
        }


        public async Task ManageCommentVote(int commentId, int userId, int foodId, VoteState voteState)
        {
            var commentVotes = await _mouthfeel.CommentVotes.ToListAsync();
            var comments = await GetCommentsByFood(foodId, userId);

            // TODO: Verify the food exists
            // TODO: Verify the user exists

            if (!comments.Any(c => c.Id == commentId))
                throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.CommentDoesNotExist);

            var existingVoteByUser = commentVotes.FirstOrDefault(v => v.Id == commentId && v.UserId == userId);

            if (existingVoteByUser != null)
            {
                if (voteState == VoteState.Neutral)
                    _mouthfeel.CommentVotes.Remove(existingVoteByUser);
                else
                    existingVoteByUser.Vote = (int)voteState;
            }

            else
            {
                _mouthfeel.CommentVotes.Add(new CommentVote
                {
                    CommentId = commentId,
                    UserId = userId,
                    Vote = (int)voteState
                });
            }

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsByFood(int foodId, int userId)
        {
            var comments = (await _mouthfeel.Comments.ToListAsync()).Where(c => c.FoodId == foodId);
            var tallyTasks = comments.Select(c => TallyCommentVotes(c, userId));
            var tallied = await Task.WhenAll(tallyTasks);
            return tallied;

        }

        public async Task<IEnumerable<CommentResponse>> GetCommentsByUser(int userId)
        {
            var comments = (await _mouthfeel.Comments.ToListAsync()).Where(c => c.UserId == userId);
            var tallyTasks = comments.Select(c => TallyCommentVotes(c, userId));
            var tallied = await Task.WhenAll(tallyTasks);
            return tallied;
        }

        private async Task<CommentResponse> TallyCommentVotes(Comment comment, int userId)
        {
            var commentVotes = (await _mouthfeel.CommentVotes.ToListAsync()).Where(c => c.Id == comment.Id);
            var userVote = commentVotes.FirstOrDefault(v => v.UserId == userId).Vote;

            return new CommentResponse(comment, commentVotes.Sum(c => c.Vote), userVote);
        }
    }
}
