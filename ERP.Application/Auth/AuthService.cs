using AutoMapper;
using ERP.Application.Auth.Dtos;
using ERP.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ERP.Application.Auth;

internal class AuthService(UserManager<AppUser> userManager,
    IMapper mapper,
    IOptions<JwtOptions> Jwt) : IAuthService
{
    private readonly JwtOptions _jwt = Jwt.Value;
    public async Task<AuthResponse> CreateUserAsync(RegisterUserDto userDto)
    {
        var authresponse = new AuthResponse();

        var userFromDb = await userManager.FindByEmailAsync(userDto.Email);

        if (userFromDb != null) {

            authresponse.ErrorMessage = "Email is already Exists";

            return authresponse;
        }

        var appUser = mapper.Map<AppUser>(userDto);

        var identityResult = await userManager.CreateAsync(appUser,userDto.Password);

        if (!identityResult.Succeeded) {

            var errors = String.Empty;

            foreach (var error in identityResult.Errors)
            {
                errors += $"{error.Description} ";
            }

            authresponse.ErrorMessage = errors;

            return authresponse;

        }

        // add role to user

        authresponse.IsAuthenticated = true;
        authresponse.Email = appUser.Email!;
        //authresponse.Name = ;
        //authresponse.Role

        var jwtsecuritytoken = await GenerateToken(appUser);
        authresponse.Token = new JwtSecurityTokenHandler().WriteToken(jwtsecuritytoken);
        var refreshtoken = GenerateRefreshToken();
        authresponse.RefreshToken = refreshtoken.Token;
        authresponse.RefreshTokenExpiration = refreshtoken.ExpiresOn;

        appUser.RefreshTokens.Add(refreshtoken);
        await userManager.UpdateAsync(appUser);

        return authresponse;
    }

    public async Task<AuthResponse> GetTokenAsync(LoginUserDto loginuserdto)
    {
        var authresponse = new AuthResponse();
        var userFromDb = await userManager.FindByEmailAsync(loginuserdto.Email);


        if (userFromDb is null || !await userManager.CheckPasswordAsync(userFromDb, loginuserdto.Password))
        {

            authresponse.ErrorMessage = "Wrong email or password";
            authresponse.IsAuthenticated = false;
            return authresponse;
        }


        var jwtsecuritytoken = await GenerateToken(userFromDb);

       // var roles = await userManager.GetRolesAsync(user);

        authresponse.Token = new JwtSecurityTokenHandler().WriteToken(jwtsecuritytoken);
        authresponse.IsAuthenticated = true;
        authresponse.Email = userFromDb.Email!;
        //authresponse.Name = ;
        //authresponse.Role = ;

        if (userFromDb.RefreshTokens.Any(t => t.IsActive))
        {
            var refreshtoken = userFromDb.RefreshTokens.FirstOrDefault(t => t.IsActive);
            authresponse.RefreshToken = refreshtoken!.Token;
            authresponse.RefreshTokenExpiration = refreshtoken!.ExpiresOn;
        }
        else
        {
            var refreshtoken = GenerateRefreshToken();
            authresponse.RefreshToken = refreshtoken!.Token;
            authresponse.RefreshTokenExpiration = refreshtoken!.ExpiresOn;
            userFromDb.RefreshTokens.Add(refreshtoken);
            await userManager.UpdateAsync(userFromDb);
        }
        return authresponse;
    }
    public async Task<AuthResponse> RefreshTokenAsync(string token)
    {
        var authresponse = new AuthResponse();
        
        //if (string.IsNullOrWhiteSpace(token))
        //{
        //    authresponse.ErrorMessage = "Invalid token";
        //    return authresponse;
        //}
        var user = await userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens!.Any(t => t.Token == token));

        if (user is null)
        {
            authresponse.ErrorMessage = "Invalid token";
            return authresponse;
        }

        var refreshtoken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshtoken.IsActive)
        {
            authresponse.ErrorMessage = "Inactive token";
            return authresponse;
        }

        refreshtoken.RevokedOn = DateTime.UtcNow;

        var newrefreshtoken = GenerateRefreshToken();
        user.RefreshTokens.Add(newrefreshtoken);
        await userManager.UpdateAsync(user);

        var jwtsecuritytoken = await GenerateToken(user);
        authresponse.Token = new JwtSecurityTokenHandler().WriteToken(jwtsecuritytoken);
        authresponse.IsAuthenticated = true;
        authresponse.RefreshTokenExpiration = newrefreshtoken.ExpiresOn;
        authresponse.RefreshToken = newrefreshtoken.Token;
        authresponse.Email = user.Email!;
        //authresponse.Name = $"{user.Fname} {user.Lname}";
        //var roles = await _usermanager.GetRolesAsync(user);
        //authresponse.Roles = roles.ToList();
        return authresponse;

    }
    public async Task<bool> RevokeTokenAsync(string token)
    {
        //if (string.IsNullOrWhiteSpace(token))
        //{
        //    return false;
        //}
        var user = await userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
        if (user is null)
        {
            return false;
        }

        var refreshtoken = user.RefreshTokens.Single(t => t.Token == token);

        if (!refreshtoken.IsActive)
        {
            return false;
        }


        refreshtoken.RevokedOn = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return true;
    }
    private async Task<JwtSecurityToken> GenerateToken(AppUser user)
    {
        // Genetate JWT token here and return it
        var roles = await userManager.GetRolesAsync(user);

        List<Claim> claims = new List<Claim>();
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
        claims.Add(new Claim(ClaimTypes.Email, user.Email!));
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SignInKey));

        var signincredintials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken jwtsecuritytoken = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
            signingCredentials: signincredintials
            );

        return jwtsecuritytoken;
    }

    private RefreshToken GenerateRefreshToken()
    {
        var randomnum = new byte[32];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(randomnum);

        return new RefreshToken
        {
            Token = Convert.ToBase64String(randomnum),
            CreatedOn = DateTime.UtcNow,
            ExpiresOn = DateTime.UtcNow.AddDays(10),
        };

    }

}
