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

        public string Flavors { get; set; }

        public string Textures { get; set; }

        public string Miscellaneous { get; set; }
    }
}
