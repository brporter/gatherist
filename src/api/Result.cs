namespace api;

public readonly struct Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;

    private Result(TValue value)
    {
        IsError = false;
        _value = value;
        _error = default;
    }

    private Result(TError error)
    {
        IsError = true;
        _value = default;
        _error = error;
    }
    
    public bool IsError { get; }

    public bool IsSuccess => !IsError;

    public static implicit operator Result<TValue, TError>(TValue value) => new (value);
    public static implicit operator Result<TValue, TError>(TError error) => new (error);
    public static implicit operator TValue(Result<TValue, TError> result) => result._value;
    public static implicit operator TError(Result<TValue, TError> result) => result._error;

    public TResult Match<TResult>(
        Func<TValue, TResult> success,
        Func<TError, TResult> failure
    ) => !IsError ? success(_value!) : failure(_error!);
}

public readonly ref struct Outcome<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;

    private Outcome(TValue value)
    {
        IsError = false;
        _value = value;
        _error = default;
    }

    private Outcome(TError error)
    {
        IsError = true;
        _value = default;
        _error = error;
    }
    
    public bool IsError { get; }

    public bool IsSuccess => !IsError;
    
    public static implicit operator Outcome<TValue, TError>(TValue value) => new (value);
    public static implicit operator Outcome<TValue, TError>(TError error) => new (error);
    public static implicit operator TValue(Outcome<TValue, TError> result) => result._value;
    public static implicit operator TError(Outcome<TValue, TError> result) => result._error;

    public TResult Match<TResult>(
        Func<TValue, TResult> success,
        Func<TError, TResult> failure
    ) => !IsError ? success(_value!) : failure(_error!);
}
