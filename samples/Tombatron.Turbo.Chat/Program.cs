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
