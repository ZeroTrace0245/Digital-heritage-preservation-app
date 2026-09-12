using Microsoft.EntityFrameworkCore;
using DigitalHeritageApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<HeritageDbContext>(options =>
    options.UseSqlite("Data Source=heritage.db"));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations and seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HeritageDbContext>();
    context.Database.EnsureCreated();
    // Ensure community tables exist for installations created before this feature.
    context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS CommunityGroups (
        Id INTEGER NOT NULL CONSTRAINT PK_CommunityGroups PRIMARY KEY AUTOINCREMENT,
        Name TEXT NOT NULL, Description TEXT NULL, Region TEXT NULL, Visibility TEXT NOT NULL,
        CreatedById INTEGER NOT NULL, CreatedAt TEXT NOT NULL);");
    context.Database.ExecuteSqlRaw(@"CREATE TABLE IF NOT EXISTS CommunityItems (
        Id INTEGER NOT NULL CONSTRAINT PK_CommunityItems PRIMARY KEY AUTOINCREMENT,
        Type TEXT NOT NULL, Title TEXT NOT NULL, Description TEXT NOT NULL, Tags TEXT NULL,
        Visibility TEXT NOT NULL, ConsentConfirmed INTEGER NOT NULL, IsSensitive INTEGER NOT NULL,
        Status TEXT NOT NULL, ContributorId INTEGER NOT NULL, GroupId INTEGER NULL, ParentItemId INTEGER NULL,
        VerifiedById INTEGER NULL, VerifiedAt TEXT NULL, CreatedAt TEXT NOT NULL);");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();
