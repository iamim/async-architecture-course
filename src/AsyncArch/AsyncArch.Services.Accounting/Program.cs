using AsyncArch.Services.Accounting;
using AsyncArch.Services.Accounting.Db;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddDbContext<Context>(
    opt => 
        opt
            .UseNpgsql(builder.Configuration.GetConnectionString("Postgres"))
            .UseSnakeCaseNamingConvention()
);

builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<KafkaConsumer>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Accounting service"));

app.UseAuthorization();

app.MapControllers();

app.Run();