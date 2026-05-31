namespace Carevionix.Helpers;

public static class FileUploadValidator
{
    public const long MaxProfileImageBytes = 2 * 1024 * 1024;

    private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private static readonly HashSet<string> AllowedImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public static async Task<bool> IsAllowedProfileImageAsync(IFormFile file)
    {
        if (file is null || file.Length <= 0 || file.Length > MaxProfileImageBytes)
        {
            return false;
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedImageExtensions.Contains(extension) || !AllowedImageContentTypes.Contains(file.ContentType))
        {
            return false;
        }

        await using var stream = file.OpenReadStream();
        var buffer = new byte[12];
        var read = await stream.ReadAsync(buffer);

        return IsJpeg(buffer, read) || IsPng(buffer, read) || IsWebp(buffer, read);
    }

    public static string ProfileImageError =>
        "Only valid JPG, PNG, or WEBP profile images up to 2 MB are allowed.";

    private static bool IsJpeg(byte[] buffer, int read) =>
        read >= 3 && buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;

    private static bool IsPng(byte[] buffer, int read) =>
        read >= 8 &&
        buffer[0] == 0x89 &&
        buffer[1] == 0x50 &&
        buffer[2] == 0x4E &&
        buffer[3] == 0x47 &&
        buffer[4] == 0x0D &&
        buffer[5] == 0x0A &&
        buffer[6] == 0x1A &&
        buffer[7] == 0x0A;

    private static bool IsWebp(byte[] buffer, int read) =>
        read >= 12 &&
        buffer[0] == 0x52 &&
        buffer[1] == 0x49 &&
        buffer[2] == 0x46 &&
        buffer[3] == 0x46 &&
        buffer[8] == 0x57 &&
        buffer[9] == 0x45 &&
        buffer[10] == 0x42 &&
        buffer[11] == 0x50;
}
