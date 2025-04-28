using MediatR;

namespace TTX.Queries;

public interface IQuery<out TResponse> : IRequest<TResponse>;
