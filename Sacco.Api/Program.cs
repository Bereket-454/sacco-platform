using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles; // Change to IgnoreCycles
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    // Ensure the serializer preserves special characters
    options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Register Swagger services
builder.Services.AddEndpointsApiExplorer();  // for minimal APIs
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Multi-tenant SACCO Management System API", Version = "v1" });

    // Add JWT authentication support
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer prefix (e.g., 'Bearer your_token_here')",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options => options.AddPolicy("AllowSpecificOrigin",
                policy =>
                {
                    policy.AllowAnyOrigin()
                    .AllowAnyMethod() // Allow all HTTP methods (GET, POST, PUT, DELETE)
                    .AllowAnyHeader(); // Allow all headers
                }));

/*builder.Services.AddDbContext<MasterDbContext>(option =>
{
    option.UseNpgsql(builder.Configuration.GetConnectionString("MasterDbConnection"));
});

// JWT Authentication Configuration
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Set to true in production for secure communication
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!))
        };
    });*/

// Register services globally
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
// Register custom application layers
/*builder.Services.AddApplicationServices();
// Register custom infrastructure layers
builder.Services.AddInfrastructureServices();

// Register TenantDbContext
builder.Services.AddDbContext<TenantDbContext>();

// Bind the AppSettings section
builder.Services.Configure<AppBasicSettingOptions>(builder.Configuration.GetSection("AppBasicSettings"));
*/
// Register signalR for real-time data
builder.Services.AddSignalR();

var app = builder.Build();

// SignalR hub for real-time data transmission

// Middleware pipeline
app.UseDeveloperExceptionPage();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Multi-tenant SACCO Management System API v1");
        c.RoutePrefix = ""; // Optional: Swagger on root URL
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Multi-tenant SACCO Management System API v1");
        c.RoutePrefix = ""; // Optional: Swagger on root URL
    });
}

app.UseCors("AllowSpecificOrigin");
app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// Permission middleware for user permission
//app.UseMiddleware<PermissionMiddleware>();
app.MapControllers();

// Redirect all other requests to the Angular app's entry point (index.html)
app.MapFallbackToFile("index.html");
await app.RunAsync();
