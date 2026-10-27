using EcobayApp.Contex;
using EcobayApp.Models;
namespace EcobayApp.Services;

public static class ClientPhoneLookup
{
    // Compare whole normalized numbers, not an arbitrary common nine-digit suffix.
    // Existing formatted phones are supported without automatically merging old clients.
    public static IQueryable<Client> ByPhone(AppDBContext db, string normalized)
    {
        var digits = new string(normalized.Where(char.IsDigit).ToArray());
        var variants = digits.StartsWith("375") && digits.Length == 12
            ? new[] { digits, digits[3..], "80" + digits[3..] }
            : new[] { digits };
        return db.Clients.Where(c => variants.Contains(c.Phone
            .Replace("+", "").Replace(" ", "").Replace("-", "")
            .Replace("(", "").Replace(")", "").Replace(".", "").Replace(" ", "")));
    }
}
