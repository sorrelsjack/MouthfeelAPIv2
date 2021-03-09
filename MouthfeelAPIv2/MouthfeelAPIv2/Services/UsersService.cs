using MouthfeelAPIv2.Constants;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Services
{
    public interface IUsersService
    {
        Task RegisterUser(CreateUserRequest request);

        Task Authenticate();
    }

    public class UsersService : IUsersService
    {
        private readonly MouthfeelContext _mouthfeel;

        public UsersService(MouthfeelContext mouthfeel)
        {
            _mouthfeel = mouthfeel;
        }

        public async Task RegisterUser(CreateUserRequest request)
        {
            var usernameTaken = _mouthfeel.Users.Any(u => u.Username == request.Username);
            if (usernameTaken) throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.UsernameTaken);

            var emailIsRegistered = _mouthfeel.Users.Any(u => u.Email == request.Email);
            if (emailIsRegistered) throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.EmailIsRegistered);

            // TODO: Change this to incorporate hash; find out how to store PWs in a DB
            _mouthfeel.Users.Add(new User 
            { 
                Username = request.Username,
                Password = request.Password,
                Email = request.Email,
                DateTimeJoined = DateTime.UtcNow
            });

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task Authenticate()
        {

        }
    }
}
