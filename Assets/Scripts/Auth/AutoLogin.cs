using System.Collections;
using Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Auth
{
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