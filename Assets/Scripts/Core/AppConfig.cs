using UnityEngine;

namespace Core
{
    [CreateAssetMenu(menuName="App/Config", fileName="AppConfig")]
    public class AppConfig : ScriptableObject
    {
        [Tooltip("Backend base URL, no trailing slash. Example: http://localhost:5000")]
        public string baseUrl = "http://localhost:5000";
    }
}
