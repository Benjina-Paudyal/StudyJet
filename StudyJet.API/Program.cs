using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using StudyJet.API.Data;
using StudyJet.API.Data.Entities;

var builder = WebApplication.CreateBuilder(args);

// Ensure environment variables are replaced in configuration before use
var config = builder.Configuration;

// Get environment variables first
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
var defaultPassword = Environment.GetEnvironmentVariable("DEFAULT_PASSWORD");

// Ensure critical environment variables are set
if (string.IsNullOrEmpty(dbPassword))
    throw new InvalidOperationException("DB_PASSWORD environment variable is not set.");
if (string.IsNullOrEmpty(defaultPassword))
    throw new InvalidOperationException("DEFAULT_PASSWORD environment variable is not set.");

// Replace the placeholder manually in connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection").Replace("${DB_PASSWORD}", dbPassword);

// Adding DbContext with connection string
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));


// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddLogging();
builder.Services.AddHttpClient();

// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAny",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Adding Identity services and configuring the user and role
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();


// Configuring Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.SignIn.RequireConfirmedEmail = true;

});


// Configuring Identity token lifespan
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromHours(2);
});

// Register DbInitializer
builder.Services.AddScoped<DbInitializer>();

// Registering UserManager, RoleManager
builder.Services.AddScoped<UserManager<User>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();


// Swagger/OpenAPI 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    // Configure Swagger to use JWT Bearer authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new string[] {}
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseCors("AllowAny");

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Seed the data
using (var scope = app.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    await dbInitializer.InitializeAsync().ConfigureAwait(false);
}

app.Run();
