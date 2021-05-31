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

        public DbSet<Attribute> Attributes { get; set; }
        public DbSet<AttributeType> AttributeTypes { get; set; }
        public DbSet<AttributeVote> AttributeVotes { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentVote> CommentVotes { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<FoodToTry> FoodsToTry { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<FoodComposition> FoodCompositions { get; set; }
        public DbSet<FoodSentiment> FoodSentiments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<FoodImage> FoodImages { get; set; }
    }
}
