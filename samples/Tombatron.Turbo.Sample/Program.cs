using Tombatron.Turbo;

var builder = WebApplication.CreateBuilder(args);

// Add Turbo services
builder.Services.AddTurbo(options =>
{
    options.HubPath = "/turbo-hub";
    options.UseSignedStreamNames = false;
});

// Add session support for the counter demo
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

// Use session
app.UseSession();

// Use Turbo middleware
app.UseTurbo();

app.UseAuthorization();

app.MapRazorPages();

// Map the Turbo SignalR hub for streaming
app.MapTurboHub();

app.Run();
