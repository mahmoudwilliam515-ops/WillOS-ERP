namespace EnterpriseERP.SharedKernel.Results;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();

    public static ApiResponse<T> Ok(T data, string message = "Operation completed successfully")
        => new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(IEnumerable<string> errors, string message = "Operation failed")
        => new() { Success = false, Errors = errors, Message = message };

    public static ApiResponse<T> Fail(string error) => Fail(new[] { error });
}

public class PagedApiResponse<T> : ApiResponse<T>
{
    public int Total { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }

    public static PagedApiResponse<T> OkPaged(T data, int total, int pageNumber, int pageSize, string message = "Operation completed successfully")
        => new() { Success = true, Data = data, Total = total, PageNumber = pageNumber, PageSize = pageSize, Message = message };
}
