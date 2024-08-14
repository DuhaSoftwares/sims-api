using Duha.SIMS.API.Security;
using Duha.SIMS.BAL.AppUser;
using Duha.SIMS.BAL.Client;
using Duha.SIMS.BAL.Interface;
using Duha.SIMS.BAL.Security;
using Duha.SIMS.BAL.Token;
using Duha.SIMS.Config;
using Duha.SIMS.DAL.Contexts;
using Duha.SIMS.ServiceModels.AppUsers.AutheticUser;
using Duha.SIMS.ServiceModels.LoggedInIdentity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<APIConfiguration>(builder.Configuration.GetSection("APIConfiguration"));

// Retrieve APIConfiguration from the configuration
var apiConfiguration = builder.Configuration.GetSection("APIConfiguration").Get<APIConfiguration>();


var connectionString = apiConfiguration.ApiDbConnectionString;

builder.Services.AddDbContext<ApiDbContext>(options =>
    options.UseSqlServer(connectionString, 
        sqlOptions => sqlOptions.MigrationsAssembly("Duha.SIMS.API")));
builder.Services.AddScoped<IPasswordEncryptHelper, PasswordEncryptHelper>();

builder.Services.AddAutoMapper(typeof(Program)); // Use AutoMapper.Extensions.Microsoft.DependencyInjection
builder.Services.AddScoped<ILoginUserDetail, LoginUserDetail>();
// Register ClientUserProcess
builder.Services.AddScoped<ClientUserProcess>();
builder.Services.AddScoped<ApplicationUserProcess>();
builder.Services.AddScoped<TokenProcess>();
builder.Services.AddScoped<ClientCompanyDetailsProcess>();

// Add Identity services
builder.Services.AddIdentity<AuthenticUserSM, IdentityRole>()
    .AddEntityFrameworkStores<ApiDbContext>()
    .AddDefaultTokenProviders();

// Configure JwtHandler
builder.Services.AddScoped<JwtHandler>(provider =>
{
    var issuer = builder.Configuration["Jwt:Issuer"];
    return new JwtHandler(issuer);
});

// Add custom JWT authentication handler
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = DuhaBearerTokenAuthHandlerRoot.DefaultSchema;
    options.DefaultChallengeScheme = DuhaBearerTokenAuthHandlerRoot.DefaultSchema;
})
.AddScheme<DuhaAuthenticationSchemeOptions, DuhaBearerTokenAuthHandlerRoot>(DuhaBearerTokenAuthHandlerRoot.DefaultSchema, options =>
{
    options.JwtTokenSigningKey = builder.Configuration["Jwt:Key"];
});

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Preserve object references during JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SIMS API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter Token Only (Without 'Bearer')",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    // Assign the security requirements to the operations
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add CORS policy
builder.Services.AddCors(p => p.AddPolicy("corspolicy", build =>
{
    build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("corspolicy");

app.MapControllers();

app.Run();



