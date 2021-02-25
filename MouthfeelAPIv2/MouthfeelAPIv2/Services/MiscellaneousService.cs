using Microsoft.EntityFrameworkCore;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Extensions;
using MouthfeelAPIv2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Services
{
    public interface IMiscellaneousService
    {
        Task<IEnumerable<VotableAttribute>> GetMiscellaneousVotesByFood(int foodId);
    }

    public class MiscellaneousService : IMiscellaneousService
    {
        private readonly MouthfeelContext _mouthfeel;

        public MiscellaneousService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task<IEnumerable<VotableAttribute>> GetMiscellaneousVotesByFood(int foodId)
        {
            var miscVotes = await _mouthfeel.MiscellaneousVotes.ToListAsync();
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
    }
}
