using System;
using System.Threading.Tasks;
using Core;
using DG.Tweening;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Auth
{
    /// <summary>
    /// Handles the authentication UI logic (Login, Sign Up, UPA login).
    /// Connects the UI input fields and buttons with AuthApiClient API calls
    /// and manages session updates + scene transitions.
    /// </summary>
    public class AuthUI : MonoBehaviour
    {
        public AuthApiClient api;
        public UserSession session;

        [Header("Login")] public TMP_InputField inputUserOrEmail;
        public TMP_InputField inputPassword;
        public TMP_Text textLoginInfo;
        public GameObject panelLogin;

        [Header("SignUp")] public GameObject panelSignUp;
        public TMP_InputField inputUsername;
        public TMP_Text textUsernameStatus;
        public TMP_InputField inputEmail;
        public TMP_Text textEmailStatus;
        public TMP_InputField inputPass1;
        public TMP_InputField inputPass2;
        public TMP_Text textSignUpInfo;

        [Header("Scenes")] public string mainSceneName = "Main";

        private async void Awake()
        {
            try
            {
                await UnityServices.InitializeAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"UnityServices init: {e.Message}");
            }

            PlayerAccountService.Instance.SignedIn += OnUpaSignedIn;
            PlayerAccountService.Instance.SignInFailed += ex => Debug.LogError($"UPA SignInFailed: {ex}");
        }

        public void OnClickOpenSignUp()
        {
            panelLogin.SetActive(false);
            panelSignUp.SetActive(true);
            ClearSignUpTexts();
        }

        public void OnClickBackToLogin()
        {
            panelSignUp.SetActive(false);
            panelLogin.SetActive(true);
            textLoginInfo.text = "";
        }

        public void OnEndEditUsername()
        {
            var u = inputUsername.text.Trim();
            if (string.IsNullOrEmpty(u))
            {
                ResetInputStatus(inputUsername, textUsernameStatus);
                return;
            }

            if (u.Length < 3)
            {
                UpdateInputStatus(inputUsername.transform, textUsernameStatus, "Username must be at least 3 chars.", false);
                return;
            }

            StartCoroutine(api.CheckUsername(u, (ok, exists, msg) =>
            {
                textUsernameStatus.text = ok ? "" : "Network error";
                UpdateInputStatus(inputUsername.transform, textUsernameStatus, msg, exists);
            }));
        }

        public void OnEndEditEmail()
        {
            var e = inputEmail.text.Trim();
            if (string.IsNullOrEmpty(e))
            {
                ResetInputStatus(inputEmail, textEmailStatus);
                return;
            }

            if (!e.Contains("@"))
            {
                UpdateInputStatus(inputEmail.transform, textEmailStatus, "Please enter a valid e-mail.", false);
                return;
            }

            StartCoroutine(api.CheckEmail(e, (ok, exists, msg) =>
            {
                textEmailStatus.text = ok ? "" : "Network error";
                UpdateInputStatus(inputEmail.transform, textEmailStatus, msg, exists);
            }));
        }

        public void OnEndEditLoginEmailorUsername()
        {
            var e = inputUserOrEmail.text.Trim();
            if (e.Length > 0)
            {
                ResetInputStatus(inputUserOrEmail, textLoginInfo, false);
            }
        }
        public void OnEndEditLoginPassword()
        {
            var e = inputPassword.text.Trim();
            if (e.Length > 0)
            {
                ResetInputStatus(inputPassword, textLoginInfo, false);
            }
        }

        public void OnEndEditPassword()
        {
            var p1 = inputPass1.text;
            var p2 = inputPass2.text;
            if (p1.Length == 0 || p2.Length == 0) return;
            if (p1.Length == 0 && p2.Length == 0)
            {
                ResetInputStatus(inputPass1, textSignUpInfo);
                ResetInputStatus(inputPass2, textSignUpInfo);
            }

            bool b = p1 == p2;
            UpdateInputStatus(inputPass1.transform, textSignUpInfo, "Passwords do not match.", b);
            UpdateInputStatus(inputPass2.transform, textSignUpInfo, "Passwords do not match.", b);
        }

        public void OnClickSignUp()
        {
            var u = inputUsername.text.Trim();
            var e = inputEmail.text.Trim();
            var p1 = inputPass1.text;
            var p2 = inputPass2.text;

            if (p1 != p2)
            {
                return;
            }

            if (u.Length < 3)
            {
                UpdateInputStatus(inputUsername.transform, textUsernameStatus, "Username must be at least 3 chars.", false);
                return;
            }

            if (!e.Contains("@"))
            {
                UpdateInputStatus(inputEmail.transform, textEmailStatus, "Please enter a valid e-mail.", false);
                return;
            }

            StartCoroutine(api.SignUp(u, e, p1, (ok, msg) =>
            {
                if (ok)
                {
                    OnClickBackToLogin();
                    textLoginInfo.color = Color.green;
                    textLoginInfo.text = "Sign up successful. Please sign in.";
                }
                else textSignUpInfo.text = msg;
            }));
        }

        public void OnClickSignIn()
        {
            var u = inputUserOrEmail.text.Trim();
            var p = inputPassword.text;
            if (string.IsNullOrWhiteSpace(u))
            {
                UpdateInputStatus(inputUserOrEmail.transform, textLoginInfo, "Please enter an e-mail or username", false);
                return;
            }

            if (string.IsNullOrWhiteSpace(p))
            {
                UpdateInputStatus(inputPassword.transform, textLoginInfo, "Please enter a password", false);
                return;
            }

            StartCoroutine(api.Login(u, p, (ok, msg, auth) =>
            {
                if (!ok)
                {
                    textLoginInfo.text = msg;
                    return;
                }

                session.Set(auth);
                SceneManager.LoadScene(mainSceneName);
            }));
        }

        public async void OnClickLoginUpa()
        {
            try
            {
                if (PlayerAccountService.Instance.IsSignedIn)
                {
                    await SignInWithUnityAuthAndExchange();
                    return;
                }

                await PlayerAccountService.Instance.StartSignInAsync();
            }
            catch (Exception e)
            {
                Debug.LogError($"UPA start flow error: {e}");
            }
        }

        private async void OnUpaSignedIn() => await SignInWithUnityAuthAndExchange();

        private async Task SignInWithUnityAuthAndExchange()
        {
            try
            {
                //Sign in with Unity Services using UPA access token
                var upaAccessToken = PlayerAccountService.Instance.AccessToken;
                if (string.IsNullOrEmpty(upaAccessToken))
                {
                    await PlayerAccountService.Instance.RefreshTokenAsync();
                    upaAccessToken = PlayerAccountService.Instance.AccessToken;
                }

                if (string.IsNullOrEmpty(upaAccessToken))
                {
                    Debug.LogError("UPA access token missing — cannot sign in with Unity.");
                    return;
                }

                await AuthenticationService.Instance.SignInWithUnityAsync(upaAccessToken);
                Debug.Log("UPA Authentication successful.");

                //Exchange UPA Access Token with your backend API
                var serverToken = AuthenticationService.Instance.AccessToken;
                if (string.IsNullOrEmpty(serverToken))
                {
                    Debug.LogError("UPA access token missing.");
                    return;
                }
                
                StartCoroutine(api.UpaLogin(serverToken, (ok, msg, auth) =>
                {
                    if (!ok)
                    {
                        textLoginInfo.text = msg;
                        return;
                    }

                    session.Set(auth);
                    SceneManager.LoadScene(mainSceneName);
                }));
            }
            catch (AuthenticationException e)
            {
                Debug.LogError($"UPA Authentication error: {e.Message}");
            }
            catch (Exception e)
            {
                Debug.LogError($"General error: {e}");
            }
        }

        public void UpdateInputStatus(Transform inputField, TMP_Text status, string msg, bool isValid)
        {
            Image image = inputField.GetComponent<Image>();

            if (!isValid)
            {
                inputField.DOShakePosition(0.5f, 15);
                image?.DOColor(Color.red, 0.5f);
                status.SetText(msg);
            }
            else
            {
                image?.DOColor(Color.green, 0.5f);
                status.SetText("");
            }
        }

        public void ResetInputStatus(TMP_InputField inputField, TMP_Text status, bool resetInput = true)
        {
            Image image = inputField.GetComponent<Image>();
            image?.DOColor(Color.white, 0.5f);

            inputField.text = resetInput ? "" : inputField.text;
            status.SetText("");
        }

        void ClearSignUpTexts()
        {
            ResetInputStatus(inputEmail, textEmailStatus);
            ResetInputStatus(inputUsername, textUsernameStatus);
            ResetInputStatus(inputPass1, textSignUpInfo);
            ResetInputStatus(inputPass2, textSignUpInfo);
        }
    }
}