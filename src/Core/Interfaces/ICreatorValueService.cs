using TTX.Core.Models;

namespace TTX.Core.Interfaces;

public interface ICreatorValueService
{
    Task Process(Creator creator, string message);
    Task Digest();
}