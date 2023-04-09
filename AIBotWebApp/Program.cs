using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using DiscordBot;
using DiscordBot.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddControllers();

builder.Services.AddOptions<BotConfiguration>().Configure<IConfiguration>((botConfiguration, configuration) =>
{
    botConfiguration.Prefix = configuration["Prefix"] ?? throw new NullReferenceException("Command Prefix Is Not Specified");
    botConfiguration.Token = configuration["DiscordToken"] ?? throw new NullReferenceException("Discord Token Is Not Specified");
    botConfiguration.WebChannelId = ulong.Parse(configuration["WebChannelId"] ?? throw new NullReferenceException("Web Channel ID Is Not Specified"));
});

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOAuth("Discord", opt =>
{
    opt.AuthorizationEndpoint = "https://discord.com/oauth2/authorize";
    opt.Scope.Add("identify");
    opt.Scope.Add("guilds");
    opt.Scope.Add("guilds.members.read");
    opt.CallbackPath = new PathString("/auth/oauthCallback");

    opt.ClientId = builder.Configuration.GetValue<string>("Discord:ClientId");
    opt.ClientSecret = builder.Configuration.GetValue<string>("Discord:ClientSecret");

    opt.TokenEndpoint = "https://discord.com/api/oauth2/token";
    opt.UserInformationEndpoint = "https://discord.com/api/users/@me";
    
    opt.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    opt.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");
    opt.ClaimActions.MapJsonKey(ClaimTypes.Hash, "avatar");

    opt.AccessDeniedPath = "/discordoauthfailed";

    opt.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;
            
            context.RunClaimActions(user);
        }
    };
});
builder.Services.AddSingleton<IBotService, BotService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapBlazorHub().RequireAuthorization(
    new AuthorizeAttribute 
    {
        AuthenticationSchemes = "Discord"
    });
app.MapFallbackToPage("/_Host");

var botService = app.Services.GetService<IBotService>();
var bot = botService.CreateBot();

await bot.RunAsync().ConfigureAwait(false);

app.UseAuthentication();

app.MapGet("/discordoauthfailed", () => $"{bot.Client.CurrentUser.Username} Disocrd OAuth failed L");
app.MapGet("/auth/oauthCallback", () => $"{bot.Client.CurrentUser.Username} Discord OAuth redirect");

app.Run();