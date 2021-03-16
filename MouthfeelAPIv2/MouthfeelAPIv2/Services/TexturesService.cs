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
    public interface ITexturesService
    {
        Task<IEnumerable<VotableAttribute>> GetTextureVotes(int? foodId);
        Task ManageTextureVote(int textureId, int userId, int foodId);
        Task<IEnumerable<Texture>> SearchTextures(string query);
    }

    public class TexturesService : ITexturesService
    {
        private readonly MouthfeelContext _mouthfeel;

        public TexturesService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task<IEnumerable<VotableAttribute>> GetTextureVotes(int? foodId)
        {
            var textureVotes = (await _mouthfeel.TextureVotes.ToListAsync()).Where(t => t.FoodId == foodId);
            var textures = await _mouthfeel.Textures.ToListAsync();

            return textures.Join(textureVotes, texture => texture.Id, vote => vote.TextureId, (texture, vote) =>
                new VotableAttribute
                {
                    Id = texture.Id,
                    Name = texture.Name,
                    Description = texture.Description,
                    Votes = textureVotes.Where(v => v.TextureId == texture.Id).Aggregate(0, (total, next) => total + next.Vote)
                }).DistinctBy(t => t.Id);
        }

        public async Task ManageTextureVote(int textureId, int userId, int foodId)
        {
            var textureVotes = await _mouthfeel.TextureVotes.ToListAsync();
            var textures = await _mouthfeel.Textures.ToListAsync();

            // TODO: Verify the food exists

            if (!textures.Any(f => f.Id == textureId))
                throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.TextureDoesNotExist, DescriptiveErrorCodes.TextureDoesNotExist);

            var existingVoteByUser = textureVotes.FirstOrDefault(v => v.TextureId == textureId && v.FoodId == foodId && v.UserId == userId);

            // Delete it, otherwise we'll have a bunch of 0 records clogging up the table
            if (existingVoteByUser != null)
                _mouthfeel.TextureVotes.Remove(existingVoteByUser);

            else
            {
                _mouthfeel.TextureVotes.Add(new TextureVote
                {
                    TextureId = textureId,
                    UserId = userId,
                    FoodId = foodId,
                    Vote = 1
                });
            }

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<IEnumerable<Texture>> SearchTextures(string query) => (await _mouthfeel.Textures.ToListAsync()).Where(t => t.Name == query);
    }
}
