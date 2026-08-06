namespace Timekeeper_Program
{
	public class Result<T, U>
    {
        public T? Value { get; private set; }
        public U? Error { get; private set; }
        public bool IsSuccess { get; private set; }

        private Result(T value)
        {
            Value = value;
            IsSuccess = true;
        }

        private Result(U error)
        {
            Error = error;
            IsSuccess = false;
        }

        public static Result<T, U> Success(T value) => new Result<T, U>(value);
        public static Result<T, U> Failure(U error) => new Result<T, U>(error);
    }
}