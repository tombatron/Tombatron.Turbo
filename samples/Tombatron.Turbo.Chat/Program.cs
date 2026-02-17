using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Tombatron.Turbo;
using Tombatron.Turbo.Chat;
using Tombatron.Turbo.Chat.Data;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environments.Development
});

// Add Turbo services
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";

    // Stimulus
    options.ImportMap.Pin("@hotwired/stimulus",
        "https://unpkg.com/@hotwired/stimulus@3.2.2/dist/stimulus.js");

    // Application entry point (preload triggers auto-import)
    options.ImportMap.Pin("application", "/js/application.js", preload: true);

    // Controller modules
    options.ImportMap.Pin("controllers", "/js/controllers/index.js");
    options.ImportMap.Pin("controllers/application", "/js/controllers/application.js");
    options.ImportMap.Pin("controllers/chat", "/js/controllers/chat_controller.js");
    options.ImportMap.Pin("controllers/typing-indicator", "/js/controllers/typing_indicator_controller.js");
    options.ImportMap.Pin("controllers/profile-sidebar", "/js/controllers/profile_sidebar_controller.js");
    options.ImportMap.Pin("controllers/chat-message", "/js/controllers/chat_message_controller.js");
    options.ImportMap.Pin("controllers/create-room", "/js/controllers/create_room_controller.js");
});

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
