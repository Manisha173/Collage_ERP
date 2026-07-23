using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Security.Jwt;
using Owin;

namespace College_ERP.Helpers
{
    public class JwtConfig
    {
        public static void ConfigureJwt(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["JwtIssuer"];
            var audience = ConfigurationManager.AppSettings["JwtAudience"];
            var secret = ConfigurationManager.AppSettings["JwtSecret"];

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = Microsoft.Owin.Security.AuthenticationMode.Active,


                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,

                    ValidIssuer = issuer,
                    ValidAudience = audience,

                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secret)
                    ),

                    ClockSkew = System.TimeSpan.Zero
                }
            });
        }
    }
}