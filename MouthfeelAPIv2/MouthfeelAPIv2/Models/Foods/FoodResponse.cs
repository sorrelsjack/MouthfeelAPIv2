using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Models.Foods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class FoodResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        // TODO: Incorporate this in the constructor, but make it optional
        public IEnumerable<FoodIngredient> Ingredients { get; set; }
        public IEnumerable<VotableAttribute> Flavors { get; set;}
        public IEnumerable<VotableAttribute> Textures { get; set; }
        public IEnumerable<VotableAttribute> Miscellaneous { get; set; }

        public FoodResponse(
            Food food, 
            IEnumerable<FoodIngredient> ingredients, 
            IEnumerable<VotableAttribute> flavors, 
            IEnumerable<VotableAttribute> textures, 
            IEnumerable<VotableAttribute> misc
        )
        {
            Id = food.Id;
            Name = food.Name;
            ImageUrl = food.ImageUrl;
            Ingredients = ingredients;
            Textures = textures;
            Flavors = flavors;
            Miscellaneous = misc;
        }
    }
}
