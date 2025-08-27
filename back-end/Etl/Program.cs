using Etl.Business.Authentication;
using Etl.Business.Authentication.Abstractions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISsoLoginUrlProvider, SsoLoginUrlProvider>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.MapGet("/auth/sso-login-url", (ISsoLoginUrlProvider ssoLoginUrlProvider) =>
{
    return Results.Ok(new { loginUrl = ssoLoginUrlProvider.GetSsoLoginUrl() });
});


app.Run();
