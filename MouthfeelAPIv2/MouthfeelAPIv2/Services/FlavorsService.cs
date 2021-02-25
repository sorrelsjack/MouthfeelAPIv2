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
    public interface IFlavorsService
    {
        Task<IEnumerable<VotableAttribute>> GetFlavorVotesByFood(int foodId);
    }

    public class FlavorsService : IFlavorsService
    {
        private readonly MouthfeelContext _mouthfeel;

        public FlavorsService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task<IEnumerable<VotableAttribute>> GetFlavorVotesByFood(int foodId)
        {
            var flavorVotes = await _mouthfeel.FlavorVotes.ToListAsync();
            var flavors = await _mouthfeel.Flavors.ToListAsync();

            return flavors.Join(flavorVotes, flavor => flavor.Id, vote => vote.FlavorId, (flavor, vote) =>
                new VotableAttribute
                {
                    Id = flavor.Id,
                    Name = flavor.Name,
                    Description = flavor.Description,
                    Votes = flavorVotes.Where(v => v.FlavorId == flavor.Id).Aggregate(0, (total, next) => total + next.Vote)
                }).DistinctBy(f => f.Id);
        }
    }
}
