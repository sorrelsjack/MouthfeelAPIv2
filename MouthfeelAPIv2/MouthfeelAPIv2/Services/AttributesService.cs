﻿using Microsoft.EntityFrameworkCore;
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
using Attribute = MouthfeelAPIv2.DbModels.Attribute;

namespace MouthfeelAPIv2.Services
{
    public interface IAttributesService
    {
        Task<IEnumerable<Attribute>> GetAllAttributes();
        Task<IEnumerable<AttributeType>> GetAttributeTypes();
        Task<IEnumerable<Attribute>> GetAttributes(VotableAttributeType type);
        Task<IEnumerable<VotableAttribute>> GetVotes(int? foodId, int userId, VotableAttributeType type);
        Task<Dictionary<int, IEnumerable<VotableAttribute>>> GetManyVotes(IEnumerable<int> foodId, int userId, VotableAttributeType type);
        Task<IEnumerable<Attribute>> GetTopThree(int? foodId);
        Task<Dictionary<int, IEnumerable<Attribute>>> GetManyTopThrees(IEnumerable<int> foodIds);
        Task<VotableAttribute> ManageVote(int attributeId, int userId, int foodId, VotableAttributeType type);
        Task<IEnumerable<Attribute>> SearchAttributes(string query, VotableAttributeType type);
    }

    public class AttributesService : IAttributesService
    {
        private readonly MouthfeelContext _mouthfeel;

        public AttributesService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task<IEnumerable<Attribute>> GetAllAttributes()
            => _mouthfeel.Attributes;

        public async Task<IEnumerable<AttributeType>> GetAttributeTypes()
            => await _mouthfeel.AttributeTypes.ToListAsync();

        public async Task<IEnumerable<Attribute>> GetAttributes(VotableAttributeType type)
            => _mouthfeel.Attributes.Where(a => a.TypeId == (int)type);

        public async Task<IEnumerable<VotableAttribute>> GetVotes(int? foodId, int userId, VotableAttributeType type)
        {
            var votes = _mouthfeel.AttributeVotes.Where(m => m.FoodId == foodId).ToList();
            var userVotes = votes.Where(v => v.UserId == userId).ToList();
            var attributes = await GetAttributes(type);

            return attributes.Join(votes, attr => attr.Id, vote => vote.AttributeId, (attr, vote) =>
                new VotableAttribute
                {
                    Id = attr.Id,
                    Name = attr.Name,
                    Description = attr.Description,
                    Votes = votes.Where(v => v.AttributeId == attr.Id).Aggregate(0, (total, next) => total + next.Vote),
                    Sentiment = userVotes.FirstOrDefault(v => v.AttributeId == attr.Id)?.Vote ?? 0
                }).DistinctBy(a => a.Id);
        }

        public async Task<Dictionary<int, IEnumerable<VotableAttribute>>> GetManyVotes(IEnumerable<int> foodIds, int userId, VotableAttributeType type)
        {
            var records = new Dictionary<int, IEnumerable<VotableAttribute>>();

            foreach (var id in foodIds)
            {
                var votes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == id);
                var userVotes = votes.Where(v => v.UserId == userId);
                var attributes = await GetAttributes(type);

                var joined = attributes.Join(votes, attr => attr.Id, vote => vote.AttributeId, (attr, vote) =>
                                new VotableAttribute
                                {
                                    Id = attr.Id,
                                    Name = attr.Name,
                                    Description = attr.Description,
                                    Votes = votes.Where(v => v.AttributeId == attr.Id).Aggregate(0, (total, next) => total + next.Vote),
                                    Sentiment = userVotes.FirstOrDefault(v => v.AttributeId == attr.Id)?.Vote ?? 0
                                }).DistinctBy(a => a.Id);

                records.Add(id, joined);
            }

            return records;
        }

        public async Task<IEnumerable<Attribute>> GetTopThree(int? foodId)
        {
            Dictionary<int, int> aggregated = new Dictionary<int, int>();

            var votes = _mouthfeel.AttributeVotes.Where(m => m.FoodId == foodId).ToList().GroupBy(v => v.AttributeId);
            var attributes = _mouthfeel.Attributes;

            foreach (var vote in votes)
                aggregated.Add(vote.Key, vote.Aggregate(0, (total, next) => total + next.Vote));

            var topThree = aggregated.OrderByDescending(kvp => kvp.Value).Take(3);
            return attributes.Where(a => topThree.Select(t => t.Key).Contains(a.Id));
        }

        public async Task<Dictionary<int, IEnumerable<Attribute>>> GetManyTopThrees(IEnumerable<int> foodIds)
        {
            Dictionary<int, int> aggregated = new Dictionary<int, int>();
            Dictionary<int, IEnumerable<Attribute>> topThrees = new Dictionary<int, IEnumerable<Attribute>>();

            var attributes = (await _mouthfeel.Attributes.ToListAsync());

            foreach (var id in foodIds)
            {
                var votes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == id).GroupBy(v => v.AttributeId);

                foreach (var vote in votes)
                    aggregated.Add(vote.Key, vote.Aggregate(0, (total, next) => total + next.Vote));

                var topThree = aggregated.OrderByDescending(kvp => kvp.Value).Take(3);

                topThrees.Add(id, attributes.Where(a => topThree.Select(t => t.Key).Contains(a.Id)));
            }

            return topThrees;
        }

        public async Task<VotableAttribute> ManageVote(int attributeId, int userId, int foodId, VotableAttributeType type)
        {
            var votes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == foodId);
            var attributes = await GetAttributes(type);

            var foods = await _mouthfeel.Foods.ToListAsync();

            if (!foods.Any(f => f.Id == foodId))
                throw new ErrorResponse(HttpStatusCode.NotFound, ErrorMessages.FoodNotFound, DescriptiveErrorCodes.FoodNotFound);

            if (!attributes.Any(f => f.Id == attributeId))
                throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.MiscellaneousDoesNotExist, DescriptiveErrorCodes.MiscellaneousDoesNotExist);

            var existingVoteByUser = votes.FirstOrDefault(v => v.AttributeId == attributeId && v.FoodId == foodId && v.UserId == userId);

            // Delete it, otherwise we'll have a bunch of 0 records clogging up the table
            if (existingVoteByUser != null)
                _mouthfeel.AttributeVotes.Remove(existingVoteByUser);

            else
            {
                _mouthfeel.AttributeVotes.Add(new AttributeVote
                {
                    AttributeId = attributeId,
                    UserId = userId,
                    FoodId = foodId,
                    Vote = 1
                });
            }

            await _mouthfeel.SaveChangesAsync();

            var votesForAttribute = (await GetVotes(foodId, userId, type))?.FirstOrDefault(v => v.Id == attributeId)?.Votes ?? 0;

            return new VotableAttribute 
            { 
                Id = attributeId, 
                Name = attributes.FirstOrDefault(a => a.Id == attributeId).Name,
                Description = attributes.FirstOrDefault(a => a.Id == attributeId).Description,
                Sentiment = existingVoteByUser != null ? 0 : 1,
                Votes = votesForAttribute
            };
        }

        public async Task<IEnumerable<Attribute>> SearchAttributes(string query, VotableAttributeType type)
            => (await GetAttributes(type)).Where(a => a.Name == query);
    }
}
