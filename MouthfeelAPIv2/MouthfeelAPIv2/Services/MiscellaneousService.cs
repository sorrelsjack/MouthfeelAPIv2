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
using System.Web.Http;

namespace MouthfeelAPIv2.Services
{
    public interface IMiscellaneousService
    {
        Task<IEnumerable<VotableAttribute>> GetMiscellaneousVotes(int? foodId);
        Task ManageMiscellaneousVote(int miscId, int userId, int foodId);

        Task<IEnumerable<Miscellaneous>> SearchMiscellaneous(string query);
    }

    public class MiscellaneousService : IMiscellaneousService
    {
        private readonly MouthfeelContext _mouthfeel;

        public MiscellaneousService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task<IEnumerable<VotableAttribute>> GetMiscellaneousVotes(int? foodId)
        {
            var miscVotes = (await _mouthfeel.MiscellaneousVotes.ToListAsync()).Where(m => m.FoodId == foodId);
            var misc = await _mouthfeel.Miscellaneous.ToListAsync();

            return misc.Join(miscVotes, misc => misc.Id, vote => vote.MiscId, (misc, vote) =>
                new VotableAttribute
                {
                    Id = misc.Id,
                    Name = misc.Name,
                    Description = misc.Description,
                    Votes = miscVotes.Where(v => v.MiscId == misc.Id).Aggregate(0, (total, next) => total + next.Vote)
                }).DistinctBy(m => m.Id);
        }

        public async Task ManageMiscellaneousVote(int miscVote, int userId, int foodId)
        {
            var miscVotes = await _mouthfeel.MiscellaneousVotes.ToListAsync();
            var misc = await _mouthfeel.Miscellaneous.ToListAsync();

            // TODO: Verify the food exists

            if (!misc.Any(f => f.Id == miscVote))
                throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.MiscellaneousDoesNotExist);

            var existingVoteByUser = miscVotes.FirstOrDefault(v => v.MiscId == miscVote && v.FoodId == foodId && v.UserId == userId);

            // Delete it, otherwise we'll have a bunch of 0 records clogging up the table
            if (existingVoteByUser != null)
                _mouthfeel.MiscellaneousVotes.Remove(existingVoteByUser);

            else
            {
                _mouthfeel.MiscellaneousVotes.Add(new MiscellaneousVote
                {
                    MiscId = miscVote,
                    UserId = userId,
                    FoodId = foodId,
                    Vote = 1
                });
            }

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<IEnumerable<Miscellaneous>> SearchMiscellaneous(string query) => (await _mouthfeel.Miscellaneous.ToListAsync()).Where(m => m.Name == query);
    }
}
