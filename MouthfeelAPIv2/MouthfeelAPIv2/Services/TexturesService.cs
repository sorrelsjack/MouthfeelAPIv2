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
    public interface ITexturesService
    {
        Task<IEnumerable<VotableAttribute>> GetTextureVotes(int? foodId, int userId);
        Task ManageTextureVote(int textureId, int userId, int foodId);
        Task<IEnumerable<Attribute>> SearchTextures(string query);
    }

    public class TexturesService : ITexturesService
    {
        private readonly MouthfeelContext _mouthfeel;

        public TexturesService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        private async Task<IEnumerable<Attribute>> GetTextureAttributes()
        {
            return await _mouthfeel.Attributes.Where(a => a.TypeId == 3).ToListAsync();
        }

        public async Task<IEnumerable<VotableAttribute>> GetTextureVotes(int? foodId, int userId)
        {
            var textureVotes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == foodId);
            var userVote = textureVotes.FirstOrDefault(v => v.UserId == userId)?.Vote ?? 0;
            var textures = await GetTextureAttributes();

            return textures.Join(textureVotes, texture => texture.Id, vote => vote.AttributeId, (texture, vote) =>
                new VotableAttribute
                {
                    Id = texture.Id,
                    Name = texture.Name,
                    Description = texture.Description,
                    Votes = textureVotes.Where(v => v.AttributeId == texture.Id).Aggregate(0, (total, next) => total + next.Vote)
                }).DistinctBy(t => t.Id);
        }

        public async Task ManageTextureVote(int textureId, int userId, int foodId)
        {
            var textureVotes = (await _mouthfeel.AttributeVotes.ToListAsync()).Where(m => m.FoodId == foodId);
            var textures = await GetTextureAttributes();

            // TODO: Verify the food exists

            if (!textures.Any(f => f.Id == textureId))
                throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.TextureDoesNotExist, DescriptiveErrorCodes.TextureDoesNotExist);

            var existingVoteByUser = textureVotes.FirstOrDefault(v => v.AttributeId == textureId && v.FoodId == foodId && v.UserId == userId);

            // Delete it, otherwise we'll have a bunch of 0 records clogging up the table
            if (existingVoteByUser != null)
                _mouthfeel.AttributeVotes.Remove(existingVoteByUser);

            else
            {
                _mouthfeel.AttributeVotes.Add(new AttributeVote
                {
                    AttributeId = textureId,
                    UserId = userId,
                    FoodId = foodId,
                    Vote = 1
                });
            }

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<IEnumerable<Attribute>> SearchTextures(string query) => (await GetTextureAttributes()).Where(t => t.Name == query);
    }
}
