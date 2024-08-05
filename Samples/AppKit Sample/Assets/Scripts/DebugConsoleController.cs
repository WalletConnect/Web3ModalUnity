using UnityEngine;

namespace WalletConnect.Web3Modal.Sample
{
    public class DebugConsoleController : MonoBehaviour
    {
        [SerializeField] private GameObject _debugConsole;
        
        private void Awake()
        {
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
            _debugConsole.SetActive(true);
#endif
        }
    }
}