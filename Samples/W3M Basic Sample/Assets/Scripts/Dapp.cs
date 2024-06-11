using System;
using System.Collections.Generic;
using System.Numerics;
using Nethereum.ABI.EIP712;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Button _readContractButton;
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
                    _readContractButton.interactable = false;
                    _disconnectButton.interactable = false;

                    _activeAddress.text = string.Empty;
                    _activeChainId.text = string.Empty;
                };

                Web3Modal.AccountChanged += (_, e) =>
                {
                    var account = e.Account;
                    _activeAddress.text = account.Address;
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

                const string message = "Hello from the service!";
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
        
        
        public const string CryptoPunksAbi = @"[{""constant"":true,""inputs"":[],""name"":""name"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[{""name"":"""",""type"":""uint256""}],""name"":""punksOfferedForSale"",""outputs"":[{""name"":""isForSale"",""type"":""bool""},{""name"":""punkIndex"",""type"":""uint256""},{""name"":""seller"",""type"":""address""},{""name"":""minValue"",""type"":""uint256""},{""name"":""onlySellTo"",""type"":""address""}],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""punkIndex"",""type"":""uint256""}],""name"":""enterBidForPunk"",""outputs"":[],""payable"":true,""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""totalSupply"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""punkIndex"",""type"":""uint256""},{""name"":""minPrice"",""type"":""uint256""}],""name"":""acceptBidForPunk"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""decimals"",""outputs"":[{""name"":"""",""type"":""uint8""}],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""addresses"",""type"":""address[]""},{""name"":""indices"",""type"":""uint256[]""}],""name"":""setInitialOwners"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[],""name"":""withdraw"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""imageHash"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""nextPunkIndexToAssign"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[{""name"":"""",""type"":""uint256""}],""name"":""punkIndexToAddress"",""outputs"":[{""name"":"""",""type"":""address""}],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""standard"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[{""name"":"""",""type"":""uint256""}],""name"":""punkBids"",""outputs"":[{""name"":""hasBid"",""type"":""bool""},{""name"":""punkIndex"",""type"":""uint256""},{""name"":""bidder"",""type"":""address""},{""name"":""value"",""type"":""uint256""}],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[{""name"":"""",""type"":""address""}],""name"":""balanceOf"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[],""name"":""allInitialOwnersAssigned"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""allPunksAssigned"",""outputs"":[{""name"":"""",""type"":""bool""}],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""punkIndex"",""type"":""uint256""}],""name"":""buyPunk"",""outputs"":[],""payable"":true,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""to"",""type"":""address""},{""name"":""punkIndex"",""type"":""uint256""}],""name"":""transferPunk"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""symbol"",""outputs"":[{""name"":"""",""type"":""string""}],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""punkIndex"",""type"":""uint256""}],""name"":""withdrawBidForPunk"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""to"",""type"":""address""},{""name"":""punkIndex"",""type"":""uint256""}],""name"":""setInitialOwner"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""punkIndex"",""type"":""uint256""},{""name"":""minSalePriceInWei"",""type"":""uint256""},{""name"":""toAddress"",""type"":""address""}],""name"":""offerPunkForSaleToAddress"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[],""name"":""punksRemainingToAssign"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""punkIndex"",""type"":""uint256""},{""name"":""minSalePriceInWei"",""type"":""uint256""}],""name"":""offerPunkForSale"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""punkIndex"",""type"":""uint256""}],""name"":""getPunk"",""outputs"":[],""payable"":false,""type"":""function""},
        {""constant"":true,""inputs"":[{""name"":"""",""type"":""address""}],""name"":""pendingWithdrawals"",""outputs"":[{""name"":"""",""type"":""uint256""}],""payable"":false,""type"":""function""},
        {""constant"":false,""inputs"":[{""name"":""punkIndex"",""type"":""uint256""}],""name"":""punkNoLongerForSale"",""outputs"":[],""payable"":false,""type"":""function""},
        {""inputs"":[],""payable"":true,""type"":""constructor""},
        {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""to"",""type"":""address""},{""indexed"":false,""name"":""punkIndex"",""type"":""uint256""}],""name"":""Assign"",""type"":""event""},
        {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""from"",""type"":""address""},{""indexed"":true,""name"":""to"",""type"":""address""},{""indexed"":false,""name"":""value"",""type"":""uint256""}],""name"":""Transfer"",""type"":""event""},
        {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""from"",""type"":""address""},{""indexed"":true,""name"":""to"",""type"":""address""},{""indexed"":false,""name"":""punkIndex"",""type"":""uint256""}],""name"":""PunkTransfer"",""type"":""event""},
        {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""punkIndex"",""type"":""uint256""},{""indexed"":false,""name"":""minValue"",""type"":""uint256""},{""indexed"":true,""name"":""toAddress"",""type"":""address""}],""name"":""PunkOffered"",""type"":""event""},
        {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""punkIndex"",""type"":""uint256""},{""indexed"":false,""name"":""value"",""type"":""uint256""},{""indexed"":true,""name"":""fromAddress"",""type"":""address""}],""name"":""PunkBidEntered"",""type"":""event""},
        {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""punkIndex"",""type"":""uint256""},{""indexed"":false,""name"":""value"",""type"":""uint256""},{""indexed"":true,""name"":""fromAddress"",""type"":""address""}],""name"":""PunkBidWithdrawn"",""type"":""event""},
        {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""punkIndex"",""type"":""uint256""},{""indexed"":false,""name"":""value"",""type"":""uint256""},{""indexed"":true,""name"":""fromAddress"",""type"":""address""},{""indexed"":true,""name"":""toAddress"",""type"":""address""}],""name"":""PunkBought"",""type"":""event""},
        {""anonymous"":false,""inputs"":[{""indexed"":true,""name"":""punkIndex"",""type"":""uint256""}],""name"":""PunkNoLongerForSale"",""type"":""event""}]";
    }
}