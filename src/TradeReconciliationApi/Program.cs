using Microsoft.EntityFrameworkCore;
using TradeReconciliation.Core.Data;
using TradeReconciliation.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<TradeReconciliationDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<IMatchingStrategy, ExactMatchStrategy>();
builder.Services.AddScoped<IReconciliationService, ReconciliationService>();
builder.Services.AddScoped<IPnLCalculator, PnLCalculator>();
builder.Services.AddScoped<ITradeImporter, CsvTradeImporter>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TradeReconciliationDbContext>();
    db.Database.Migrate();
    DataSeeder.Seed(db);
}

app.Run();
