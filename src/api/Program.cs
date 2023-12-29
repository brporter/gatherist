using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace api;

public class Program
{
    private const string ResponseType = "code";
    private const string Scope = "openid email";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
        });
        
        builder.Services.AddSingleton<HttpClient>();
        builder.Services.AddSingleton<JwtConfiguration>();

        var app = builder.Build();

        var sampleTodos = new Todo[] {
            new(1, "Walk the dog"),
            new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
            new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
            new(4, "Clean the bathroom"),
            new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
        };

        var todosApi = app.MapGroup("/todos");
        todosApi.MapGet("/", () => sampleTodos);
        todosApi.MapGet("/{id}", (int id) =>
            sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
                ? Results.Ok(todo)
                : Results.NotFound());

        var tokenApi = app.MapGroup("/token");

        tokenApi.MapGet("/{provider}", (string provider, JwtConfiguration configuration) =>
        {
            if (!configuration.Providers.TryGetValue(provider.ToLowerInvariant(), out var providerConfiguration))
            {
                return Results.Problem("No configuration for the requested provider was found.", null, 400);
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

            // Response.Cookies.Append("state", state.ToString(), new CookieOptions()
            // {
            //     IsEssential = true,
            //     Expires = DateTime.UtcNow.AddMinutes(10),
            //     Secure = true,
            //     HttpOnly = true,
            //     SameSite = SameSiteMode.Lax
            // });

            return Results.Redirect(uri.ToString());
        });


        var tokenRedeemHandler = async (string provider, string state, string code, JwtConfiguration configuration, HttpClient client) =>
        {
            if (!configuration.Providers.TryGetValue(provider, out var providerConfiguration))
            {
                return Results.Problem("No configuration for the requested provider was found.", null, 400);
            }

            // TODO
            // if (!Request.Cookies.TryGetValue("state",  out var storedState)
            //     || !state.Equals(storedState, StringComparison.OrdinalIgnoreCase))
            // {
            //     return BadRequest();
            // }

            var result = await client.PostAsync(providerConfiguration.TokenEndpoint, new FormUrlEncodedContent(new[]
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
                return Results.BadRequest();

            var token = GenerateJwtToken(provider, email, configuration.SecretKey, configuration.Issuer,
                configuration.Audience);

            var tokenString = _tokenHandler.WriteToken(token);
            
            // Response.Cookies.Append("token", tokenString, new CookieOptions()
            // {
            //     IsEssential = true,
            //     Expires = token.ValidTo,
            //     Secure = true,
            //     HttpOnly = true,
            //     SameSite = SameSiteMode.Lax,
            //     Domain = string.IsNullOrWhiteSpace(_configuration.Domain) ? "localhost" : _configuration.Domain 
            // });

            return Results.Ok(tokenString);
            // return Ok(tokenString);
        };
        
        
        tokenApi.MapGet("/{provider}/redeem", tokenRedeemHandler);
        tokenApi.MapPost("/{provider}/redeem", tokenRedeemHandler);

        app.Run();
    }

    private static readonly JwtSecurityTokenHandler _tokenHandler = new();

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

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(ProblemDetails))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}