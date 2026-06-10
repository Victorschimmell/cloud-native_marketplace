using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Attributes;

public class NotEmptyGuidAttribute : ValidationAttribute
{
    private const string DefaultErrorMessage = "{0} cannot be an empty GUID.";

    public NotEmptyGuidAttribute() : base(DefaultErrorMessage)
    {
    }

    public NotEmptyGuidAttribute(string errorMessage) : base(errorMessage)
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is Guid guid)
        {
            return guid != Guid.Empty;
        }
        return true;
    }

    public override string FormatErrorMessage(string name)
    {
        return string.Format(ErrorMessageString ?? DefaultErrorMessage, name);
    }
}