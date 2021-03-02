using MouthfeelAPIv2.DbModels;
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
        public IEnumerable<VotableAttribute> Flavors { get; set;}
        public IEnumerable<VotableAttribute> Textures { get; set; }
        public IEnumerable<VotableAttribute> Miscellaneous { get; set; }

        public FoodResponse(Food food, IEnumerable<VotableAttribute> flavors, IEnumerable<VotableAttribute> textures, IEnumerable<VotableAttribute> misc)
        {
            Id = food.Id;
            Name = food.Name;
            ImageUrl = food.ImageUrl;
            Textures = textures;
            Flavors = flavors;
            Miscellaneous = misc;
        }
    }
}
