using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PollBuilder.API.Data;
using PollBuilder.API.Hubs;
using System;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Core Framework Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// 2. Setup Database Engine (MS SQL Server)
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Configure CORS for Docker/Local Environment
builder.Services.AddCors(options => {
    options.AddPolicy("CorsPolicy", policy => {
        // Allow the frontend container/localhost access
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. Middlewares Pipeline
app.UseWebSockets();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();

// 5. Map Endpoints 
app.MapControllers();
app.MapHub<PollHub>("/hubs/poll");

// 6. Automatically apply EF Core migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while creating or migrating the database container.");
    }
}

app.Run();