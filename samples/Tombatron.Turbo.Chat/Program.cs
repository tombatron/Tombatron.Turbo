using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Tombatron.Turbo;
using Tombatron.Turbo.Stimulus;
using Tombatron.Turbo.Chat;
using Tombatron.Turbo.Chat.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environments.Development
});

// Add Turbo services
builder.Services.AddTurbo();

builder.Services.AddStimulus(options => options.ControllersPath = "js/controllers");

// EF Core + SQLite
builder.Services.AddDbContext<ChatDbContext>(opt =>
    opt.UseSqlite("Data Source=chat.db"));

// Cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
    });

// Register chat service as scoped (matches DbContext lifetime)
builder.Services.AddScoped<ChatService>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseTurbo();
app.UseAuthorization();

app.MapRazorPages();
app.MapTurboHub();

app.Run();
