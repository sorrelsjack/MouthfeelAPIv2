using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Services
{
    public interface ICommentsService
    {
        Task CreateComment(CreateCommentRequest request);
        Task<IEnumerable<Comment>> GetCommentsByFood(int foodId);
        Task<IEnumerable<Comment>> GetCommentsByUser(int userId);
    }

    // TODO: Need table for comment votes!
    public class CommentsService : ICommentsService
    {
        private readonly MouthfeelContext _mouthfeel;

        public CommentsService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task CreateComment(CreateCommentRequest request)
        {
            // TODO: Need validation here
            // TODO: Do we want to be able to edit comments?
            _mouthfeel.Comments.Add(new Comment
            {
                UserId = request.UserId,
                FoodId = request.FoodId,
                Body = request.Body,
                DateTime = DateTime.UtcNow
            });

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsByFood(int foodId)
            => (await _mouthfeel.Comments.ToListAsync()).Where(c => c.FoodId == foodId);

        public async Task<IEnumerable<Comment>> GetCommentsByUser(int userId)
            => (await _mouthfeel.Comments.ToListAsync()).Where(c => c.UserId == userId);
    }
}
