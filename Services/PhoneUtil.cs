namespace EcobayApp.Services;

public static class PhoneUtil
{
    /// <summary>Приводит белорусский номер к виду +375XXXXXXXXX (если возможно).</summary>
    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return "";
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("375") && digits.Length == 12) return "+" + digits;
        if (digits.StartsWith("80") && digits.Length == 11) return "+375" + digits[2..];
        if (digits.Length == 9) return "+375" + digits;
        return phone.Trim().StartsWith('+') ? "+" + digits : digits;
    }

    public static bool IsValid(string? phone)
    {
        var digits = new string((phone ?? "").Where(char.IsDigit).ToArray());
        return digits.Length is >= 9 and <= 15;
    }

    public static string Last9(string phone)
    {
        var digits = new string(phone.Where(char.IsDigit).ToArray());
        return digits.Length <= 9 ? digits : digits[^9..];
    }
}
