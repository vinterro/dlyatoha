using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using Thread_.NET.BLL.Exceptions;
using Thread_.NET.BLL.Hubs;
using Thread_.NET.BLL.JWT;
using Thread_.NET.BLL.Services.Abstract;
using Thread_.NET.Common.DTO.Auth;
using Thread_.NET.Common.DTO.User;
using Thread_.NET.Common.Security;
using Thread_.NET.DAL.Context;
using Thread_.NET.DAL.Entities;
using Thread_.NET.DAL.Entities.Abstract;
using Microsoft.Extensions.Logging;

namespace Thread_.NET.BLL.Services
{
    public sealed class AuthService : BaseService
    {
        private readonly JwtFactory _jwtFactory;
        private readonly IHubContext<PostHub> _authHub;
        private readonly ILogger<AuthService> _logger;
        public AuthService(ThreadContext context, IMapper mapper, ILogger<AuthService> logger,  JwtFactory jwtFactory, IHubContext<PostHub> authHub) : base(context, mapper)
        {
            _jwtFactory = jwtFactory;
            _authHub = authHub;
            _logger = logger;
        }



        public async Task<bool> SendEmailAsync(string email)
        {
            
            var userEntity = await _context.Users
                .Include(u => u.Avatar)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (userEntity == null)
            {
                throw new NoUserWithEmailException();
            }

            if (userEntity.Email != email)
            {
                throw new EmailTakenException();
            }



            var token = await GenerateAccessToken(userEntity.Id, userEntity.UserName , userEntity.Email);


            MailAddress from = new("vanyep3@gmail.com", "Tom");
            MailAddress to = new(email);
            MailMessage m = new(from, to)
            {
                Subject = "Reset your password",
                Body = $"Click on this link to reset your password: http://localhost:4200/reset-password?token={token.AccessToken.Token}",
                IsBodyHtml = true
            };
            SmtpClient smtp = new("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("vanyep3@gmail.com", "dkfejrfdsexuvwuo"),
                EnableSsl = true
            };
            await smtp.SendMailAsync(m);
            

            return true;
        }

        public async Task<AuthUserDTO> Authorize(UserLoginDTO userDto)
        {
            
            var userEntity = await _context.Users
                .Include(u => u.Avatar)
                .FirstOrDefaultAsync(u => u.Email == userDto.Email);

            _logger.LogInformation($"password: {userEntity.Password}, email: {userEntity.Email}");

            if (userEntity == null)
            {
                throw new NotFoundException(nameof(User));
            }

            if (!SecurityHelper.ValidatePassword(userDto.Password, userEntity.Password, userEntity.Salt))
            {
                throw new InvalidUsernameOrPasswordException();
            }

            var token = await GenerateAccessToken(userEntity.Id, userEntity.UserName, userEntity.Email);
            var user = _mapper.Map<UserDTO>(userEntity);

            return new AuthUserDTO
            {
                User = user,
                Token = token
            };
        }

        public async Task<AccessTokenDTO> GenerateAccessToken(int userId, string userName, string email)
        {
            var refreshToken = _jwtFactory.GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                UserId = userId
            });

            await _context.SaveChangesAsync();

            var accessToken = await _jwtFactory.GenerateAccessToken(userId, userName, email);

            return new AccessTokenDTO(accessToken, refreshToken);
        }

        public async Task<AccessTokenDTO> RefreshToken(RefreshTokenDTO dto)
        {
            var userId = _jwtFactory.GetUserIdFromToken(dto.AccessToken, dto.SigningKey);
            var userEntity = await _context.Users.FindAsync(userId);

            if (userEntity == null)
            {
                throw new NotFoundException(nameof(User), userId);
            }

            var rToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == dto.RefreshToken && t.UserId == userId);

            if (rToken == null)
            {
                throw new InvalidTokenException("refresh");
            }

            if (!rToken.IsActive)
            {
                throw new ExpiredRefreshTokenException();
            }

            var jwtToken = await _jwtFactory.GenerateAccessToken(userEntity.Id, userEntity.UserName, userEntity.Email);
            var refreshToken = _jwtFactory.GenerateRefreshToken();

            _context.RefreshTokens.Remove(rToken); // delete the token we've exchanged
            _context.RefreshTokens.Add(new RefreshToken // add the new one
            {
                Token = refreshToken,
                UserId = userEntity.Id
            });

            await _context.SaveChangesAsync();

            return new AccessTokenDTO(jwtToken, refreshToken);
        }

        public async Task RevokeRefreshToken(string refreshToken, int userId)
        {
            var rToken = _context.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken && t.UserId == userId);

            if (rToken == null)
            {
                throw new InvalidTokenException("refresh");
            }

            _context.RefreshTokens.Remove(rToken);
            await _context.SaveChangesAsync();
        }
    }
}
