using NotificationService.DataAccess;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;


var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.AddDataAccess(Environment.GetEnvironmentVariable("ConnectionStrings_NotificationService_Postgres") ?? 
                       config.GetConnectionString("PostgresDb") ?? 
                       throw new Exception("No connection string to sql database"));

// services
//     .AddOptions<RabbitMqConfiguration>()
//     .Bind(config.GetSection(nameof(RabbitMqConfiguration)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
