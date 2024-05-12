namespace WebSocketServer.Common;
public record Result
{
    protected Result()
    {
        IsSuccess = true;
        Failures = new Failure[0];
    }

    protected Result(IEnumerable<Failure> failures)
    {
        Failures = failures;
    }

    public static Result Success() => new Result();
    public static Result Failure(params Failure[] failure) => new Result(failure);
    public static Result Failure(IEnumerable<Failure> failure) => new Result(failure);


    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public IEnumerable<Failure> Failures { get; private set; }
}

public record Result<T> : Result
{
    protected Result(T value) : base()
    {
        Value = value;
    }

    protected Result(IEnumerable<Failure> failures) : base(failures)
    {
    }

    public static Result<T> Success(T value) => new Result<T>(value);
    public static new Result<T> Failure(params Failure[] Failures) => new Result<T>(Failures);
    public static new Result<T> Failure(IEnumerable<Failure> failure) => new Result<T>(failure);

    public T? Value { get; private set; }

    public T Expect()
    {
        if (IsFailure)
        {
            throw new InvalidOperationException("Result is in a failed state");
        }

        return Value!;
    }
}