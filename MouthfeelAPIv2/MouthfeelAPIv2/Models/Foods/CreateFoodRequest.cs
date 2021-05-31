using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models.Foods
{
    // TODO: Brainstorm what the UI should look like to add ingredients. Then, add Ingredients back to this model
    public class CreateFoodRequest
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }

        public int[] Flavors { get; set; }

        public int[] Textures { get; set; }

        public int[] Miscellaneous { get; set; }
    }
}
