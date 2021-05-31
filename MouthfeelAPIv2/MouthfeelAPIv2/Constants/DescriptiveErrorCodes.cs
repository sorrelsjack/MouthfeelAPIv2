using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Constants
{
    public static class DescriptiveErrorCodes
    {
        public static string CommentMissingBody = "COMMENT_MISSING_BODY";
        public static string CommentDoesNotExist = "COMMENT_DOES_NOT_EXIST";

        public static string FlavorDoesNotExist = "FLAVOR_DOES_NOT_EXIST";
        public static string MiscellaneousDoesNotExist = "MISCELLANEOUS_DOES_NOT_EXIST";
        public static string TextureDoesNotExist = "TEXTURE_DOES_NOT_EXIST";

        public static string FoodNotFound = "FOOD_NOT_FOUND";
        public static string FoodAlreadyExists = "FOOD_ALREADY_EXISTS";
        public static string FoodMissingName = "FOOD_NAME_MISSING";

        public static string UsernameTaken = "USERNAME_TAKEN";
        public static string EmailAlreadyRegistered = "EMAIL_ALREADY_REGISTERED";
        public static string UsernameTooLong = "USERNAME_TOO_LONG";
        public static string EmailInvalidStructure = "EMAIL_INVALID_STRUCTURE";
        public static string MissingLoginDetails = "MISSING_LOGIN_DETAILS";
        public static string UserNotFound = "USER_NOT_FOUND";
        public static string IncorrectCredentials = "INCORRECT_CREDENTIALS";
    }
}
