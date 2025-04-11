using TTX.Core.Interfaces;
using TTX.Core.Models;
using TwitchLib.Client;

namespace TTX.Infrastructure.Twitch.Services;

public class TwitchBotService : IBotService
{
    public event EventHandler<Message> OnMessage = delegate { };
    private readonly Dictionary<string, Creator> channels = [];
    private readonly TwitchClient client = new();

    public TwitchBotService()
    {
        client.Initialize(new("justinfan2425", ""));
        client.OnMessageReceived += (_, e) =>
        {
            var content = e.ChatMessage.Message;
            OnMessage(this, new Message
            {
                Creator = channels[e.ChatMessage.Channel],
                Content = content,
            });
        };
        client.OnConnected += (_, e) =>
        {
            foreach (var channel in channels)
            {
                client.JoinChannel(channel.Key);
            }
        };
    }

    public Task Start(CancellationToken token)
    {
        client.Connect();
        return Task.Run(() =>
        {
            while (!token.IsCancellationRequested) {}
        }, token);
    }

    public void AddCreator(Creator creator) => channels[creator.Slug.ToLower()] = creator;
    public void RemoveCreator(Creator creator) => channels.Remove(creator.Slug.ToLower());
}