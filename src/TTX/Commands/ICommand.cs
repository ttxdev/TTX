using MediatR;

namespace TTX.Commands
{
    internal interface ICommand<TResponse> : IRequest<TResponse>;
}