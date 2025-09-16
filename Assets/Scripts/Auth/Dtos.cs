namespace Auth
{
    public class Dtos
    {
        [System.Serializable] public class ExistsResponse { public bool exists; public string message; }
        [System.Serializable] public class AuthResponse   { public string token; public string username; public string email; }
        [System.Serializable] public class ErrorMsg       { public string message; }
        [System.Serializable] public class UserMe         { public string username; public string email; }
    }
}
