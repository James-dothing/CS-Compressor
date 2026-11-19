namespace CsCompressor.Core.Validation;

public record ValidationError(string PropertyName, string Message, bool IsWarning = false);

public class ValidationResult
{
    private readonly List<ValidationError> _errors = new();
    private readonly List<ValidationError> _warnings = new();

    public bool IsValid => _errors.Count == 0;
    public IReadOnlyList<ValidationError> Errors => _errors;
    public IReadOnlyList<ValidationError> Warnings => _warnings;

    public void AddError(string propertyName, string message)
    {
        _errors.Add(new ValidationError(propertyName, message, IsWarning: false));
    }

    public void AddWarning(string propertyName, string message)
    {
        _warnings.Add(new ValidationError(propertyName, message, IsWarning: true));
    }

    public string GetCombinedErrorMessage()
    {
        if (IsValid) return string.Empty;
        return string.Join("\n", _errors.Select(e => $"• {e.Message}"));
    }
}
