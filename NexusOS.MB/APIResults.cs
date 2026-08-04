namespace NexusOS.MB
{
    public class APIResults<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Result { get; set; }

        public static APIResults<T> Success(T result, string message)
        {
            return new APIResults<T> { IsSuccess = true, Result = result, Message = message };
        }

        public static APIResults<T> Failure(string message)
        {
            return new APIResults<T> { IsSuccess = false, Message = message };
        }
    }
}
