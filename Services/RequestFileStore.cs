using Microsoft.AspNetCore.Components.Forms;

namespace EcobayApp.Services;

/// <summary>
/// Скриншоты заявок хранятся в App_Data/uploads (не в wwwroot), отдаются только авторизованным
/// пользователям через /admin/files/requests/{id}.
/// </summary>
public sealed class RequestFileStore
{
    public const long MaxBytes = 10 * 1024 * 1024;

    private static readonly Dictionary<string, string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/webp"] = ".webp"
    };

    private readonly string _root;

    public RequestFileStore(IWebHostEnvironment env, IConfiguration config)
    {
        var configured = config["Uploads:Path"];
        _root = string.IsNullOrWhiteSpace(configured)
            ? Path.Combine(env.ContentRootPath, "App_Data", "uploads")
            : configured;
    }

    public static string? Validate(IBrowserFile file)
    {
        if (!Allowed.ContainsKey(file.ContentType)) return "Поддерживаются только изображения PNG, JPG и WEBP.";
        if (file.Size > MaxBytes) return "Файл больше 10 МБ.";
        return null;
    }

    /// <summary>Сохраняет файл и возвращает относительный путь для БД.</summary>
    public async Task<string> SaveAsync(IBrowserFile file, CancellationToken ct = default)
    {
        var ext = Allowed[file.ContentType];
        var rel = Path.Combine("requests", DateTime.Now.ToString("yyyyMM"), Guid.NewGuid().ToString("N") + ext);
        var full = Path.Combine(_root, rel);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        await using var input = file.OpenReadStream(MaxBytes, ct);
        await using var output = File.Create(full);
        await input.CopyToAsync(output, ct);
        return rel.Replace('\\', '/');
    }

    public string? Resolve(string? relative)
    {
        if (string.IsNullOrWhiteSpace(relative)) return null;
        var full = Path.GetFullPath(Path.Combine(_root, relative));
        // Защита от выхода за пределы каталога (../)
        if (!full.StartsWith(Path.GetFullPath(_root), StringComparison.Ordinal)) return null;
        return File.Exists(full) ? full : null;
    }

    public static string ContentType(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "image/jpeg"
    };
}
