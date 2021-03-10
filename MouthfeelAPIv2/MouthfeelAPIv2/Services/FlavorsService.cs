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
    public interface IFlavorsService
    {
        Task<IEnumerable<VotableAttribute>> GetFlavorVotes(int? foodId);

        Task ManageFlavorVote(int flavorId, int userId, int foodId);

        Task<IEnumerable<Flavor>> SearchFlavors(string query);
    }

    public class FlavorsService : IFlavorsService
    {
        private readonly MouthfeelContext _mouthfeel;

        public FlavorsService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task<IEnumerable<VotableAttribute>> GetFlavorVotes(int? foodId)
        {
            var flavorVotes = (await _mouthfeel.FlavorVotes.ToListAsync()).Where(f => f.FoodId == foodId);
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

        public async Task ManageFlavorVote(int flavorId, int userId, int foodId)
        {
            var flavorVotes = await _mouthfeel.FlavorVotes.ToListAsync();
            var flavors = await _mouthfeel.Flavors.ToListAsync();

            // TODO: Verify the food exists

            if (!flavors.Any(f => f.Id == flavorId))
                throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.FlavorDoesNotExist);

            var existingVoteByUser = flavorVotes.FirstOrDefault(v => v.FlavorId == flavorId && v.FoodId == foodId && v.UserId == userId);

            // Delete it, otherwise we'll have a bunch of 0 records clogging up the table
            if (existingVoteByUser != null)
                _mouthfeel.FlavorVotes.Remove(existingVoteByUser);

            else
            {
                _mouthfeel.FlavorVotes.Add(new FlavorVote 
                { 
                    FlavorId = flavorId, 
                    UserId = userId, 
                    FoodId = foodId, 
                    Vote = 1 
                });
            }

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<IEnumerable<Flavor>> SearchFlavors(string query) => (await _mouthfeel.Flavors.ToListAsync()).Where(f => f.Name == query);

    }
}
