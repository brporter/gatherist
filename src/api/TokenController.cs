using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using api.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace api;

[ApiController]
[Route("[controller]")]
public class TokenController 
    : Controller
{
    private const string ResponseType = "code";
    private const string Scope = "openid email";
    
    private static readonly JwtSecurityTokenHandler _tokenHandler = new();

    private readonly JwtConfiguration _configuration;
    private readonly HttpClient _client;

    public TokenController(HttpClient client, JwtConfiguration jwtConfig)
    {
        _configuration = jwtConfig ?? throw new ArgumentNullException(nameof(jwtConfig));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    [HttpGet("/[controller]/{provider}")]
    public IActionResult Authenticate(string provider)
    {
        if (!_configuration.Providers.TryGetValue(provider.ToLowerInvariant(), out var providerConfiguration))
        {
            return Problem("No configuration for the requested provider was found.", null, 400);
        }

        var clientId = providerConfiguration.ClientId;

        var redirect_uri = providerConfiguration.RedirectUri.ToString();
        var state = Guid.NewGuid();
        var nonce = Guid.NewGuid();

        var uri = new UriBuilder(providerConfiguration.AuthEndpoint)
        {
            Query = string.Format(providerConfiguration.AuthEndpointQueryFormat, ResponseType, clientId, Scope,
                redirect_uri, state.ToString(), nonce.ToString())
        };

        Response.Cookies.Append("state", state.ToString(), new CookieOptions()
        {
            IsEssential = true,
            Expires = DateTime.UtcNow.AddMinutes(10),
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax
        });

        return Redirect(uri.ToString());
    }

    [AcceptVerbs("GET", "POST", Route="/[controller]/{provider}/redeem")]
    public async Task<IActionResult> ConvertTokenAsync(string provider, string state, string code)
    {
        if (!_configuration.Providers.TryGetValue(provider, out var providerConfiguration))
        {
            return Problem("No configuration for the requested provider was found.", null, 400);
        }

        if (!Request.Cookies.TryGetValue("state",  out var storedState)
            || !state.Equals(storedState, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest();
        }

        var result = await _client.PostAsync(providerConfiguration.TokenEndpoint, new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("client_id", providerConfiguration.ClientId),
            new KeyValuePair<string, string>("client_secret", providerConfiguration.ClientSecret),
            new KeyValuePair<string, string>("redirect_uri", providerConfiguration.RedirectUri.ToString()),
            new KeyValuePair<string, string>("grant_type", "authorization_code")
        })).ConfigureAwait(false);

        var content = await result.Content.ReadAsStringAsync();
        var json = JsonNode.Parse(content) ?? throw new ArgumentException();

        var id_token = json["id_token"]?.GetValue<string>() ?? throw new ArgumentException();

        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(id_token);

        var email = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == "email")?.Value;

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest();

        var token = GenerateJwtToken(provider, email, _configuration.SecretKey, _configuration.Issuer,
            _configuration.Audience);

        var tokenString = _tokenHandler.WriteToken(token);
        
        Response.Cookies.Append("token", tokenString, new CookieOptions()
        {
            IsEssential = true,
            Expires = token.ValidTo,
            Secure = true,
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Domain = string.IsNullOrWhiteSpace(_configuration.Domain) ? "localhost" : _configuration.Domain 
        });

        return Ok(tokenString);
    }

    private static JwtSecurityToken GenerateJwtToken(string provider, string email, string secretKey, string issuer, string audience)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("provider", provider),
            new Claim("tenant_id", "123")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(1);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        return token;
    }
}
