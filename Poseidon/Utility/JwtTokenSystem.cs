using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Poseidon;

public class JwtTokenSystem
{
    public User ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
        var key = Encoding.ASCII.GetBytes(secretKey);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var uid = jwtToken.Claims.First(x => x.Type == "uid").Value;
            var usn = jwtToken.Claims.First(x => x.Type == "usn").Value;
            User user = new User();
            user.uid = uid;
            user.usn = usn;

            return user;
        }
        catch
        {
            return null;
        }
    }
}

