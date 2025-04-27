using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Text.Json;
using TTX.Interfaces.Discord;

namespace TTX.Infrastructure.Discord;

public class DiscordAuthService(string clientId, string clientSecret) : IDiscordAuthService
{
    public async Task<DiscordUser?> GetByOAuth(string code)
    {
        using HttpClient httpClient = new();
        HttpResponseMessage tokenResponse = await httpClient.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", ""),
        ]));
        tokenResponse.EnsureSuccessStatusCode();

        string tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        TokenResponse token = JsonSerializer.Deserialize<TokenResponse>(tokenContent);
        DiscordConnection[] connections = await GetConnections(token);

        return new DiscordUser { Token = token.AccessToken, Connections = connections };
    }

    private static async Task<DiscordConnection[]> GetConnections(TokenResponse token)
    {
        using HttpClient httpClient = new();
        
        HttpRequestMessage userRequest = new(HttpMethod.Get, "https://discord.com/api/v10/users/@me/connections");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        HttpResponseMessage userResponse = await httpClient.SendAsync(userRequest);
        userResponse.EnsureSuccessStatusCode();

        string userContent = await userResponse.Content.ReadAsStringAsync();
        return [..JsonSerializer.Deserialize<ConnectionResponse[]>(userContent)!.Select(c => new DiscordConnection
        {
            Id = c.Id,
            Type = c.Type,
            Verified = c.Verified
        })];
    }
}