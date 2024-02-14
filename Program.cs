using Microsoft.EntityFrameworkCore;
using Npgsql;
using SprPhone.Data;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<ApiDbContext>(options =>
     options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSingleton<NpgsqlConnection>(_ =>
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    var connection = new NpgsqlConnection(connectionString);
    connection.Open();
    return connection;
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    // options.RoutePrefix = string.Empty;
    options.RoutePrefix = "api";
});

// app.UseHttpsRedirection();

// app.UseAuthorization();

app.MapControllers();

app.Run();