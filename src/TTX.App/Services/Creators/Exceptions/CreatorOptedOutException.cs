using TTX.Domain.Exceptions;

namespace TTX.App.Services.Creators.Exceptions;

public sealed class CreatorOptedOutException() : InvalidActionException("Creator has opted out");
