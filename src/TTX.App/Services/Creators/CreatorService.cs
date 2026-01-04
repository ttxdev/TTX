using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using TTX.App.Dto.Creators;
using TTX.App.Dto.Pagination;
using TTX.App.Dto.Portfolio;
using TTX.App.Events;
using TTX.App.Events.Creators;
using TTX.App.Exceptions;
using TTX.App.Interfaces.Platforms;
using TTX.App.Repositories;
using TTX.App.Services.Creators.Exceptions;
using TTX.App.Services.Creators.Validation;
using TTX.Domain.Exceptions;
using TTX.Domain.Models;
using TTX.Domain.ValueObjects;

namespace TTX.App.Services.Creators;

public class CreatorService(
    ICreatorRepository _repository,
    IServiceProvider _services,
    IEventDispatcher _events
)
{
    public async Task<PaginationDto<CreatorDto>> Index(IndexCreatorsRequest request)
    {
        IndexResult<Creator> result = await _repository.Index(
            page: request.Page,
            limit: request.Limit,
            search: request.Search,
            minValue: request.MinValue,
            order: request.Order ?? new Order<CreatorOrderBy>()
            {
                By = CreatorOrderBy.Name,
                Dir = OrderDirection.Ascending
            },
            history: request.HistoryParams
        );

        return new PaginationDto<CreatorDto>
        {
            Total = result.Total,
            Data = [.. result.Data.Select(CreatorDto.Create)]
        };
    }

    public async Task<CreatorDto?> Find(Slug slug, HistoryParams historyParams)
    {
        Creator? creator = await _repository.GetCreatorDetails(slug, historyParams);
        if (creator is null)
        {
            return null;
        }

        return CreatorDto.Create(creator);
    }

    public async Task<bool> IsPlayer(Slug creatorSlug, ModelId playerId)
    {
        bool? isPlayer = await _repository.IsPlayer(creatorSlug, playerId);

        return isPlayer.HasValue && isPlayer.Value;
    }

    public async Task<Result<CreatorOptOutDto>> OptOut(Slug slug, string? reason = null)
    {
        Creator? creator = await _repository.FindBySlug(slug);
        if (creator is null)
        {
            return Result<CreatorOptOutDto>.Err(new NotFoundException<Creator>());
        }

        CreatorOptOut opt = CreatorOptOut.Create(creator, reason);
        _repository.AddOptOut(opt);
        _repository.Remove(creator);
        await _repository.SaveChanges();

        return Result<CreatorOptOutDto>.Ok(CreatorOptOutDto.Create(opt));
    }

    public async Task<Result<ModelId>> Onboard(OnboardRequest request)
    {
        OnboardValidator validator = new(_repository);
        ValidationResult result = await validator.ValidateAsync(request);
        if (result.IsValid)
        {
            return Result<ModelId>.Err(new InvalidRequestException(result.Errors));
        }

        PlatformUser? user = null;
        IPlatformUserService platformUserService = _services.GetRequiredKeyedService<IPlatformUserService>(request.Platform);
        if (request.Username is not null)
        {
            user = await platformUserService.GetUserByUsername(request.Username);
        }
        else if (request.PlatformId is not null)
        {
            user = await platformUserService.GetUserById(request.PlatformId);
        }

        if (user is null)
        {
            return Result<ModelId>.Err(new NotFoundException<PlatformUser>());
        }

        Creator? creator = await _repository.FindByPlatform(request.Platform, user.Id);
        if (creator is not null)
        {
            _repository.Update(creator);
            if (creator.Sync(user.DisplayName, user.Username, user.AvatarUrl))
            {
                await _repository.SaveChanges();
            }

            return Result<ModelId>.Ok(creator.Id);
        }

        if (await _repository.IsOptOut(request.Platform, user.Id))
        {
            return Result<ModelId>.Err(new CreatorOptedOutException());
        }

        creator = new()
        {
            PlatformId = user.Id,
            Platform = request.Platform,
            Name = user.DisplayName,
            Slug = user.Username,
            AvatarUrl = user.AvatarUrl,
            Ticker = request.Ticker
        };

        _repository.Add(creator);
        await _repository.SaveChanges();
        await _events.Dispatch(CreateCreatorEvent.Create(creator));

        return Result<ModelId>.Ok(creator.Id);
    }
}
