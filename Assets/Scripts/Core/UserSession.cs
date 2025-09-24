using System;
using UnityEngine;
using static Auth.Dtos;

namespace Core
{
    /// <summary>
    /// Manages the authenticated user’s session data.
    /// Stores JWT, username, and email both in memory and PlayerPrefs.
    /// </summary>
    public class UserSession : MonoBehaviour
    {
        public static UserSession Instance { get; private set; }
        public string Token { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }


        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Save authentication data into the session and PlayerPrefs.
        /// </summary>
        public void Set(AuthResponse auth)
        {
            Token = auth.token;
            Username = auth.username;
            Email = auth.email; 
            
            PlayerPrefs.SetString("jwt", auth.token);
            PlayerPrefs.SetString("username", auth.username);
            PlayerPrefs.SetString("email", auth.email);
            PlayerPrefs.Save();
        }

        private void Update()
        {
            // Debug helper: press 'C' to clear session manually
            if (Input.GetKeyDown(KeyCode.C))
            {
                Clear();
            }
        }

        /// <summary>
        /// Clear and remove saved PlayerPrefs keys.
        /// </summary>
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