using System;
using System.Text;

namespace Core
{
    public static class JwtUtils
    {
        public static bool IsExpired(string jwt)
        {
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length != 3) return true;
                string payload = parts[1].PadRight(parts[1].Length + (4 - parts[1].Length % 4) % 4, '=')
                    .Replace('-', '+').Replace('_', '/');
                var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
                var key = "\"exp\":";
                int i = json.IndexOf(key, StringComparison.Ordinal);
                if (i < 0) return true;
                var sub = json.Substring(i + key.Length);
                var end = sub.IndexOfAny(new[] { ',', '}' });
                if (end < 0) return true;
                if (!long.TryParse(sub.Substring(0, end).Trim(), out var exp)) return true;
                return DateTimeOffset.UtcNow >= DateTimeOffset.FromUnixTimeSeconds(exp);
            }
            catch { return true; }
        }
    }
}