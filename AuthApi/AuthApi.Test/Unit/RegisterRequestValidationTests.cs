using System.ComponentModel.DataAnnotations;
using AuthApi.Dtos;

namespace AuthApi.Test.Unit;

public class RegisterRequestValidationTests
{
    private static List<ValidationResult> Validate(RegisterRequest request)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(request, new ValidationContext(request), results, validateAllProperties: true);
        return results;
    }

    private static RegisterRequest Valid() => new()
    {
        Email = "budi@example.com",
        Password = "Passw0rd123",
        ConfirmPassword = "Passw0rd123",
    };

    [Fact]
    public void Valid_request_has_no_errors()
    {
        Assert.Empty(Validate(Valid()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Invalid_email_is_rejected(string email)
    {
        var request = Valid();
        request.Email = email;

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequest.Email)));
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("onlyletters")]
    [InlineData("12345678")]
    public void Weak_password_is_rejected(string password)
    {
        var request = Valid();
        request.Password = password;
        request.ConfirmPassword = password;

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequest.Password)));
    }

    [Fact]
    public void Mismatched_confirm_password_is_rejected()
    {
        var request = Valid();
        request.ConfirmPassword = "Passw0rd12";

        var errors = Validate(request);

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(RegisterRequest.ConfirmPassword)));
    }
}
