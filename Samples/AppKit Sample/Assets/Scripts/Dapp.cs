using System;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Skibitsky.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WalletConnect.Web3Modal.Sample
{
    public class Dapp : MonoBehaviour
    {
        private bool _resumed;

        [SerializeField] private TMP_Text _activeChainId;
        [SerializeField] private TMP_Text _initializingLabel;

        [Space] [SerializeField] private Button _connectButton;
        [SerializeField] private Button _networkButton;
        [SerializeField] private Button _accountButton;
        [SerializeField] private Button _signTypedDataButton;
        [SerializeField] private Button _transactionButton;
        [SerializeField] private Button _personalSignButton;
        [SerializeField] private Button _getBalanceButton;
        [SerializeField] private Button _readContractButton;
        [SerializeField] private Button _disconnectButton;

        private void Awake()
        {
            Application.targetFrameRate = Screen.currentResolution.refreshRate;
            _connectButton.interactable = false;
        }

        private async void Start()
        {
            if (!Web3Modal.IsInitialized)
            {
                Notification.ShowMessage("Web3Modal is not initialized. Please initialize Web3Modal first.");
                return;
            }
            
            try
            {
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

                Web3Modal.AccountConnected += async (_, e) =>
                {
                    _connectButton.interactable = false;
                    _accountButton.interactable = true;
                    _signTypedDataButton.interactable = true;
                    _transactionButton.interactable = true;
                    _personalSignButton.interactable = true;
                    _getBalanceButton.interactable = true;
                    _readContractButton.interactable = true;
                    _disconnectButton.interactable = true;

                    var account = await e.GetAccount();
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
                    _readContractButton.interactable = false;
                    _disconnectButton.interactable = false;

                    _activeChainId.text = string.Empty;
                };

                Web3Modal.AccountChanged += (_, e) =>
                {
                    var account = e.Account;
                    _activeChainId.text = account.ChainId;
                };

                _resumed = await Web3Modal.ConnectorController.TryResumeSessionAsync();
                
                _networkButton.interactable = true;
                _connectButton.interactable = !_resumed;
                _accountButton.interactable = _resumed;
                _signTypedDataButton.interactable = _resumed;
                _transactionButton.interactable = _resumed;
                _personalSignButton.interactable = _resumed;
                _getBalanceButton.interactable = _resumed;
                _readContractButton.interactable = _resumed;
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

                var account = await Web3Modal.GetAccountAsync();

                var balance = await Web3Modal.Evm.GetBalanceAsync(account.Address);

                Notification.ShowMessage($"Balance: {Web3.Convert.FromWei(balance)} ETH");
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
                var account = await Web3Modal.GetAccountAsync();

                const string message = "Hello from Unity!";
                var signature = await Web3Modal.Evm.SignMessageAsync(message);
                var isValid = await Web3Modal.Evm.VerifyMessageSignatureAsync(account.Address, message, signature);
                
                Notification.ShowMessage($"Signature valid: {isValid}");
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
                Notification.ShowMessage($"{e.GetType()}:\n{e.Message}");
                Debug.LogException(e, this);
            }
        }

        public async void OnSendTransactionButton()
        {
            Debug.Log("[Web3Modal Sample] OnSendTransactionButton");
            
            const string toAddress = "0xd8dA6BF26964aF9D7eEd9e03E53415D37aA96045";

            try
            {
                Notification.ShowMessage("Sending transaction...");
                
                var value = Web3.Convert.ToWei(0.001);
                var result = await Web3Modal.Evm.SendTransactionAsync(toAddress, value);
                Debug.Log("Transaction hash: " + result);

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

            var account = await Web3Modal.GetAccountAsync();

            Debug.Log("Get mail typed definition");
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
                var signature = await Web3Modal.Evm.SignTypedDataAsync(jsonMessage);
                
                var isValid = await Web3Modal.Evm.VerifyTypedDataSignatureAsync(account.Address, jsonMessage, signature);
                
                Notification.ShowMessage($"Signature valid: {isValid}");
            }
            catch (Exception e)
            {
                Notification.ShowMessage("Error signing typed data");
                Debug.LogException(e, this);
            }
        }
        
        public async void OnReadContractClicked()
        {
            if (Web3Modal.NetworkController.ActiveChain.ChainId != "1")
            {
                Notification.ShowMessage("Please switch to Ethereum mainnet.");
                return;
            }
            
            const string contractAddress = "0xb47e3cd837ddf8e4c57f05d70ab865de6e193bbb"; // on Ethereum mainnet
            const string yugaLabsAddress = "0xA858DDc0445d8131daC4d1DE01f834ffcbA52Ef1";
            const string abi = CryptoPunksAbi;
            
            Notification.ShowMessage("Reading smart contract state...");
            
            try
            {
                var tokenName = await Web3Modal.Evm.ReadContractAsync<string>(contractAddress, abi, "name");
                Debug.Log($"Token name: {tokenName}");

                var balance = await Web3Modal.Evm.ReadContractAsync<BigInteger>(contractAddress, abi, "balanceOf", new object[]
                {
                    yugaLabsAddress
                });
                var result = $"Yuga Labs owns: {balance} {tokenName} tokens active chain.";
                
                Notification.ShowMessage(result);
            }
            catch (Exception e)
            {
                Notification.ShowMessage($"Contract reading error.\n{e.Message}");
                Debug.LogException(e, this);
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
        
        public const string CryptoPunksAbi =
            @"[{""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],""payable"":false,""stateMutability"":""view"",""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""stateMutability"":""view"",""type"":""function""}]";
    }
}