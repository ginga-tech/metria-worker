using System.Text.RegularExpressions;
using Metria.EmailWorker.Domain.Exceptions;

namespace Metria.EmailWorker.Domain.ValueObjects;

public sealed record EmailAddress
{
    private static readonly Regex Pattern = new(
        "^[^\\s@]+@[^\\s@]+\\.[^\\s@]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException("Email must be provided.");
        }

        var normalized = value.Trim();
        if (!Pattern.IsMatch(normalized))
        {
            throw new DomainValidationException("Email format is invalid.");
        }

        Value = normalized;
    }

    public override string ToString() => Value;
}
