using System;
using System.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace Umbraco.RestApi
{
    /// <summary>
    /// Used to write out jwt tokens
    /// </summary>
    /// <remarks>
    /// For some oddball reason microsoft doesn't support this ootb with the normal JwtFormat class, it only unprotects so conveniently we need
    /// to implement this ourselves
    /// see http://odetocode.com/blogs/scott/archive/2015/01/15/using-json-web-tokens-with-katana-and-webapi.aspx
    /// </remarks>
    internal class JwtFormatWriter : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly OAuthAuthorizationServerOptions _options;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly string _base64Key;

        public JwtFormatWriter(OAuthAuthorizationServerOptions options, string issuer, string audience, string base64Key)
        {
            _options = options;
            _issuer = issuer;
            _audience = audience;
            _base64Key = base64Key;
        }

        public string SignatureAlgorithm => "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256";
        public string DigestAlgorithm => "http://www.w3.org/2001/04/xmlenc#sha256";

        public string Protect(AuthenticationTicket data)
        {
            if (data == null) throw new ArgumentNullException("data");
            
            var issuer = _issuer;
            var audience = _audience;
            var key = Convert.FromBase64String(_base64Key);
            //TODO: Validate key length, must be at least 128
            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(_options.AccessTokenExpireTimeSpan.TotalMinutes);
            var signingCredentials = new SigningCredentials(
                new InMemorySymmetricSecurityKey(key),
                SignatureAlgorithm,
                DigestAlgorithm);
            var token = new JwtSecurityToken(issuer, audience, data.Identity.Claims, now, expires, signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);            
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}