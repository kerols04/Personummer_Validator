using System;
using System.Globalization;
using System.Linq;

namespace CI_CD_Group_8
{
    /*
     * ===========================
     * PROGRAM – GRUPP 8
     * ===========================
     * Detta är konsolapplikationens startpunkt.
     *
     * Viktigt designval:
     * - Program-klassen hanterar ENDAST in- och utmatning (UI).
     * - All faktisk logik för personnummerkontroll ligger i PersonnummerValidator.
     *
     * Detta följer principen:
     * "Separation of Concerns" (ansvarsuppdelning)
     */
    internal static class Program
    {
        static void Main()
        {
            // Säkerställer att svenska tecken (å ä ö) visas korrekt i terminalen
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // Informationsutskrift till användaren
            Console.WriteLine("Personnummerkontroll – Grupp 8");
            Console.WriteLine("Tillåtna format:");
            Console.WriteLine("YYMMDD-XXXX, YYMMDDXXXX, YYYYMMDD-XXXX, YYYYMMDDXXXX");
            Console.WriteLine("Även + som separator stöds.");
            Console.WriteLine();

            /*
             * Programmet körs i en loop tills användaren själv väljer att avsluta.
             * Detta gör programmet användarvänligt och testbart.
             */
            while (true)
            {
                Console.Write("Ange personnummer (eller 'q' för att avsluta): ");
                string? input = Console.ReadLine()?.Trim();

                // Om användaren skriver 'q' avslutas programmet direkt
                if (string.Equals(input, "q", StringComparison.OrdinalIgnoreCase))
                    return;

                // Tom inmatning är alltid fel
                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.WriteLine("Fel: Tom inmatning.\n");
                    continue;
                }

                /*
                 * Validering sker via PersonnummerValidator.
                 * Program-klassen bryr sig inte om HUR valideringen sker,
                 * bara OM den är giltig eller inte.
                 */
                ValidationResult result = PersonnummerValidator.Validate(input);

                if (result.IsValid)
                {
                    // All information kommer färdigpaketerad i ValidationResult
                    Console.WriteLine("✔ Personnumret är giltigt");
                    Console.WriteLine($"Normaliserat format: {result.Normalized}");
                    Console.WriteLine($"Födelsedatum: {result.BirthDate:yyyy-MM-dd}");
                    Console.WriteLine($"Kön (heuristik): {result.GenderHint}");
                }
                else
                {
                    // Felmeddelandet kommer från valideringslogiken
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
     * Denna klass innehåller ALL affärslogik.
     * Den kan testas fristående via enhetstester (xUnit).
     */
    public static class PersonnummerValidator
    {
        /// <summary>
        /// Huvudmetod för validering av svenska personnummer.
        /// Returnerar ett ValidationResult istället för bool
        /// för att kunna ge detaljerad feedback.
        /// </summary>
        public static ValidationResult Validate(string input)
        {
            // Ta bort onödiga mellanslag
            string trimmed = input.Trim();

            // Identifiera eventuell separator (+ eller -)
            char? separator =
                trimmed.Contains('+') ? '+' :
                trimmed.Contains('-') ? '-' :
                (char?)null;

            /*
             * Ta bort alla tecken som inte är siffror.
             * Detta gör att vi kan hantera flera format på samma sätt.
             */
            string digits = new string(trimmed.Where(char.IsDigit).ToArray());

            // Ett svenskt personnummer ska ha 10 eller 12 siffror
            if (digits.Length != 10 && digits.Length != 12)
            {
                return ValidationResult.Invalid(
                    "Fel format: Personnumret måste innehålla 10 eller 12 siffror.");
            }

            /*
             * De sista 10 siffrorna används alltid i Luhn-kontrollen,
             * oavsett om personnumret skrivs med 10 eller 12 siffror.
             */
            string last10 = digits.Length == 12
                ? digits.Substring(2, 10)
                : digits;

            DateTime birthDate;

            /*
             * Datumvalidering:
             * - 12 siffror → YYYYMMDD
             * - 10 siffror → YYMMDD (kräver sekeltolkning)
             */
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

                // Avgör rätt sekel baserat på separator och dagens datum
                birthDate = ResolveCentury(parsed, separator);
            }

            // Kontrollsiffra enligt Luhn-algoritmen
            if (!IsValidLuhnForPersonnummer(last10))
            {
                return ValidationResult.Invalid(
                    "Ogiltig kontrollsiffra: Luhn-algoritmen misslyckades.");
            }

            // Skapa ett enhetligt, normaliserat format
            string normalized = FormatNormalized(birthDate, last10, separator);

            // Allt är korrekt → returnera giltigt resultat
            return ValidationResult.Valid(
                normalized,
                birthDate,
                GenderHintFromSerial(last10));
        }

        /// <summary>
        /// Försöker tolka ett datum exakt enligt angivet format.
        /// Används för att undvika kulturberoende fel.
        /// </summary>
        private static bool TryParseDateExact(string value, string format, out DateTime date)
        {
            return DateTime.TryParseExact(
                value,
                format,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        /// <summary>
        /// Avgör rätt sekel (1800/1900/2000) för YYMMDD.
        /// '+' betyder att personen är över 100 år.
        /// '-' betyder under 100 år.
        /// </summary>
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

            // Om ingen separator används: välj rimligaste datum i dåtid
            return candidates.Where(d => d <= today).Max();
        }

        /// <summary>
        /// Luhn-algoritm (mod 10) anpassad för personnummer.
        /// </summary>
        private static bool IsValidLuhnForPersonnummer(string last10)
        {
            int sum = 0;

            for (int i = 0; i < 9; i++)
            {
                int digit = last10[i] - '0';
                int factor = (i % 2 == 0) ? 2 : 1;
                int product = digit * factor;

                // Summera siffrorna i produkten
                sum += (product > 9) ? product - 9 : product;
            }

            int controlDigit = last10[9] - '0';
            return (10 - (sum % 10)) % 10 == controlDigit;
        }

        /// <summary>
        /// Skapar ett standardiserat format: YYYYMMDD-XXXX
        /// </summary>
        private static string FormatNormalized(DateTime birthDate, string last10, char? separator)
        {
            char sep = separator ?? '-';
            return $"{birthDate:yyyyMMdd}{sep}{last10.Substring(6, 4)}";
        }

        /// <summary>
        /// Enkel könsindikering:
        /// - Jämn siffra → Kvinna
        /// - Udda siffra → Man
        /// (Observera: detta är en förenkling)
        /// </summary>
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
     * Används för att returnera både status och detaljerad information.
     * Detta är bättre än att returnera true/false.
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
