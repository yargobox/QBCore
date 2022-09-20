using System.Net;
using Example1.API.Models;
using QBCore.Controllers;

namespace Example1.API.Middlewares;

public class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;

	public ExceptionHandlingMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext httpContext)
	{
		try
		{
			await _next(httpContext);
		}
		catch (FormatException ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.BadRequest, ex);
		}
		catch (NotSupportedException ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.BadRequest, ex);
		}
		catch (InvalidOperationException ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.BadRequest, ex);
		}
		catch (ArgumentException ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.BadRequest, ex);
		}
		catch (NotFound404Exception ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.NotFound, ex);
		}
		catch (Unauthorized401Exception ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.Unauthorized, ex);
		}
		catch (Conflict409Exception ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.Conflict, ex);
		}
/* 		catch (Handled500Exception ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.InternalServerError, ex);
		}
		catch (ApplicationException ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.InternalServerError, ex);
		} */
		catch (Exception ex)
		{
			await HandleExceptionAsync(httpContext, HttpStatusCode.InternalServerError, ex);
		}
	}

	private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode httpStatusCode, Exception ex)
	{
		context.Response.ContentType = "application/json";
		context.Response.StatusCode = (int) httpStatusCode;

		var details = ex.GetBaseException()?.Message;
		if (ex.Message == details)
		{
			details = null;
		}

		var exceptionResponse = new HttpExceptionResponse
		{
			StatusCode = (int)httpStatusCode,
			Instance = context.Request.Path,
			Type = ex.GetType().ToString(),
			Message = ex.Message,
			Details = details,
			StackTrace = ex.StackTrace
		};

		return context.Response.WriteAsync(exceptionResponse.ToString());
	}
}