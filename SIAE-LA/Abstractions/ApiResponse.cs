namespace SIAE_LA.Abstractions
{
    public class ApiResponse<T>
    {
        public bool Ok { get; init; }
        public string? Message { get; init; }
        public T? Data { get; init; }


        public static ApiResponse<T> Success(T data, string? message = null)
        => new() { Ok = true, Data = data, Message = message };


        public static ApiResponse<T> Fail(string message)
        => new() { Ok = false, Message = message };
    }
}
