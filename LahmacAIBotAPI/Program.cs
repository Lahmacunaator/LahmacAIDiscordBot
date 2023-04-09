using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using DiscordBot;
using DiscordBot.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddCors(options =>
{
    options.AddPolicy("LahmacAIBotCorsPolicy",policy =>
    {
        policy.WithOrigins("https://localhost:7179").AllowAnyMethod().AllowAnyHeader().AllowCredentials();
    });
});
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
    opt.CallbackPath = new PathString("/auth/oauthCallback");

    opt.ClientId = builder.Configuration.GetValue<string>("Discord:ClientId");
    opt.ClientSecret = builder.Configuration.GetValue<string>("Discord:ClientSecret");

    opt.TokenEndpoint = "https://discord.com/api/oauth2/token";
    opt.UserInformationEndpoint = "https://discord.com/api/users/@me";
    
    opt.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
    opt.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");

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

app.UseExceptionHandler("/Error");
// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();
app.MapControllers();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.UseCors("LahmacAIBotCorsPolicy");

var botService = app.Services.GetService<IBotService>();

var bot = botService.CreateBot();
await botService.GetBot()!.RunAsync().ConfigureAwait(false);

app.UseAuthentication();

//app.MapGet("/", () => $"{bot.Client.CurrentUser.Username} is up and running");

app.MapGet("/discordoauthfailed", () => $"{bot.Client.CurrentUser.Username} Disocrd OAuth failed L");
app.MapGet("/auth/oauthCallback", () => $"{bot.Client.CurrentUser.Username} Discord OAuth redirect");

app.Run();