using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Aspire ServiceDefaults
builder.AddServiceDefaults();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", pb =>
        pb.WithOrigins("http://localhost:5173", "https://localhost:7100", "http://localhost:7100")
          .AllowAnyHeader()
          .AllowAnyMethod());
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔐 B2C auth: use AzureAdB2C section
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"),
        jwtBearerScheme: JwtBearerDefaults.AuthenticationScheme,
        subscribeToJwtBearerMiddlewareDiagnosticsEvents: true)
    // accept either audience form (GUID clientId or AppId URI)
    .EnableTokenAcquisitionToCallDownstreamApi() // harmless for pure API; keeps defaults consistent
    .AddInMemoryTokenCaches();

builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var apiClientId = builder.Configuration["AzureAdB2C:ClientId"];
    var appIdUri = builder.Configuration["AzureAdB2C:AppIdUri"];

    options.TokenValidationParameters ??= new TokenValidationParameters();
    options.TokenValidationParameters.ValidAudiences = new[]
    {
        apiClientId, // e.g., ca776d36-138a-4e85-83f7-1bc07e5dd582
        appIdUri     // e.g., https://gearhawkadb2c.onmicrosoft.com/ca776d36-...
    };

    // optional but helpful during bring-up
    options.Events ??= new JwtBearerEvents();
    options.Events.OnAuthenticationFailed = ctx =>
    {
        Console.WriteLine(ctx.Exception.ToString());
        return Task.CompletedTask;
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DevCors");
app.UseAuthentication();
app.UseAuthorization();

// Aspire health endpoints in Development
app.MapDefaultEndpoints();

app.MapControllers();

app.Run();
