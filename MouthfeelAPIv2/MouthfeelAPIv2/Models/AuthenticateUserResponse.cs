using MouthfeelAPIv2.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class AuthenticateUserResponse
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }

        public AuthenticateUserResponse(User user, string token)
        {
            Id = user.Id;
            Username = user.Username;
            Token = token;
        }
    }
}
