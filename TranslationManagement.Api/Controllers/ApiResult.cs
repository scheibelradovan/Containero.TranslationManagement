namespace TranslationManagement.Api.Controllers
{
    public class ApiResult
    {
        internal static ApiResult Successfull = new ApiResult() { Error = null, Success = true };

        public bool Success { get; set; }

        public string Error { get; set; }

        internal static ApiResult CreateError(string error)
        {
            return new ApiResult()
            {
                Error = error,
                Success = false
            };
        }
    }

    public class ApiResult<T> : ApiResult
    {
        public T Result { get; set; }

        internal ApiResult(T result)
        {
            Success = true;
            Result = result;
        }
    }
}
