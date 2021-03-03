using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models.Foods
{
    public class CreateFoodRequest
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }

        public int[] Flavors { get; set; }

        public int[] Textures { get; set; }

        public int[] Miscellaneous { get; set; }
    }
}
