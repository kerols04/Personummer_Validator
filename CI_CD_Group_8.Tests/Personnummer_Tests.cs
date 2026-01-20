using Xunit;
using PersonnummerApp;

namespace PersonnummerApp.Tests;

public class PersonnummerValidatorTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData("123", false)]                 // för kort
    [InlineData("12345678901", false)]         // 11 siffror -> ogiltigt
    [InlineData("1234567890123", false)]       // för långt
    public void IsValid_ShouldReturnFalse_ForInvalidLengthOrEmpty(string input, bool expected)
    {
        Assert.Equal(expected, PersonnummerValidator.IsValid(input));
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_ForInvalidChecksum()
    {
        // Fel kontrollsiffra (sista siffran ändrad)
        Assert.False(PersonnummerValidator.IsValid("199301055815"));
    }

    [Theory]
    [InlineData("19930105-5814")]
    [InlineData("19930105+5814")]
    [InlineData("199301055814")]
    public void IsValid_ShouldHandleSeparators_AndReturnSameAsDigitsOnly(string input)
    {
        // Eftersom din kod tar ut bara siffror, ska dessa format bete sig likadant
        var digitsOnly = "199301055814";
        Assert.Equal(
            PersonnummerValidator.IsValid(digitsOnly),
            PersonnummerValidator.IsValid(input)
        );
    }

    [Theory]
    [InlineData("199301055814", "9301055814")] // 12 siffror -> tar bort två första
    public void IsValid_ShouldTreat12DigitsAs10Digits(string twelveDigits, string tenDigits)
    {
        Assert.Equal(
            PersonnummerValidator.IsValid(tenDigits),
            PersonnummerValidator.IsValid(twelveDigits)
        );
    }
}
