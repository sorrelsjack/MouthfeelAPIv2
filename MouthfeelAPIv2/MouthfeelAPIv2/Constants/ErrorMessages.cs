using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Constants
{
    public static class ErrorMessages
    {
        public static string FoodNotFound = "Food does not exist under the provided id.";
        public static string FlavorDoesNotExist = VotableAttributeDoesNotExist("flavors");

        public static string TextureDoesNotExist = VotableAttributeDoesNotExist("textures");

        public static string MiscellaneousDoesNotExist = VotableAttributeDoesNotExist("miscellaneous attributes");
        private static string VotableAttributeDoesNotExist(string type) => $"One or more {type} does not exist under the provided id.";
    }
}
