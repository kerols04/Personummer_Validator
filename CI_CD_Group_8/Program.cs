using System;
using System.Globalization;
using System.Linq;

namespace CI_CD_Group_8
{
    /*
     * ===========================
     * PROGRAM – GRUPP 8
     * ===========================
     * Startpunkt för konsolapplikationen.
     *
     * Design:
     * - Program = UI (in/utmatning)
     * - PersonnummerValidator = all logik (testbar)
     */
    internal static class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Personnummerkontroll – Grupp 8");
            Console.WriteLine("Tillåtna format:");
            Console.WriteLine("YYMMDD-XXXX, YYMMDDXXXX, YYYYMMDD-XXXX, YYYYMMDDXXXX");
            Console.WriteLine("Även + som separator stöds.");
            Console.WriteLine();

            while (true)
            {
                Console.Write("Ange personnummer (eller 'q' för att avsluta): ");
                string? input = Console.ReadLine()?.Trim();

                if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                    return;

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Fel: Tom inmatning.\n");
                    continue;
                }

                ValidationResult result = PersonnummerValidator.Validate(input);

                if (result.IsValid)
                {
                    Console.WriteLine("✔ Personnumret är giltigt");
                    Console.WriteLine($"Normaliserat format: {result.Normalized}");
                    Console.WriteLine($"Födelsedatum: {result.BirthDate:yyyy-MM-dd}");
                    Console.WriteLine($"Kön (heuristik): {result.GenderHint}");
                }
                else
                {
                    Console.WriteLine("✖ Personnumret är ogiltigt");
                    Console.WriteLine(result.ErrorMessage);
                }

                Console.WriteLine();
            }
        }
    }

    /*
     * ===========================
     * PERSONNUMMERVALIDATOR
     * ===========================
     * All affärslogik här → lätt att testa.
     */
    public static class PersonnummerValidator
    {
        // FIX FÖR CI + TESTER:
        // Dina xUnit-tester anropar PersonnummerValidator.IsValid(...)
        // Men den metoden fanns inte i din kod → CI failar vid build/test.
        // Denna wrapper löser det direkt.
        public static bool IsValid(string input) => Validate(input).IsValid;

        /// <summary>
        /// Huvudmetod för validering av svenska personnummer.
        /// Returnerar detaljerad info via ValidationResult.
        /// </summary>
        public static ValidationResult Validate(string input)
        {
            string trimmed = input.Trim();

            char? separator =
                trimmed.Contains('+') ? '+' :
                trimmed.Contains('-') ? '-' :
                (char?)null;

            // Ta bort alla tecken som inte är siffror
            string digits = new string(trimmed.Where(char.IsDigit).ToArray());

            // Måste vara 10 eller 12 siffror
            if (digits.Length != 10 && digits.Length != 12)
            {
                return ValidationResult.Invalid(
                    "Fel format: Personnumret måste innehålla 10 eller 12 siffror.");
            }

            // Luhn används alltid på de sista 10 siffrorna
            string last10 = digits.Length == 12
                ? digits.Substring(2, 10)
                : digits;

            DateTime birthDate;

            // Datumvalidering
            if (digits.Length == 12)
            {
                if (!TryParseDateExact(digits.Substring(0, 8), "yyyyMMdd", out birthDate))
                {
                    return ValidationResult.Invalid(
                        "Ogiltigt datum: YYYYMMDD är inte ett giltigt datum.");
                }
            }
            else
            {
                if (!TryParseDateExact(digits.Substring(0, 6), "yyMMdd", out DateTime parsed))
                {
                    return ValidationResult.Invalid(
                        "Ogiltigt datum: YYMMDD är inte ett giltigt datum.");
                }

                birthDate = ResolveCentury(parsed, separator);
            }

            // Luhn-check
            if (!IsValidLuhnForPersonnummer(last10))
            {
                return ValidationResult.Invalid(
                    "Ogiltig kontrollsiffra: Luhn-algoritmen misslyckades.");
            }

            // Normalisering
            string normalized = FormatNormalized(birthDate, last10, separator);

            return ValidationResult.Valid(
                normalized,
                birthDate,
                GenderHintFromSerial(last10));
        }

        private static bool TryParseDateExact(string value, string format, out DateTime date)
        {
            return DateTime.TryParseExact(
                value,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        private static DateTime ResolveCentury(DateTime yyDate, char? separator)
        {
            DateTime today = DateTime.Today;
            int yy = yyDate.Year % 100;

            DateTime[] candidates =
            {
                new DateTime(2000 + yy, yyDate.Month, yyDate.Day),
                new DateTime(1900 + yy, yyDate.Month, yyDate.Day),
                new DateTime(1800 + yy, yyDate.Month, yyDate.Day)
            };

            if (separator == '+')
                return candidates.First(d => (today - d).TotalDays >= 365.25 * 100);

            if (separator == '-')
                return candidates.Where(d => d <= today).Max();

            return candidates.Where(d => d <= today).Max();
        }

        private static bool IsValidLuhnForPersonnummer(string last10)
        {
            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                int digit = last10[i] - '0';
                int factor = (i % 2 == 0) ? 2 : 1;
                int product = digit * factor;
                sum += (product > 9) ? product - 9 : product;
            }

            int controlDigit = last10[9] - '0';
            return (10 - (sum % 10)) % 10 == controlDigit;
        }

        private static string FormatNormalized(DateTime birthDate, string last10, char? separator)
        {
            char sep = separator ?? '-';
            return $"{birthDate:yyyyMMdd}{sep}{last10.Substring(6, 4)}";
        }

        private static string GenderHintFromSerial(string last10)
        {
            int digit = last10[8] - '0';
            return (digit % 2 == 0) ? "Kvinna" : "Man";
        }
    }

    /*
     * ===========================
     * VALIDATIONRESULT
     * ===========================
     */
    public sealed class ValidationResult
    {
        public bool IsValid { get; }
        public string Normalized { get; }
        public DateTime BirthDate { get; }
        public string GenderHint { get; }
        public string ErrorMessage { get; }

        private ValidationResult(bool valid, string normalized, DateTime date, string gender, string error)
        {
            IsValid = valid;
            Normalized = normalized;
            BirthDate = date;
            GenderHint = gender;
            ErrorMessage = error;
        }

        public static ValidationResult Valid(string normalized, DateTime date, string gender)
            => new ValidationResult(true, normalized, date, gender, "");

        public static ValidationResult Invalid(string message)
            => new ValidationResult(false, "", default, "", message);
    }
}

