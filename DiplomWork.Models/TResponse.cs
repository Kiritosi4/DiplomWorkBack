

namespace DiplomWork.Models
{
    public class TResponse<T>
    {
        public bool Error { get; set; }
        public T Response { get; set; }
        public string ErrorMessage { get; set; }

        public TResponse(bool error, T response, string errorMessage)
        {
            Error = error;
            Response = response;
            ErrorMessage = errorMessage;
        }

        public static TResponse<T> Success(T response)
        {
            return new TResponse<T>(false, response, string.Empty);
        }

        public static TResponse<T> Fail(string message)
        {
            return new TResponse<T>(true, default(T), message);
        }
    }
}
