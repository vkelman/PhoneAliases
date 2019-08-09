using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PhoneAliasesService.Models;

namespace PhoneAliasesService.Services
{
    public interface IPhoneAliasesProcessor
    {
        bool TryValidatePhone(string rawPhone, out string phoneNumber);
        bool GetPhoneAliases(string rawPhone, int page, out PhoneAliasesResult phoneAliasesResult);
    }

    public class PhoneAliasesProcessor : IPhoneAliasesProcessor
    {
        private const string TenDigitPhonePattern = @"^\(?\d{3}\)?-? *\d{3}-? *-?\d{4}$";   // See https://stackoverflow.com/questions/18091324/regex-to-match-all-us-phone-number-formats
        private const string SevenDigitPhonePattern = @"^\d{3}-? *-?\d{4}$";

        private static readonly Dictionary<char, string[]> NumberAliases;

        private readonly int _pageSize;


        private List<string> GetPartialPhoneAliasesList(string partialPhoneNumber)
        {
            if (string.IsNullOrEmpty(partialPhoneNumber)) throw new Exception("cannot be");    // just on the safe side

            var result = new List<string>();

            int numberLength = partialPhoneNumber.Length;
            char firstDigit = partialPhoneNumber[0];

            if (numberLength > 1)
            {
                var previousResult = GetPartialPhoneAliasesList(partialPhoneNumber.Substring(1));
                foreach (string alias in NumberAliases[firstDigit])
                {
                    result.AddRange(previousResult.Select(_ => alias + _));
                }
            }
            else
            {
                result.AddRange(NumberAliases[firstDigit]);
            }

            return result;
        }

        static PhoneAliasesProcessor()
        {
            // See https://en.wikipedia.org/wiki/Telephone_keypad
            NumberAliases = new Dictionary<char, string[]>
            {
                ['0'] = new[] {"0"},
                ['1'] = new[] {"1"},
                ['2'] = new[] {"2", "a", "b", "c"},
                ['3'] = new[] {"3", "d", "e", "f"},
                ['4'] = new[] {"4", "g", "h", "i"},
                ['5'] = new[] {"5", "j", "k", "l"},
                ['6'] = new[] {"6", "m", "n", "o"},
                ['7'] = new[] {"7", "p", "q", "r", "s"},
                ['8'] = new[] {"8", "t", "u", "v"},
                ['9'] = new[] {"9", "w", "x", "y", "z"}
            };
        }

        public PhoneAliasesProcessor(int pageSize)
        {
            if (pageSize > 0)
                _pageSize = pageSize;
            else
                throw new ArgumentException("pageSize must be positive");
        }

        public bool TryValidatePhone(string rawPhone, out string phoneNumber)
        {
            phoneNumber = "";
            rawPhone = (rawPhone + "").Trim();

            if (rawPhone == string.Empty) return false;

            var tenRegex = new Regex(TenDigitPhonePattern);
            var sevenRegex = new Regex(SevenDigitPhonePattern);

            if (!tenRegex.IsMatch(rawPhone) && !sevenRegex.IsMatch(rawPhone)) return false;

            phoneNumber = new string(rawPhone.Where(char.IsDigit).ToArray());     // See https://stackoverflow.com/questions/3977497/stripping-out-non-numeric-characters-in-string
            return true;

        }

        public bool GetPhoneAliases(string rawPhone, int page, out PhoneAliasesResult phoneAliasesResult)
        {
            phoneAliasesResult = new PhoneAliasesResult()
            {
                TotalAliasesNumber = 0,
                CurrentPageNumber = 0,
                Aliases = new List<string>()
            };

            var allPhoneAliases = new List<string>();

            // ToDo: implement caching to avoid recalculating results on each call.

            if (!TryValidatePhone(rawPhone, out var phoneNumber)) return false;

            allPhoneAliases = GetPartialPhoneAliasesList(phoneNumber);

            int totalNumberOfAliases = allPhoneAliases.Count;
            int numberOfAliasesOnLastPage = totalNumberOfAliases % _pageSize;
            bool allPagesAreFull = (numberOfAliasesOnLastPage) == 0;
            if (numberOfAliasesOnLastPage == 0) numberOfAliasesOnLastPage = _pageSize;
            int numberOfPages = totalNumberOfAliases / _pageSize + (allPagesAreFull ? 0 : 1);

            int actualPage = Math.Min(page, numberOfPages);

            if (numberOfPages < 2)
            {
                phoneAliasesResult = new PhoneAliasesResult()
                {
                    DigitsOnlyPhone = phoneNumber,
                    TotalAliasesNumber = totalNumberOfAliases,
                    CurrentPageNumber = actualPage,
                    Aliases = allPhoneAliases
                };
            }
            else
            {
                int index = (actualPage - 1) * _pageSize;
                int count = (actualPage < numberOfPages) ? _pageSize : numberOfAliasesOnLastPage;
                var pageAliases = allPhoneAliases.GetRange(index, count);
                phoneAliasesResult = new PhoneAliasesResult()
                {
                    DigitsOnlyPhone = phoneNumber,
                    TotalAliasesNumber = totalNumberOfAliases,
                    NumberOfPages = numberOfPages,
                    CurrentPageNumber = actualPage,
                    Aliases = pageAliases
                };
            }

            return true;

        }


  }
}