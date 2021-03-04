using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Constants
{
    public static class FoodSearchType
    {
        public static string Name = "name";
        public static string Ingredients = "ingredients";
        public static string Attributes = "attributes";

        public static string[] GetAllTypes() => new[] { Name, Ingredients, Attributes };
    }
}
