using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thread_.NET.BLL.Services;
using Thread_.NET.Common.DTO.User;
using Thread_.NET.DAL.Entities;
using Thread_.NET.Extensions;
using Thread_.NET.WebAPI.Controllers;

namespace Thread_.NET.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<UserService> _logger;
        public UsersController(UserService userService, ILogger<UserService> logger)
        {
            _userService = userService;
            _logger = logger;
        }



        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> ChangePassword([FromBody] PasswordTokenRequest request)
        {
            _logger.LogInformation($"Message sending{request.Password}");
            return Ok(await _userService.UpdateUserYes(request));
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<ICollection<UserDTO>>> Get()
        {
            return Ok(await _userService.GetUsers());
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<UserDTO>> GetById(int id)
        {
            return Ok(await _userService.GetUserById(id));
        }

        [HttpGet("fromToken")]
        public async Task<ActionResult<UserDTO>> GetUserFromToken()
        {
            _logger.LogInformation("Message sent");
            return Ok(await _userService.GetUserById(this.GetUserIdFromToken()));
        }



        

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UserDTO user)
        {
            await _userService.UpdateUser(user);
            return NoContent();
        }

        

            [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _userService.DeleteUser(id);
            return NoContent();
        }
    }
}
