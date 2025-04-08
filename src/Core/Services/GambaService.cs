using TTX.Core.Models;
using TTX.Core.Repositories;

namespace TTX.Core.Services;

public interface IGambaService
{
  public Task<LootBoxResult> Gamba(User user);
}

public class GambaService(ICreatorRepository creatorRepo, IUserRepository repository) : IGambaService
{
    public async Task<LootBoxResult> Gamba(User user)
    {
        var creators = await creatorRepo.GetAllAbove(100);
        var lootBox = user.Gamba(creators);

        repository.Update(user);
        await repository.SaveChanges();

        return lootBox;
    }

}