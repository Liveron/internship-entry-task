using L.TicTacToe.Domain.Exceptions;
using L.TicTacToe.Infrastructure.Exceptions;

namespace L.TicTacToe.API.Extensions;

internal static class EndpointExtensions
{
    public static RouteGroupBuilder AddExceptionFilter(this RouteGroupBuilder builder)
	{
		return builder.AddEndpointFilter<ExceptionFilter>();
	}
}

internal sealed class ExceptionFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
		try
		{
			return await next(context);
		}
		catch (DomainException ex)
		{
			return TypedResults.BadRequest(ex.Message);
		}
		catch (ConcurrencyException ex)
		{
			context.HttpContext.SetResponseETagVersionHeader(ex.ActualVersion);
			return TypedResults.Ok();
		}
    }
}
