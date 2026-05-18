using api_server.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using server_API.BLL;
using server_API.DAL;
using server_API.Mappings;
using server_API.Model;
using System.Text;
using WebApplication1.BLL;
using WebApplication1.BLL.interfaces;
using WebApplication1.DAL;
using WebApplication1.DAL.interfaces;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Services Configuration ---
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddControllers();

// יבוא השכבות
builder.Services.AddScoped<IDonorBLL, DonorBLL>();
builder.Services.AddScoped<IDonorDAL, DonorDAL>();
builder.Services.AddScoped<IGiftBLL, GiftBLL>();
builder.Services.AddScoped<IGiftDAL, GiftDAL>();
builder.Services.AddScoped<ILotteryBLL, LotteryBLL>();
builder.Services.AddScoped<ILotteryDAL, LotteryDAL>();
builder.Services.AddScoped<IPurchaserBLL, PurchaserBLL>();
builder.Services.AddScoped<IPurchaserDAL, PurchaserDAL>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis")
        ?? throw new InvalidOperationException("Connection string 'Redis' was not found.");
    options.InstanceName = "ChineseAuction:";
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ChineseAuction API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(option =>
    option.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(10, TimeSpan.FromSeconds(5), null)));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});

builder.Services.AddProblemDetails();

var app = builder.Build();

// --- 2. Database Migration & Seed ---
for (var attempt = 1; attempt <= 12; attempt++)
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();

        if (!db.Users.Any(u => u.Role == "manager"))
        {
            var admin = new User
            {
                Name = "Admin",
                Email = "admin@gmail.com",
                UserName = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = "manager",
                phone = "0583299284",
                adress = "Jerusalem"
            };
            db.Users.Add(admin);
            db.SaveChanges();
        }
        break;
    }
    catch when (attempt < 12)
    {
        Thread.Sleep(TimeSpan.FromSeconds(5));
    }
}


app.UseSwagger();
app.UseSwaggerUI(c => {
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ChineseAuction API v1");
    c.RoutePrefix = string.Empty; 
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseStatusCodePages();
app.UseCors("AllowAll");

// app.UseHttpsRedirection(); 

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();