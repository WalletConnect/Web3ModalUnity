using System;
using System.Collections;
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

            Debug.Log("Environment Variables:");
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
            {
                Debug.Log($"{de.Key} = {de.Value}");
            }
            
            SceneManager.LoadScene(_mainScene);
        }

        private void InitDebugConsole()
        {
#if UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS
            DontDestroyOnLoad(gameObject);
            _debugConsole.SetActive(true);
#endif
        }
    }
}