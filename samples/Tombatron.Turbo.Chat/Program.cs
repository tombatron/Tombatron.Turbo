using Tombatron.Turbo;
using Tombatron.Turbo.Chat;

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
});

// Add session for user identity (in a real app, use proper authentication)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register our chat service
builder.Services.AddSingleton<ChatService>();

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseTurbo();
app.UseAuthorization();

app.MapRazorPages();
app.MapTurboHub();

app.Run();
