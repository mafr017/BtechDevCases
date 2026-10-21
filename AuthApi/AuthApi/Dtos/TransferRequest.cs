using System.ComponentModel.DataAnnotations;

namespace AuthApi.Dtos;

public class TransferRequest : IValidatableObject
{
    [Required, EmailAddress, MaxLength(254)]
    public string Recipient { get; set; } = "";

    [Range(typeof(decimal), "0.01", "1000000000", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "Amount must be between 0.01 and 1,000,000,000.")]
    public decimal Amount { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (decimal.Round(Amount, 2) != Amount)
            yield return new ValidationResult("Amount can have at most 2 decimal places.", [nameof(Amount)]);
    }
}
