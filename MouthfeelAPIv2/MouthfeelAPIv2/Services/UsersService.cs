using MouthfeelAPIv2.Constants;
using MouthfeelAPIv2.DbModels;
using MouthfeelAPIv2.Extensions;
using MouthfeelAPIv2.Helpers;
using MouthfeelAPIv2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Services
{
    public interface IUsersService
    {
        Task RegisterUser(CreateUserRequest request);

        Task<AuthenticateUserResponse> AuthenticateUser(AuthenticateUserRequest request);
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

            if (request.Username.Length > 50) throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.UsernameTooLong);

            if (!request.Email.IsValidEmail()) throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.EmailIsNotAValidStructure);

            _mouthfeel.Users.Add(new User 
            { 
                Username = request.Username,
                Password = request.Password.MakePasswordHash(),
                Email = request.Email,
                DateTimeJoined = DateTime.UtcNow
            });

            await _mouthfeel.SaveChangesAsync();
        }

        public async Task<AuthenticateUserResponse> AuthenticateUser(AuthenticateUserRequest request)
        {
            var user = _mouthfeel.Users.FirstOrDefault(u => u.Username == request.Username);

            if (request == null || request.Username.IsNullOrWhitespace() || request.Password.IsNullOrWhitespace()) throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.MissingLoginDetails);
            if (user == null) throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.UserNotFound);
            if (user.Password != request.Password.MakePasswordHash()) throw new ErrorResponse(HttpStatusCode.BadRequest, ErrorMessages.IncorrectPassword);

            var token = TokenHelper.GenerateToken(user);

            return new AuthenticateUserResponse(user, token);
        }
    }
}
