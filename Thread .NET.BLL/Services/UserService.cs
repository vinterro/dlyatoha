using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thread_.NET.BLL.Exceptions;
using Thread_.NET.BLL.Services.Abstract;
using Thread_.NET.Common.DTO.User;
using Thread_.NET.Common.Security;
using Thread_.NET.DAL.Context;
using Thread_.NET.DAL.Entities;
using Thread_.NET.BLL.JWT;
using Thread_.NET.DAL.Entities.Abstract;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;

namespace Thread_.NET.BLL.Services
{
    public sealed class UserService : BaseService
    {
        private readonly JwtFactory _jwtFactory;
        private readonly ILogger<UserService> _logger;
        public UserService(ThreadContext context, JwtFactory jwtFactory, IMapper mapper, ILogger<UserService> logger) : base(context, mapper) {
            _jwtFactory = jwtFactory;
            _logger = logger;
        }


        










        public async Task<ICollection<UserDTO>> GetUsers()
        {
            var users = await _context.Users
                .Include(x => x.Avatar)
                .ToListAsync();

            return _mapper.Map<ICollection<UserDTO>>(users);
        }

        public async Task<UserDTO> GetUserById(int id)
        {
            var user = await GetUserByIdInternal(id);
            if (user == null)
            {
                throw new NotFoundException(nameof(User), id);
            }

            return _mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> CreateUser(UserRegisterDTO userDto)
        {
           
            if (IsEmailTaken(userDto.Email))
            {
                throw new EmailTakenException();
            }

            var userEntity = _mapper.Map<User>(userDto);
            var salt = SecurityHelper.GetRandomBytes();

            userEntity.Salt = Convert.ToBase64String(salt);
            userEntity.Password = SecurityHelper.HashPassword(userDto.Password, salt);

            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDTO>(userEntity);
        }

        public async Task UpdateUser(UserDTO userDto)
        {
            var userEntity = await GetUserByIdInternal(userDto.Id);

            if (IsEmailTaken(userDto.Email) && userEntity.Email != userDto.Email)
            {
                throw new EmailTakenException();
            }

            if (userEntity == null)
            {
                throw new NotFoundException(nameof(User), userDto.Id);
            }

            var timeNow = DateTime.Now;

            userEntity.Email = userDto.Email;
            userEntity.UserName = userDto.UserName;
            userEntity.UpdatedAt = timeNow;

            if (!string.IsNullOrEmpty(userDto.Avatar))
            {
                if (userEntity.Avatar == null)
                {
                    userEntity.Avatar = new Image
                    {
                        URL = userDto.Avatar
                    };
                }
                else
                {
                    userEntity.Avatar.URL = userDto.Avatar;
                    userEntity.Avatar.UpdatedAt = timeNow;
                }
            }
            else
            {
                if (userEntity.Avatar != null)
                {
                    _context.Images.Remove(userEntity.Avatar);
                }
            }

            _context.Users.Update(userEntity);
            await _context.SaveChangesAsync();
        }

        public async Task<PasswordTokenRequest> UpdateUserYes(PasswordTokenRequest passwordTokenRequest)
        {

            Console.WriteLine("ha");
            var userId = await _jwtFactory.GetUserIdFromTokenAsync(passwordTokenRequest.Token, passwordTokenRequest.SigningKey);

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                throw new NotFoundException(nameof(User), userId);
            }

            var timeNow = DateTime.Now;

            user.UpdatedAt = timeNow;

            var salt = SecurityHelper.GetRandomBytes();
            user.Salt = Convert.ToBase64String(salt);

            user.Password = SecurityHelper.HashPassword(passwordTokenRequest.Password, salt);

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Message DOOOOONE{user.Password}");

            return passwordTokenRequest;
        }

        public async Task DeleteUser(int userId)
        {
            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (userEntity == null)
            {
                throw new NotFoundException(nameof(User), userId);
            }

            _context.Users.Remove(userEntity);
            await _context.SaveChangesAsync();
        }

        private async Task<User> GetUserByIdInternal(int id)
        {
            return await _context.Users
                .Include(u => u.Avatar)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public bool IsEmailTaken(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }
    }
    
}
