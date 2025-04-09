using System.Net.Http.Headers;
using System.Text.Json;
using TTX.Core.Interfaces;
using TTX.Core.Models;

namespace TTX.Infrastructure.Discord.Services;

public class DiscordAuthService(ITwitchAuthService twitch, string clientId, string clientSecret) : IDiscordAuthService
{
    public async Task<DiscordUser?> GetByOAuth(string code)
    {
        using var httpClient = new HttpClient();

        var tokenResponse = await httpClient.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", ""),
        }));
        tokenResponse.EnsureSuccessStatusCode();
        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<DiscordTokenResponse>(tokenContent);
        if (token is null)
            return null;
        var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/users/@me/connections");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
        var userResponse = await httpClient.SendAsync(userRequest);
        userResponse.EnsureSuccessStatusCode();
        var userContent = await userResponse.Content.ReadAsStringAsync();
        var connections = JsonSerializer.Deserialize<List<DiscordConnection>>(userContent);
        var twitchConnections = connections?.Where(c => c.type == "twitch" && c.verified).ToArray();
        var twitchUsers = new List<TwitchUser>();
        if (twitchConnections is null)
            return null;
        foreach (var connection in twitchConnections)
        {
            var twitchUser = await twitch.FindById(connection.id);
            if (twitchUser is null)
                continue;
            twitchUsers.Add(twitchUser);
        }
        return new DiscordUser
        {
            access_token = token.access_token,
            TwitchUsers = twitchUsers
        };
    }

    public async Task<TwitchUser?> GetByOAuthToTwitch(string access_token, string twitchId)
    {
        using var httpClient = new HttpClient();

        var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/users/@me/connections");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
        var userResponse = await httpClient.SendAsync(userRequest);
        userResponse.EnsureSuccessStatusCode();
        var userContent = await userResponse.Content.ReadAsStringAsync();
        var connections = JsonSerializer.Deserialize<List<DiscordConnection>>(userContent);
        var connection = connections?.FirstOrDefault(c => c.type == "twitch" && c.id == twitchId);
        if (connection is null)
            return null;

        var user = await twitch.FindById(connection.id);

        return user;
    }
}