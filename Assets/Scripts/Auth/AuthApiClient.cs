using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.Networking;
using static Auth.Dtos;

namespace Auth
{
    public class AuthApiClient : MonoBehaviour
    {
        [Header("Config")] public AppConfig config; // Inspector’dan AppConfig.asset bağla

        [Serializable]
        class SignUpRequest
        {
            public string Username;
            public string Email;
            public string Password;
        }

        [Serializable]
        class LoginRequest
        {
            public string UsernameOrEmail;
            public string Password;
        }

        [Serializable]
        class UnityLoginRequest
        {
            public string IdToken;
        }

        string BaseUrl => config != null ? config.baseUrl : "http://localhost:5000";

        static string ExtractMessage(string json)
        {
            try
            {
                return JsonUtility.FromJson<ErrorMsg>(json).message;
            }
            catch
            {
                return json;
            }
        }

        public IEnumerator CheckUsername(string username, Action<bool, bool, string> cb)
        {
            using var req =
                UnityWebRequest.Get($"{BaseUrl}/auth/check-username?username={UnityWebRequest.EscapeURL(username)}");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                cb(false, false, req.error);
                yield break;
            }

            var res = JsonUtility.FromJson<ExistsResponse>(req.downloadHandler.text);
            cb(true, !res.exists, res.message);
        }

        public IEnumerator CheckEmail(string email, Action<bool, bool, string> cb)
        {
            using var req = UnityWebRequest.Get($"{BaseUrl}/auth/check-email?email={UnityWebRequest.EscapeURL(email)}");
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                cb(false, false, req.error);
                yield break;
            }

            var res = JsonUtility.FromJson<ExistsResponse>(req.downloadHandler.text);
            cb(true, !res.exists, res.message);
        }

        public IEnumerator SignUp(string username, string email, string password, Action<bool, string> cb)
        {
            var body = JsonUtility.ToJson(new SignUpRequest
                { Username = username, Email = email, Password = password });
            using var req = new UnityWebRequest($"{BaseUrl}/auth/signup", "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                cb(false, req.error);
            }
            else if (req.responseCode == 201) cb(true, "ok");
            else cb(false, ExtractMessage(req.downloadHandler.text));
        }

        public IEnumerator Login(string userOrEmail, string password, Action<bool, string, AuthResponse> cb)
        {
            var body = JsonUtility.ToJson(new LoginRequest { UsernameOrEmail = userOrEmail, Password = password });
            using var req = new UnityWebRequest($"{BaseUrl}/auth/login", "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success) cb(false, req.error, null);
            else if (req.responseCode == 200)
                cb(true, "ok", JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text));
            else if (req.responseCode == 401) cb(false, "Invalid username/email or password.", null);
            else cb(false, ExtractMessage(req.downloadHandler.text), null);
        }

        public IEnumerator GetMe(string token, Action<bool, UserMe, string> cb)
        {
            using var req = UnityWebRequest.Get($"{BaseUrl}/users/me");
            req.SetRequestHeader("Authorization", $"Bearer {token}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                cb(false, null, req.error);
            }
            else if (req.responseCode == 200) cb(true, JsonUtility.FromJson<UserMe>(req.downloadHandler.text), null);
            else cb(false, null, $"HTTP {req.responseCode}");
        }

        public IEnumerator UpaLogin(string idToken, Action<bool, string, AuthResponse> cb)
        {
            var body = JsonUtility.ToJson(new UnityLoginRequest { IdToken = idToken });

            using var req = new UnityWebRequest($"{BaseUrl}/auth/upa", "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API /auth/upa ağ hatası: {req.error}");
                cb(false, req.error, null);
                yield break;
            }

            if (req.responseCode == 200)
            {
                var res = JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text);
                cb(true, "ok", res);
            }
            else if (req.responseCode == 401)
            {
                var msg = ExtractMessage(req.downloadHandler.text);
                Debug.LogWarning($"API /auth/upa yetkisiz: {msg}");
                cb(false, "Yetkisiz: UPA token doğrulanamadı.", null);
            }
            else
            {
                var msg = ExtractMessage(req.downloadHandler.text);
                Debug.LogError($"API /auth/upa HTTP {req.responseCode}: {msg}");
                cb(false, msg, null);
            }
        }
    }
}