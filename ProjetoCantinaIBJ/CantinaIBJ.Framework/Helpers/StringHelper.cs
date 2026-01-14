using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace CantinaIBJ.Framework.Helpers
{
    public static class StringHelper
    {

        public static string OnlyNumbers(this string stIn)
        {
            return string.IsNullOrEmpty(stIn) ? string.Empty : Regex.Replace(stIn, "[^0-9]", string.Empty);
        }

        public static string RemoveDiacritics(this string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        public static string RemoveSpecialChars(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            var r = new Regex("(?:[^a-z0-9 ]|(?<=['\"])s)",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            return r.Replace(value, String.Empty);
        }

        public static string RemoveAccents(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var textNormalizado = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in textNormalizado)
            {
                var categoriaUnicode = CharUnicodeInfo.GetUnicodeCategory(c);

                if (categoriaUnicode != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string NormalizeString(this string value)
        {
            String normalizedString = value.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (char c in normalizedString)
            {
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        stringBuilder.Append(c);
                        break;
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                        stringBuilder.Append('-');
                        break;
                }
            }
            string result = stringBuilder.ToString();
            return String.Join("-", result.Split(new char[] { '-' }
                , StringSplitOptions.RemoveEmptyEntries));
        }

        public static string NullSafeToLower(this string s)
        {
            if (s == null)
            {
                s = string.Empty;
            }
            return s.ToLower();
        }

        public static string ToJson(this object s) => JsonConvert.SerializeObject(s, Formatting.Indented);

        public static string FormatCPF(this string sender)
        {
            string response = sender.Trim();
            if (response.Length == 11)
            {
                response = response.Insert(9, "-");
                response = response.Insert(6, ".");
                response = response.Insert(3, ".");
            }
            return response;
        }

        public static string FormatCNPJ(this string sender)
        {
            string response = sender.Trim();
            if (response.Length == 14)
            {
                response = response.Insert(12, "-");
                response = response.Insert(8, "/");
                response = response.Insert(5, ".");
                response = response.Insert(2, ".");
            }
            return response;
        }
    }
}