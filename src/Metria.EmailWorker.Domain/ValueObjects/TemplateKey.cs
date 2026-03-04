using System.Text.RegularExpressions;
using Metria.EmailWorker.Domain.Exceptions;

namespace Metria.EmailWorker.Domain.ValueObjects;

public sealed record TemplateKey
{
    private static readonly Regex Pattern = new(
        "^[a-z0-9._-]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Value { get; }

    public TemplateKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainValidationException("Template key must be provided.");
        }

        var normalized = value.Trim();
        if (!Pattern.IsMatch(normalized))
        {
            throw new DomainValidationException("Template key format is invalid.");
        }

        Value = normalized;
    }

    public override string ToString() => Value;
}
