using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.DbModels
{
    [Table("foods_to_try")]
    public class FoodToTry
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("food_id")]
        public int FoodId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
    }
}
