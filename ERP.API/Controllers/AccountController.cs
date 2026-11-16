using ERP.Application.Auth;
using ERP.Application.Auth.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace ERP.API.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController(IAuthService authService) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
        {
            var authResponse = await authService.CreateUserAsync(registerUserDto);

            if (authResponse.IsAuthenticated)
            {
                //SetRefreshTokenToCookie
                return Ok(authResponse);
            }
            else
            {
                return BadRequest(authResponse.ErrorMessage);
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDto loginUserDto)
        {
            var authResponse = await authService.GetTokenAsync(loginUserDto);

            if (!authResponse.IsAuthenticated)
                return BadRequest(authResponse.ErrorMessage);

            if(!String.IsNullOrEmpty(authResponse.RefreshToken))
                SetRefreshTokenToCookie(authResponse.RefreshToken!, authResponse.RefreshTokenExpiration);

            return Ok(authResponse);
            
        }

        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var token = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required");

            var authResponse = await authService.RefreshTokenAsync(token);

            if (!authResponse.IsAuthenticated)
                return BadRequest(authResponse.ErrorMessage);

            SetRefreshTokenToCookie(authResponse.RefreshToken!, authResponse.RefreshTokenExpiration);

            return Ok(authResponse);

        }

        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody]RefreshTokenDto tokenDto)
        {
            var token = tokenDto.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is required");

            var result = await authService.RevokeTokenAsync(token);

            if (!result)
                return BadRequest("Invalid token");

            return Ok("Token revoked successfully.");
        }

        private void SetRefreshTokenToCookie(string refreshToken, DateTime expires)
        {
            var CookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime(),
                //SameSite = SameSiteMode.Lax,
                //Secure = false
            };


            Response.Cookies.Append("refreshToken", refreshToken, CookieOptions);
        }
    }
}
