using Common;
using System.Net;

public class AppException : Exception
{
    public int ErrorCode { get; }
    public HttpStatusCode StatusCode { get; }

    public AppException(int errorCode, string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }

    public AppException(FoundationCode.ErrorInfo error)
        : this(error.Code, error.Message, error.StatusCode)
    {
    }
    public AppException(FoundationCode.ErrorInfo error, string message)
    : this(error.Code, message, error.StatusCode)
    {
    }

}
