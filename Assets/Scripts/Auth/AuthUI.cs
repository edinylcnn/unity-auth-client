using System.Collections;
using Auth;
using Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthUI : MonoBehaviour
{
    public AuthApiClient api;
    public UserSession session;

    [Header("Login")]
    public TMP_InputField inputUserOrEmail;
    public TMP_InputField inputPassword;
    public TMP_Text textLoginInfo;
    public GameObject panelLogin;

    [Header("SignUp")]
    public GameObject panelSignUp;
    public TMP_InputField inputUsername;
    public TMP_Text textUsernameStatus;
    public TMP_InputField inputEmail;
    public TMP_Text textEmailStatus;
    public TMP_InputField inputPass1;
    public TMP_InputField inputPass2;
    public TMP_Text textSignUpInfo;

    [Header("Scenes")]
    public string mainSceneName = "Main";

    public void OnClickOpenSignUp() { panelLogin.SetActive(false); panelSignUp.SetActive(true); ClearSignUpTexts(); }
    public void OnClickBackToLogin() { panelSignUp.SetActive(false); panelLogin.SetActive(true); textLoginInfo.text = ""; }

    public void OnEndEditUsername()
    {
        var u = inputUsername.text.Trim(); if (string.IsNullOrEmpty(u)) { textUsernameStatus.text=""; return; }
        StartCoroutine(api.CheckUsername(u, (ok,msg)=> { textUsernameStatus.text = ok ? msg : "Network error"; }));
    }

    public void OnEndEditEmail()
    {
        var e = inputEmail.text.Trim(); if (string.IsNullOrEmpty(e)) { textEmailStatus.text=""; return; }
        StartCoroutine(api.CheckEmail(e, (ok,msg)=> { textEmailStatus.text = ok ? msg : "Network error"; }));
    }

    public void OnClickSignUp()
    {
        var u = inputUsername.text.Trim();
        var e = inputEmail.text.Trim();
        var p1 = inputPass1.text; var p2 = inputPass2.text;

        if (p1 != p2) { textSignUpInfo.text = "Passwords do not match."; return; }
        if (u.Length < 3) { textSignUpInfo.text = "Username must be at least 3 chars."; return; }
        if (!e.Contains("@")) { textSignUpInfo.text = "Please enter a valid e-mail."; return; }

        StartCoroutine(api.SignUp(u, e, p1, (ok,msg)=>
        {
            if (ok) { OnClickBackToLogin(); textLoginInfo.text = "Sign up successful. Please sign in."; }
            else textSignUpInfo.text = msg;
        }));
    }

    public void OnClickSignIn()
    {
        var u = inputUserOrEmail.text.Trim();
        var p = inputPassword.text;

        StartCoroutine(api.Login(u, p, (ok,msg,auth)=>
        {
            if (!ok) { textLoginInfo.text = msg; return; }
            session.Set(auth);
            SceneManager.LoadScene(mainSceneName);
        }));
    }

    void ClearSignUpTexts(){ textUsernameStatus.text=""; textEmailStatus.text=""; textSignUpInfo.text=""; }
}
