using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Core
{
    public static class JwtUtils
    {
        private static readonly TimeSpan DefaultClockSkew = TimeSpan.FromSeconds(60);

        /// <summary>
        /// Checks if the JWT is expired with a given clock skew tolerance.
        /// </summary>
        public static bool IsExpired(string jwt, TimeSpan? clockSkew = null)
        {
            var skew = clockSkew ?? DefaultClockSkew;

            try
            {
                var parts = jwt?.Split('.');
                if (parts == null || parts.Length < 2)
                    return true;
                
                var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
                
                var exp = ExtractJsonLong(payloadJson, "exp");
                if (exp == null) return true;

                var expTime = DateTimeOffset.FromUnixTimeSeconds(exp.Value);
                
                return DateTimeOffset.UtcNow >= expTime - skew;
            }
            catch
            {
                // Any error - treat as expired for safety
                return true;
            }
        }

        private static byte[] Base64UrlDecode(string input)
        {
            string s = input.Replace('-', '+').Replace('_', '/');
            int pad = (4 - s.Length % 4) % 4;
            if (pad > 0) s += new string('=', pad);
            return Convert.FromBase64String(s);
        }

        private static long? ExtractJsonLong(string json, string key)
        {
            // "exp": 1700000000 gibi numeric-date bekleniyor
            var m = Regex.Match(json, $"\"{Regex.Escape(key)}\"\\s*:\\s*(\\d+)");
            if (!m.Success) return null;
            return long.TryParse(m.Groups[1].Value, out var val) ? val : null;
        }
    }
}