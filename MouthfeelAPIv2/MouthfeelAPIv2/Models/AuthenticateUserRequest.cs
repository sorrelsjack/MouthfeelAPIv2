using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class AuthenticateUserRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
