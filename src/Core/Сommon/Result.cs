namespace LibraryApp.Core.Common;

public abstract record Result<T>
{
    // public sealed record Success(T Data) : Result<T>;
    // public sealed record Error(string Message) : Result<T>;
    // public sealed record Loading : Result<T>;
    //
    // public T? Data => (this as Success)?.Data;
    // public sealed record Error(ErrorMessage Error) : Result<T>;
    // public bool IsLoading => this is Loading;
    // public bool IsSuccess => this is Success;
    // public bool IsError => this is Error;
}