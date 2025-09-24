using System;
using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.Networking;
using static Auth.Dtos;

namespace Auth
{
    /// <summary>
    /// Handles all communication with the backend authentication API.
    /// Provides methods for checking username/email availability,
    /// signing up, logging in, retrieving user data, and Unity Player Accounts (UPA) login.
    /// </summary>
    public class AuthApiClient : MonoBehaviour
    {
        [Header("Config")] public AppConfig config; //Link an AppConfig asset in the Inspector to set the backend base URL.

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

        // Determine base API URL from config
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

        /// <summary>
        /// Checks if the given username is available.
        /// Calls /auth/check-username?username={username}
        /// </summary>
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

        /// <summary>
        /// Checks if the given email is available.
        /// Calls /auth/check-email?email={email}
        /// </summary>
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

        /// <summary>
        /// Creates a new user account.
        /// Calls POST /auth/signup with {username, email, password}
        /// </summary>
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
                cb(false, req.error);
            else if (req.responseCode == 201) 
                cb(true, "ok");
            else 
                cb(false, ExtractMessage(req.downloadHandler.text));
        }

        /// <summary>
        /// Logs in a user with username/email and password.
        /// Calls POST /auth/login with {UsernameOrEmail, Password}
        /// Returns AuthResponse (token, username, email) if successful.
        /// </summary>
        public IEnumerator Login(string userOrEmail, string password, Action<bool, string, AuthResponse> cb)
        {
            var body = JsonUtility.ToJson(new LoginRequest { UsernameOrEmail = userOrEmail, Password = password });
            using var req = new UnityWebRequest($"{BaseUrl}/auth/login", "POST");
            req.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success) 
                cb(false, req.error, null);
            else if (req.responseCode == 200)
                cb(true, "ok", JsonUtility.FromJson<AuthResponse>(req.downloadHandler.text));
            else if (req.responseCode == 401) 
                cb(false, "Invalid username/email or password.", null);
            else 
                cb(false, ExtractMessage(req.downloadHandler.text), null);
        }

        /// <summary>
        /// Retrieves the current authenticated user’s information.
        /// Calls GET /users/me with Authorization: Bearer {token}
        /// </summary>
        public IEnumerator GetMe(string token, Action<bool, UserMe, string> cb)
        {
            using var req = UnityWebRequest.Get($"{BaseUrl}/users/me");
            req.SetRequestHeader("Authorization", $"Bearer {token}");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                cb(false, null, req.error); //Network error
            }
            else if (req.responseCode == 200) 
                cb(true, JsonUtility.FromJson<UserMe>(req.downloadHandler.text), null);
            else 
                cb(false, null, $"HTTP {req.responseCode}"); //Other error
        }

        ///<summary>
        /// Logs in using Unity Player Accounts (UPA).
        /// Calls POST /auth/upa with {IdToken}.
        /// </summary>
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
                Debug.LogError($"API /auth/upa network error:: {req.error}");
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
                Debug.LogWarning($"API /auth/upa unauthorized: {msg}");
                cb(false, "Unauthorized: UPA token validation failed.", null);
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