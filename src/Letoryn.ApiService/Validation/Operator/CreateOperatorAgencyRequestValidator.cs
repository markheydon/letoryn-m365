using System.Net.Mail;

namespace Letoryn.ApiService.Validation.Operator;

/// <summary>
/// Validates <see cref="CreateOperatorAgencyRequest"/> at the HTTP trust boundary (T026).
/// </summary>
public sealed class CreateOperatorAgencyRequestValidator : IRequestValidator<CreateOperatorAgencyRequest>
{
    /// <inheritdoc />
    public RequestValidationResult Validate(CreateOperatorAgencyRequest request)
    {
        var failures = new List<ValidationFailure>();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            failures.Add(new ValidationFailure(nameof(CreateOperatorAgencyRequest.DisplayName), "Display name is required."));
        }
        else if (request.DisplayName.Length > 200)
        {
            failures.Add(new ValidationFailure(nameof(CreateOperatorAgencyRequest.DisplayName), "Display name must be 200 characters or fewer."));
        }

        if (string.IsNullOrWhiteSpace(request.PrimaryContactEmail))
        {
            failures.Add(new ValidationFailure(nameof(CreateOperatorAgencyRequest.PrimaryContactEmail), "Primary contact email is required."));
        }
        else if (!IsValidEmail(request.PrimaryContactEmail))
        {
            failures.Add(new ValidationFailure(nameof(CreateOperatorAgencyRequest.PrimaryContactEmail), "Primary contact email is not a valid email address."));
        }

        if (!string.IsNullOrWhiteSpace(request.PrimaryContactPhone)
            && request.PrimaryContactPhone.Length > 32)
        {
            failures.Add(new ValidationFailure(nameof(CreateOperatorAgencyRequest.PrimaryContactPhone), "Primary contact phone must be 32 characters or fewer."));
        }

        return failures.Count == 0
            ? RequestValidationResult.Success()
            : RequestValidationResult.Failure(failures);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
