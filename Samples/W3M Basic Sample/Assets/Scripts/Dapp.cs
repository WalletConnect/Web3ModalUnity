using System;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.Signer.EIP712;
using Nethereum.Web3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WalletConnectUnity.Core;

namespace WalletConnect.Web3Modal.Sample
{
    public class Dapp : MonoBehaviour
    {
        private bool _resumed;

        [SerializeField] private TMP_Text _activeAddress;
        [SerializeField] private TMP_Text _activeChainId;
        [SerializeField] private TMP_Text _initializingLabel;

        [Space] [SerializeField] private Button _connectButton;
        [SerializeField] private Button _networkButton;
        [SerializeField] private Button _accountButton;
        [SerializeField] private Button _signTypedDataButton;
        [SerializeField] private Button _transactionButton;
        [SerializeField] private Button _personalSignButton;
        [SerializeField] private Button _getBalanceButton;
        [SerializeField] private Button _reverseResoleEnsButton;
        [SerializeField] private Button _disconnectButton;

        private void Awake()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            _connectButton.interactable = false;
        }

        private async void Start()
        {
            Debug.Log("Init Web3Modal...");
            try
            {
                await Web3Modal.InitializeAsync();

                _initializingLabel.gameObject.SetActive(false);

                Web3Modal.ChainChanged += (_, e) =>
                {
                    if (e.Chain == null)
                    {
                        _activeChainId.text = "Unsupported chain";
                        return;
                    }

                    _activeChainId.text = e.Chain.ChainId;
                };

                Web3Modal.AccountConnected += (_, e) =>
                {
                    _connectButton.interactable = false;
                    _accountButton.interactable = true;
                    _signTypedDataButton.interactable = true;
                    _transactionButton.interactable = true;
                    _personalSignButton.interactable = true;
                    _getBalanceButton.interactable = true;
                    _reverseResoleEnsButton.interactable = true;
                    _disconnectButton.interactable = true;

                    var account = e.GetAccount();
                    _activeAddress.text = account.Address;
                    _activeChainId.text = account.ChainId;
                };

                Web3Modal.AccountDisconnected += (_, _) =>
                {
                    _connectButton.interactable = true;
                    _accountButton.interactable = false;
                    _signTypedDataButton.interactable = false;
                    _transactionButton.interactable = false;
                    _personalSignButton.interactable = false;
                    _getBalanceButton.interactable = false;
                    _reverseResoleEnsButton.interactable = false;
                    _disconnectButton.interactable = false;

                    _activeAddress.text = string.Empty;
                    _activeChainId.text = string.Empty;
                };

                Web3Modal.AccountChanged += (_, e) =>
                {
                    _activeAddress.text = e.Account.Address;
                    _activeChainId.text = e.Account.ChainId;
                };

                _resumed = await Web3Modal.ConnectorController.TryResumeSessionAsync();

                _networkButton.interactable = true;
                _connectButton.interactable = !_resumed;
                _accountButton.interactable = _resumed;
                _signTypedDataButton.interactable = _resumed;
                _transactionButton.interactable = _resumed;
                _personalSignButton.interactable = _resumed;
                _getBalanceButton.interactable = _resumed;
                _reverseResoleEnsButton.interactable = _resumed;
                _disconnectButton.interactable = _resumed;
            }
            catch (Exception e)
            {
                Notification.ShowMessage(e.Message);
                throw;
            }
        }

        public void OnConnectButton()
        {
            Web3Modal.OpenModal();
        }

        public void OnNetworkButton()
        {
            Web3Modal.OpenModal(ViewType.NetworkSearch);
        }

        public void OnAccountButton()
        {
            Web3Modal.OpenModal(ViewType.Account);
        }

