using mixpanel;
using Sentry;
using Skibitsky.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WalletConnect.Web3Modal.Sample
{
    public class AppInit : MonoBehaviour
    {
        [SerializeField] private SceneReference _mainScene;

        [Space]
        [SerializeField] private GameObject _debugConsole;

        private void Start()
        {
            InitDebugConsole();
            ConfigureMixpanel();
            SceneManager.LoadScene(_mainScene);

            Debug.LogError("Test error log");
        }

        private void InitDebugConsole()
        {
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
            DontDestroyOnLoad(gameObject);
            _debugConsole.SetActive(true);
#endif
        }

        private void ConfigureMixpanel()
        {
            Application.logMessageReceived += (logString, stackTrace, type) =>
            {
                var props = new Value
                {
                    ["type"] = type.ToString(),
                    ["scene"] = SceneManager.GetActiveScene().name
                };
                Mixpanel.Track(logString, props);
            };
        }
    }
}