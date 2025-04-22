using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Text.Json;
using TTX.Interfaces.Discord;

namespace TTX.Infrastructure.Discord;

public class DiscordAuthService(string clientId, string clientSecret) : IDiscordAuthService
{
    public async Task<DiscordUser?> GetByOAuth(string code)
    {
        using var httpClient = new HttpClient();
        var tokenResponse = await httpClient.PostAsync("https://discord.com/api/oauth2/token", new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "authorization_code"),
            new KeyValuePair<string, string>("code", code),
            new KeyValuePair<string, string>("redirect_uri", ""),
        ]));
        tokenResponse.EnsureSuccessStatusCode();

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var token = JsonSerializer.Deserialize<DiscordTokenResponse>(tokenContent);
        var connections = await getConnections(token);

        return new DiscordUser(token, connections);
    }

    private async Task<ImmutableArray<DiscordConnection>> getConnections(DiscordTokenResponse token)
    {
        using var httpClient = new HttpClient();

        var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/users/@me/connections");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

        var userResponse = await httpClient.SendAsync(userRequest);
        userResponse.EnsureSuccessStatusCode();

        var userContent = await userResponse.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ImmutableArray<DiscordConnection>>(userContent);
    }
}