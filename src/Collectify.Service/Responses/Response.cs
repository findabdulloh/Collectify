namespace Collectify.Service.Responses;

public class Response<TResult>
{
    public int Code { get; set; } = 200;
    public string Message { get; set; } = "Success";
    public TResult Result { get; set; }
}