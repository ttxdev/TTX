using MediatR;

namespace TTX.Commands
{
    public interface ICommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
    }
}