var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;


var services = builder.Services;

services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services
    .AddOptions<RabbitMqConfiguration>()
    .Bind(config.GetSection(nameof(RabbitMqConfiguration)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();
