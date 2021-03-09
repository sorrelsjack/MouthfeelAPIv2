using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.DbModels
{
    [Table("comments")]
    public class Comment
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("food_id")]
        public int FoodId { get; set; }
        [Column("comment")]
        public string Body { get; set; }
        [Column("date_time")]
        public DateTime DateTime { get; set; }
    }
}
