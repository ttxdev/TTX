using MediatR;

namespace TTX.Queries;

public interface IQuery<TResponse> : IRequest<TResponse>;
