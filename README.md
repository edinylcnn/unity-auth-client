# Unity Auth Client

Unity Auth Client is a lightweight authentication client for Unity projects.  
It handles **user registration, login, JWT-based session management, auto-login**, and also supports **Unity Player Accounts (UPA)** integration.  
This project is designed to connect seamlessly with a [backend API](https://github.com/edinylcnn/dotnet-auth-api) for secure user authentication.

---

## 🚀 Features

- 🔑 User registration (username, email, password)  
- 🔐 User login (username/email + password)  
- 🔄 JWT session management (stored in PlayerPrefs)  
- ⏳ Token expiration check (JwtUtils)  
- 📡 API communication with backend endpoints (`/auth/signup`, `/auth/login`, `/auth/check-username`, `/auth/check-email`, `/users/me`, `/auth/upa`)  
- 🎮 Unity Player Accounts (UPA) login support  
- 🔄 Automatic login if a valid token exists  

---

## 📁 Scripts Structure

```bash
Scripts/
│── Core/
│   ├── UserSession.cs     # Handles user info & JWT storage
│   ├── JwtUtils.cs        # Checks if JWT is expired
│   ├── AppConfig.cs       # Stores backend API URL
│
│── Auth/
│   ├── Dtos.cs            # API DTO definitions
│   ├── AuthApiClient.cs   # Handles API calls (signup, login, etc.)
│   ├── AutoLogin.cs       # Auto login using saved token
│   ├── AuthUI.cs          # Login & SignUp UI logic
│
```

---

## ⚙️ Setup

1. Clone or download the repository:
   ```bash
   git clone https://github.com/edinylcnn/unity-auth-client.git
   ```

2. Copy it into your Unity project under `Assets/`.  

3. Configure **AppConfig**:  
   - In Unity, create a config asset via `Create → App → Config`.  
   - Set the backend API URL (e.g., `http://localhost:5000`).  

4. Assign the `AppConfig` asset to the `AuthApiClient` component in the Inspector.  

---

## 🖥️ Usage

### Sign Up
```csharp
StartCoroutine(api.SignUp("username", "mail@mail.com", "password123", (ok, msg) =>
{
    Debug.Log(ok ? "Sign up successful" : $"Sign up failed: {msg}");
}));
```

### Login
```csharp
StartCoroutine(api.Login("usernameOrEmail", "password123", (ok, msg, auth) =>
{
    if (ok)
    {
        session.Set(auth); // Save user session
        Debug.Log("Login successful: " + auth.username);
    }
    else Debug.LogError("Login error: " + msg);
}));
```

### Get Current User
```csharp
StartCoroutine(api.GetMe(session.Token, (ok, me, err) =>
{
    if (ok) Debug.Log($"User: {me.username} / {me.email}");
    else Debug.LogError("GetMe error: " + err);
}));
```

### Auto Login
- `AutoLogin.cs` automatically signs in if a valid token is stored and not expired.  

### Unity Player Accounts (UPA) Login
```csharp
// Call OnClickLoginUpa from AuthUI.cs
// If successful, the UGS token is exchanged with your backend and session is stored.
```

---

## 📜 License

This project is licensed under the MIT License.  
