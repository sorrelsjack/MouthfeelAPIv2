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
using Attribute = MouthfeelAPIv2.DbModels.Attribute;

namespace MouthfeelAPIv2.Services
{
    public interface IMiscellaneousService
    {
        Task<IEnumerable<VotableAttribute>> GetMiscellaneousVotes(int? foodId, int userId);
        Task ManageMiscellaneousVote(int miscId, int userId, int foodId);

        Task<IEnumerable<Attribute>> SearchMiscellaneous(string query);
    }

    public class MiscellaneousService : IMiscellaneousService
    {
        private readonly MouthfeelContext _mouthfeel;

        public MiscellaneousService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        private async Task<IEnumerable<Attribute>> GetMiscellaneousAttributes()
        {
            return await _mouthfeel.Attributes.Where(a => a.TypeId == 2).ToListAsync();
        }

        public async Task<IEnumerable<VotableAttribute>> GetMiscellaneousVotes(int? foodId, int userId)
        {
            var miscVotes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == foodId);
            var userVote = miscVotes.FirstOrDefault(v => v.UserId == userId)?.Vote ?? 0;
            var misc = await GetMiscellaneousAttributes();

            return misc.Join(miscVotes, misc => misc.Id, vote => vote.AttributeId, (misc, vote) =>
                new VotableAttribute
                {
                    Id = misc.Id,
                    Name = misc.Name,
                    Description = misc.Description,
                    Votes = miscVotes.Where(v => v.AttributeId == misc.Id).Aggregate(0, (total, next) => total + next.Vote),
                    Sentiment = userVote
                }).DistinctBy(m => m.Id);
        }

        public async Task ManageMiscellaneousVote(int miscVote, int userId, int foodId)
        {
            var miscVotes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == foodId);
            var misc = await GetMiscellaneousAttributes();

            // TODO: Verify the food exists

            if (!misc.Any(f => f.Id == miscVote))
                throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.MiscellaneousDoesNotExist, DescriptiveErrorCodes.MiscellaneousDoesNotExist);

            var existingVoteByUser = miscVotes.FirstOrDefault(v => v.AttributeId == miscVote && v.FoodId == foodId && v.UserId == userId);

            // Delete it, otherwise we'll have a bunch of 0 records clogging up the table
            if (existingVoteByUser != null)
                _mouthfeel.AttributeVotes.Remove(existingVoteByUser);

            else
            {
                _mouthfeel.AttributeVotes.Add(new AttributeVote
                {
                    AttributeId = miscVote,
                    UserId = userId,
                    FoodId = foodId,
                    Vote = 1
                });
            }

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<IEnumerable<Attribute>> SearchMiscellaneous(string query) => (await GetMiscellaneousAttributes()).Where(m => m.Name == query);
    }
}