        public async void OnGetBalanceButton()
        {
            Debug.Log("[Web3Modal Sample] OnGetBalanceButton");

            try
            {
                Notification.ShowMessage("Getting balance with WalletConnect Blockchain API...");

                var account = Web3Modal.GetAccount();

                var balance = await Web3Modal.Web3.Eth.GetBalance.SendRequestAsync(account.Address);

                Notification.ShowMessage($"Balance: {Web3.Convert.FromWei(balance.Value)} ETH");
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"{nameof(RpcResponseException)}:\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        public async void OnPersonalSignButton()
        {
            Debug.Log("[Web3Modal Sample] OnPersonalSignButton");

            try
            {
                var account = Web3Modal.GetAccount();

                const string message = "Hello WalletConnect!";
                var encodedMessage = new HexUTF8String(message);

                var response = await Web3Modal.Web3.Eth.AccountSigning.PersonalSign.SendRequestAsync(encodedMessage, account.Address);

                Notification.ShowMessage($"Response: {response}");
            }
            catch (RpcResponseException e)
            {
                Notification.ShowMessage($"{nameof(RpcResponseException)}:\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        public async void OnDisconnectButton()
        {
            Debug.Log("[Web3Modal Sample] OnDisconnectButton");

            try
            {
                Notification.ShowMessage($"Disconnecting...");
                await Web3Modal.DisconnectAsync();
                Notification.Hide();
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"{nameof(RpcResponseException)}:\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        public async void OnSendTransactionButton()
        {
            Debug.Log("[Web3Modal Sample] OnSendTransactionButton");

            Notification.ShowMessage("Sending transaction...");

            const string toAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";

            try
            {
                await Web3Modal.Web3.Eth
                    .GetEtherTransferService()
                    .TransferEtherAndWaitForReceiptAsync(toAddress, 0.00001m);

                Notification.ShowMessage("Transaction sent");
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"Error sending transaction.\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        public async void OnSignTypedDataV4Button()
        {
            Debug.Log("[Web3Modal Sample] OnSignTypedDataV4Button");

            Notification.ShowMessage("Signing typed data...");

            var account = Web3Modal.GetAccount();

            var typedData = GetMailTypedDefinition();
            var mail = new Mail
            {
                From = new Person
                {
                    Name = "Cow",
                    Wallets = new List<string>
                    {
                        "0xCD2a3d9F938E13CD947Ec05AbC7FE734Df8DD826",
                        "0xDeaDbeefdEAdbeefdEadbEEFdeadbeEFdEaDbeeF"
                    }
                },
                To = new List<Person>
                {
                    new()
                    {
                        Name = "Bob",
                        Wallets = new List<string>
                        {
                            "0xbBbBBBBbbBBBbbbBbbBbbbbBBbBbbbbBbBbbBBbB",
                            "0xB0BdaBea57B0BDABeA57b0bdABEA57b0BDabEa57",
                            "0xB0B0b0b0b0b0B000000000000000000000000000"
                        }
                    }
                },
                Contents = "Hello, Bob!"
            };

            typedData.Domain.ChainId = BigInteger.Parse(account.ChainId.Split(":")[1]);
            typedData.SetMessage(mail);

            var jsonMessage = typedData.ToJson();

            try
            {
                var response = await Web3Modal.Web3.Eth.AccountSigning.SignTypedDataV4.SendRequestAsync(jsonMessage);
                var recoveredAccount = new Eip712TypedDataSigner().RecoverFromSignatureV4(typedData, response);

                var signatureValid = string.Equals(account.Address, recoveredAccount, StringComparison.CurrentCultureIgnoreCase);
                Notification.ShowMessage($"Signature valid: {signatureValid}\nRecovered Account: {recoveredAccount}");
            }
            catch (Exception e)
            {
                Notification.ShowMessage("Error signing typed data");
                Debug.LogException(e, this);
            }
        }

        public async void OnReverseResolveEns()
        {
            Debug.Log("[Web3Modal Sample] OnReverseResolveEns");

            Notification.ShowMessage("Reverse-resolving ENS with WalletConnect Blockchain API...");

            var account = Web3Modal.GetAccount();

            try
            {
                var addr = await Web3Modal.Web3.Eth.GetEnsService().ReverseResolveAsync(account.Address);
                var result = string.IsNullOrWhiteSpace(addr)
                    ? "No ENS name found"
                    : $"ENS name: {addr}";

                Notification.ShowMessage(result);
            }
            catch (Exception e)
            {
                Notification.ShowMessage("No ENS name found");
            }
        }

        private TypedData<Domain> GetMailTypedDefinition()
        {
            return new TypedData<Domain>
            {
                Domain = new Domain
                {
                    Name = "Ether Mail",
                    Version = "1",
                    ChainId = 1,
                    VerifyingContract = "0xCcCCccccCCCCcCCCCCCcCcCccCcCCCcCcccccccC"
                },
                Types = MemberDescriptionFactory.GetTypesMemberDescription(typeof(Domain), typeof(Group), typeof(Mail), typeof(Person)),
                PrimaryType = nameof(Mail)
            };
        }
    }
}