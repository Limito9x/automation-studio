using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Automation.SharedKernel.Extensions.Strings;

public static class StringExtensions
{
    public static string ToSlug(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Convert to lowercase
        text = text.ToLowerInvariant();

        // Remove diacritics (accents)
        text = RemoveDiacritics(text);

        // Replace spaces with hyphens
        text = Regex.Replace(text, @"\s+", "-");

        // Remove invalid characters
        text = Regex.Replace(text, @"[^a-z0-9\-]", "");

        // Remove consecutive hyphens
        text = Regex.Replace(text, @"-+", "-");

        // Trim hyphens from ends
        text = text.Trim('-');

        return text;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        // Special handling for Vietnamese character 'đ'
        var result = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        return result.Replace('đ', 'd').Replace('Đ', 'd');
    }
}

