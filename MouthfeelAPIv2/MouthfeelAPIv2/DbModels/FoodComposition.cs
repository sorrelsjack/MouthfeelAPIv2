using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.DbModels
{
    [Table("food_compositions")]
    public class FoodComposition
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("food_id")]
        public int FoodId { get; set; }
        [Column("ingredient_id")]
        public int IngredientId { get; set; }
        [Column("quantity")]
        public string Quantity { get; set; }
    }
}
