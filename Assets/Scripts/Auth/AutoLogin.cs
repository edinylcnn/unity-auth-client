using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Auth
{
    /// <summary>
    /// Handles automatic login on application start.
    /// Checks if a JWT token exists in PlayerPrefs and is still valid.
    /// If valid, it retrieves the user data from the backend
    /// and transitions directly into the main scene.
    /// </summary>
    public class AutoLogin : MonoBehaviour
    {
        public AuthApiClient api;
        public UserSession session;
        public string mainSceneName = "Main";

        IEnumerator Start()
        {
            var token = PlayerPrefs.GetString("jwt", null);
            if (string.IsNullOrEmpty(token)) yield break;

            if (JwtUtils.IsExpired(token)) yield break;

            yield return api.GetMe(token, (ok, me, err) =>
            {
                if (!ok) return;
                session.Set(new Dtos.AuthResponse { token = token, username = me.username, email = me.email });
                SceneManager.LoadScene(mainSceneName);
            });
        }
    }
}