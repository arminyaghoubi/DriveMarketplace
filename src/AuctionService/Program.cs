using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AuctionDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddMassTransit(configure =>
{
    configure.AddEntityFrameworkOutbox<AuctionDbContext>(config =>
    {
        config.QueryDelay = TimeSpan.FromSeconds(10);
        config.UsePostgres();
        config.UseBusOutbox();
    });

    configure.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();

    configure.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

    configure.UsingRabbitMq((context, config) =>
    {
        config.ConfigureEndpoints(context);
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["IdentityServiceUrl"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters.ValidateAudience = false;
        options.TokenValidationParameters.NameClaimType = "username";
    });

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDatabase(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

app.Run();
