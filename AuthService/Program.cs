using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["jwt:Issuer"],
            ValidAudience = builder.Configuration["jwt:Aud1"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwt:key"]))
        };
    });
   

builder.Services.AddSingleton<ITokenService>(new TokenService());

await using var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/validate", [AllowAnonymous] (UserValidationRequestModel request, HttpContext http, ITokenService tokenService) =>
    {
        if (request is UserValidationRequestModel { UserName: "john.doe", Password: "123456" })
        {
            var token = tokenService.buildToken(builder.Configuration["jwt:key"],
                                                builder.Configuration["jwt:issuer"],
                                                 new[]
                                                {
                                                 builder.Configuration["jwt:Aud"]
                                                 },
                                                 request.UserName);

            return new
            {
                Token = token,
                IsAuthenticated = true
            };
        }
        return new
        {
            Token = string.Empty,
            IsAuthenticated = false
        };
    }).WithName("validate");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

internal record UserValidationRequestModel([Required] string UserName, [Required] string Password);

internal interface ITokenService
{
    string buildToken(string key, string issuer, IEnumerable<string> audience, string userName);
}

internal class TokenService : ITokenService
{
    private TimeSpan ExpiryDuration = new TimeSpan(20, 30, 0);
    public string buildToken(string key, string issuer, IEnumerable<string> audience, string userName)
    {
        var claims = new List<Claim>
       {
           new Claim(JwtRegisteredClaimNames.UniqueName, userName)
       };
        claims.AddRange(audience.Select(aud => new Claim(JwtRegisteredClaimNames.Aud, aud)));
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
            expires: DateTime.Now.Add(ExpiryDuration), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}

