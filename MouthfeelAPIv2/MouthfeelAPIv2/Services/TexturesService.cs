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
    public interface ITexturesService
    {
        Task<IEnumerable<VotableAttribute>> GetTextureVotes(int? foodId);
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
    }
}
