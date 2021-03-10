using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.DbModels
{
    public class MouthfeelContext : DbContext
    {
        public MouthfeelContext(DbContextOptions<MouthfeelContext> options) : base(options)
        {

        }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentVote> CommentVotes { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<FoodToTry> FoodsToTry { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<FoodComposition> FoodCompositions { get; set; }
        public DbSet<FoodSentiment> FoodSentiments { get; set; }
        public DbSet<Flavor> Flavors { get; set; }
        public DbSet<FlavorVote> FlavorVotes { get; set; }
        public DbSet<Miscellaneous> Miscellaneous { get; set; }
        public DbSet<MiscellaneousVote> MiscellaneousVotes { get; set; }
        public DbSet<Texture> Textures { get; set; }
        public DbSet<TextureVote> TextureVotes { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
