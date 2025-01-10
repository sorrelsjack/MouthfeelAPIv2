using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Constants
{
    public static class ErrorMessages
    {
        public static string UsernameTaken = "That username is taken.";

        public static string EmailIsRegistered = "That email is already associated with an account.";

        public static string UsernameTooLong = "The specified username is too long.";

        public static string UserNotFound = "No such user found.";

        public static string CommentMustHaveBody = "Comments must have a body.";

        public static string CommentDoesNotExist = "Comment does not exist under the provided id.";

        public static string MissingLoginDetails = "Some login details are missing.";

        public static string IncorrectPassword = "The given password is incorrect.";

        public static string EmailIsNotAValidStructure = "The specified email is not valid.";

        public static string FoodNotFound = "Food does not exist under the provided id.";

        public static string AttributeDoesNotExist = VotableAttributeDoesNotExist("attributes");

        public static string FlavorDoesNotExist = VotableAttributeDoesNotExist("flavors");

        public static string TextureDoesNotExist = VotableAttributeDoesNotExist("textures");

        public static string MiscellaneousDoesNotExist = VotableAttributeDoesNotExist("miscellaneous attributes");
        private static string VotableAttributeDoesNotExist(string type) => $"One or more {type} does not exist under the provided id.";
    }
}
