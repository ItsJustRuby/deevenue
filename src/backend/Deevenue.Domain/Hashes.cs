using System.Text;

namespace Deevenue.Domain;

public static class Hashes
{
    public static async Task<string> ComputeMD5Async(Stream stream)
    {
        var md5 = System.Security.Cryptography.MD5.Create();

        return await md5.ComputeHashAsync(stream)
            .ContinueWith(hashBytes =>
            {
                var sb = new StringBuilder();
                foreach (var b in hashBytes.Result)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            });
    }
}
