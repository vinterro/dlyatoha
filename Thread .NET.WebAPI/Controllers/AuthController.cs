using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Thread_.NET.BLL.Services;
using Thread_.NET.Common.DTO.User;
using Thread_.NET.DAL.Entities;

namespace Thread_.NET.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(AuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthUserDTO>> Login(UserLoginDTO dto)
        {
            _logger.LogInformation("LOOOOOOGGGGGGgg hahahaha");
            return Ok(await _authService.Authorize(dto));
        }

        [HttpPost("email")]
        [AllowAnonymous]
        public async Task<ActionResult<string>> SendEmail([FromBody] EmailRequest request)
        {
           
            _logger.LogInformation("EMAIIIl SENTED!!!!!");
            return Ok(await _authService.SendEmailAsync(request.Email));

            //if( _userService.IsEmailTaken(email))
            //{
            //} else
            //{
            //    return Ok(await _userService.Authorize(email));
            //}

        }


    }
}