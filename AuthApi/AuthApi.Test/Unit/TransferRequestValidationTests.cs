using System.ComponentModel.DataAnnotations;
using AuthApi.Dtos;

namespace AuthApi.Test.Unit;

public class TransferRequestValidationTests
{
    private static List<ValidationResult> Validate(TransferRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }

    private static TransferRequest Valid() => new()
    {
        Recipient = "siti@example.com",
        Amount = 150.25m,
        Notes = "Lunch",
    };

    [Fact]
    public void Valid_request_has_no_errors()
    {
        Assert.Empty(Validate(Valid()));
    }

    [Fact]
    public void Notes_are_optional()
    {
        var request = Valid();
        request.Notes = null;

        Assert.Empty(Validate(request));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Invalid_recipient_is_rejected(string recipient)
    {
        var request = Valid();
        request.Recipient = recipient;

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(TransferRequest.Recipient)));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("1.234")]
    [InlineData("1000000000.01")]
    public void Invalid_amount_is_rejected(string amount)
    {
        var request = Valid();
        request.Amount = decimal.Parse(amount, System.Globalization.CultureInfo.InvariantCulture);

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(TransferRequest.Amount)));
    }

    [Fact]
    public void Notes_longer_than_200_characters_are_rejected()
    {
        var request = Valid();
        request.Notes = new string('x', 201);

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(TransferRequest.Notes)));
    }
}
