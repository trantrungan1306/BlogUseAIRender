namespace SimpleBlog.Application.Common;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class ForbiddenException : Exception
{
    public ForbiddenException(string message = "You are not allowed to perform this action.") : base(message) { }
}

public class AppValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public AppValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public AppValidationException(IDictionary<string, string[]> errors) : base("Validation failed.")
    {
        Errors = errors;
    }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
