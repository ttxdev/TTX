using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TTX.Api.Controllers.Dto;
using TTX.Api.Converters;
using TTX.App.Dto.Players;
using TTX.App.Interfaces.Platforms;
using TTX.App.Services.Players;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;
using SessionOptions = TTX.Api.Options.SessionOptions;

namespace TTX.Api.Controllers;

[ApiController]
[Route("sessions")]
public class SessionsController(IServiceProvider _services, IOptions<SessionOptions> _options, PlayerService _playerService) : ControllerBase
{
    const string CALLBACK_STATE_KEY = "state";

    [HttpGet("login")]
    [EndpointName("GetLoginUrl")]
    public async Task<ActionResult<string>> Login()
    {
        IPlatformUserService platform = _services.GetRequiredKeyedService<IPlatformUserService>(Platform.Twitch);
        string stateKey = Guid.NewGuid().ToString();
        Uri loginUrl = new(platform.GetLoginUrl() + $"&state={stateKey}");
        HttpContext.Session.SetString(CALLBACK_STATE_KEY, stateKey);

        return Ok(loginUrl.ToString());
    }

    [HttpGet("twitch/callback")]
    [EndpointName("TwitchCallback")]
    public async Task<ActionResult<TokenDto>> TwitchCallback([FromQuery] string code, [FromQuery] string state)
    {
        // TODO
        // string? stateKey = HttpContext.Session.GetString(CALLBACK_STATE_KEY);
        // if (stateKey != state)
        // {
        //     return BadRequest("Invalid state key");
        // }

        HttpContext.Session.Remove(CALLBACK_STATE_KEY);
        await HttpContext.Session.CommitAsync();

        Result<PlayerPartialDto> result = await _playerService.Authenticate(Platform.Twitch, code);
        if (!result.IsSuccessful)
        {
            return result.ToBadActionResult();
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_options.Value.Key));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);
        string token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
            _options.Value.Validation.ValidIssuer,
            _options.Value.Validation.ValidAudience,
            result.Value!.ToClaims(),
            expires: DateTime.Now.Add(_options.Value.Expires),
            signingCredentials: creds
        ));

        return new TokenDto(token);
    }

    // [HttpGet("discord/callback")]
    // [EndpointName("DiscordCallback")]
    // public async Task<ActionResult<DiscordTokenDto>> DiscordCallback([FromQuery] string code)
    // {
    //     var result = await sender.Send(new AuthenticateDiscordUserCommand
    //     {
    //         OAuthCode = code
    //     });

    //     return Ok(new DiscordTokenDto(result, sessions.CreateDiscordSession(result)));
    // }

    // [HttpPost("discord/link")]
    // [EndpointName("LinkDiscordTwitch")]
    // public async Task<ActionResult<TokenDto>> LinkDiscordTwitch([FromBody] LinkDiscordTwitchDto req)
    // {
    //     var tUser = sessions.ParseDiscordSession(req.Token).FirstOrDefault(t => t.Id == req.TwitchId);
    //     if (tUser is null)
    //         return NotFound();

    //     return Ok(await sender.Send(new AuthenticateTwitchUserCommand
    //     {
    //         UserId = tUser.Id
    //     }).ContinueWith(t => new TokenDto(sessions.CreateSession(t.Result))));
    // }
}
