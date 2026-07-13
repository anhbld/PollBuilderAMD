using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PollBuilder.API.Data;
using PollBuilder.API.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Core Framework Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 
builder.Services.AddSignalR();

// 2. Setup Database Engine
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("PollEngineDb"));

// 3. Configure CORS (Critical for development testing)
builder.Services.AddCors(options => {
    options.AddPolicy("CorsPolicy", policy => {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "http://localhost:51187")
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

// 4. Middlewares must be ordered strictly like this
app.UseCors("CorsPolicy");
app.UseAuthorization();

// 5. Serve static files FIRST before trying to route standard endpoints
app.UseDefaultFiles(); // <-- Enforces serving index.html on root '/' request
app.UseStaticFiles();  // <-- Serves JavaScript, CSS, images out of /wwwroot

app.UseRouting();

// 6. Map Endpoints 
app.MapControllers();
app.MapHub<PollHub>("/hubs/poll");

// 7. Fallback route must be last to delegate all client-side URL sub-routes back to React
app.MapFallbackToFile("index.html");

app.Run();