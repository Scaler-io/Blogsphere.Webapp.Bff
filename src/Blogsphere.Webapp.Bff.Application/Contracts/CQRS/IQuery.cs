using MediatR;

namespace Blogsphere.Webapp.Bff.Application.Contracts.CQRS
{
    public interface IQuery<out TResponse> : IRequest<TResponse>
        where TResponse : notnull
    {

    }
}
