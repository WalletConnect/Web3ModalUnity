using TMPro;
using UnityEngine;

namespace WalletConnect.Web3Modal.Sample
{
    public class Notification : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _messageText;

        public static Notification Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<Notification>(true);

                return _instance;
            }
        }

        public GameObject Root
        {
            get => _root;
        }

        private static Notification _instance;

        public static void ShowMessage(string message)
        {
            Instance.Show(message);
        }

        public void Show(string message)
        {
            Debug.Log(message, this);

            _messageText.text = message;
            _root.SetActive(true);
        }

        public static void Hide()
        {
            Instance.Root.SetActive(false);
        }

        public void OnButtonHide()
        {
            Hide();
        }
    }
}