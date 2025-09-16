using System;
using UnityEngine;
using static Auth.Dtos;

namespace Core
{
    public class UserSession : MonoBehaviour
    {
        public static UserSession I { get; private set; }
        public string Token { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }

        void Awake()
        {
            if (I != null) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Set(AuthResponse auth)
        {
            Token = auth.token; Username = auth.username; Email = auth.email;
            PlayerPrefs.SetString("jwt", auth.token);
            PlayerPrefs.SetString("username", auth.username);
            PlayerPrefs.SetString("email", auth.email);
            PlayerPrefs.Save();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                Clear();
            }
        }

        public void Clear()
        {
            Token = Username = Email = null;
            PlayerPrefs.DeleteKey("jwt");
            PlayerPrefs.DeleteKey("username");
            PlayerPrefs.DeleteKey("email");
            PlayerPrefs.Save();
        }
    }
}