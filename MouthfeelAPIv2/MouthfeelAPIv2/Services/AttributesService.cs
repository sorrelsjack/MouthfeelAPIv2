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
        Task<IEnumerable<AttributeType>> GetAttributeTypes();
        Task<IEnumerable<Attribute>> GetAttributes(VotableAttributeType type);
        Task<IEnumerable<VotableAttribute>> GetVotes(int? foodId, int userId, VotableAttributeType type);
        Task ManageVote(int attributeId, int userId, int foodId, VotableAttributeType type);
        Task<IEnumerable<Attribute>> SearchAttributes(string query, VotableAttributeType type);
    }

    public class AttributesService : IAttributesService
    {
        private readonly MouthfeelContext _mouthfeel;

        public AttributesService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task<IEnumerable<AttributeType>> GetAttributeTypes()
            => await _mouthfeel.AttributeTypes.ToListAsync();

        public async Task<IEnumerable<Attribute>> GetAttributes(VotableAttributeType type)
            => await _mouthfeel.Attributes.Where(a => a.TypeId == (int)type).ToListAsync();

        public async Task<IEnumerable<VotableAttribute>> GetVotes(int? foodId, int userId, VotableAttributeType type)
        {
            var votes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == foodId);
            var userVote = votes.FirstOrDefault(v => v.UserId == userId)?.Vote ?? 0;
            var attributes = await GetAttributes(type);

            return attributes.Join(votes, attr => attr.Id, vote => vote.AttributeId, (attr, vote) =>
                new VotableAttribute
                {
                    Id = attr.Id,
                    Name = attr.Name,
                    Description = attr.Description,
                    Votes = votes.Where(v => v.AttributeId == attr.Id).Aggregate(0, (total, next) => total + next.Vote),
                    Sentiment = userVote
                }).DistinctBy(a => a.Id);
        }

        public async Task ManageVote(int attributeId, int userId, int foodId, VotableAttributeType type)
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
        }

        public async Task<IEnumerable<Attribute>> SearchAttributes(string query, VotableAttributeType type)
            => (await GetAttributes(type)).Where(a => a.Name == query);
    }
}