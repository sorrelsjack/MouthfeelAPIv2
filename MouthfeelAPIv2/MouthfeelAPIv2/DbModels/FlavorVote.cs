using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.DbModels
{
    [Table("flavor_votes")]
    public class FlavorVote
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("flavor_id")]
        public int FlavorId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("food_id")]
        public int FoodId { get; set; }
        [Column("vote")]
        public int Vote { get; set; }
    }
}
