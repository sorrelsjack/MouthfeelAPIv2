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
    public interface IFlavorsService
    {
        Task<IEnumerable<VotableAttribute>> GetFlavorVotes(int? foodId, int userId);

        Task ManageFlavorVote(int flavorId, int userId, int foodId);

        Task<IEnumerable<Attribute>> SearchFlavors(string query);
    }

    public class FlavorsService : IFlavorsService
    {
        private readonly MouthfeelContext _mouthfeel;

        public FlavorsService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        private async Task<IEnumerable<Attribute>> GetFlavorAttributes()
        {
            return await _mouthfeel.Attributes.Where(a => a.TypeId == 1).ToListAsync();
        }

        public async Task<IEnumerable<VotableAttribute>> GetFlavorVotes(int? foodId, int userId)
        {
            var flavorVotes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == foodId);
            var userVote = flavorVotes.FirstOrDefault(v => v.UserId == userId)?.Vote ?? 0;
            var flavors = await GetFlavorAttributes();

            return flavors.Join(flavorVotes, flavor => flavor.Id, vote => vote.AttributeId, (flavor, vote) =>
                new VotableAttribute
                {
                    Id = flavor.Id,
                    Name = flavor.Name,
                    Description = flavor.Description,
                    Votes = flavorVotes.Where(v => v.AttributeId == flavor.Id).Aggregate(0, (total, next) => total + next.Vote),
                    Sentiment = userVote
                }).DistinctBy(f => f.Id);
        }

        public async Task ManageFlavorVote(int flavorId, int userId, int foodId)
        {
            var flavorVotes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == foodId);
            var flavors = await GetFlavorAttributes();

            // TODO: Verify the food exists

            if (!flavors.Any(f => f.Id == flavorId))
                throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.FlavorDoesNotExist, DescriptiveErrorCodes.FlavorDoesNotExist);

            var existingVoteByUser = flavorVotes.FirstOrDefault(v => v.AttributeId == flavorId && v.FoodId == foodId && v.UserId == userId);

            // Delete it, otherwise we'll have a bunch of 0 records clogging up the table
            if (existingVoteByUser != null)
                _mouthfeel.AttributeVotes.Remove(existingVoteByUser);

            else
            {
                _mouthfeel.AttributeVotes.Add(new AttributeVote 
                { 
                    AttributeId = flavorId, 
                    UserId = userId, 
                    FoodId = foodId, 
                    Vote = 1 
                });
            }

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<IEnumerable<Attribute>> SearchFlavors(string query) => (await GetFlavorAttributes()).Where(f => f.Name == query);

    }
}
