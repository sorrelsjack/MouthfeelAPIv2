using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.DbModels
{
    [Table("misc_votes")]
    public class MiscellaneousVote
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("misc_id")]
        public int MiscId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("food_id")]
        public int FoodId { get; set; }
        [Column("vote")]
        public int Vote { get; set; }
    }
}
