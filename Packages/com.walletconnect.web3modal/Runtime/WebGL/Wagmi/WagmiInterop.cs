using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WalletConnect.Web3Modal.WebGl.Wagmi
{
    public class WagmiInterop : InteropService
    {
        [DllImport("__Internal")]
        private static extern void WagmiCall(int id, string methodName, string payload, ExternalMethodCallback callback);

        public WagmiInterop() : base(WagmiCall)
        {
        }

        // -- Get Account ----------------------------------------------
        public Task<GetAccountReturnType> GetAccountAsync()
        {
            return InteropCallAsync<object, GetAccountReturnType>(WagmiMethods.GetAccount, null);
        }

        // -- Get Chain ID ---------------------------------------------
        public Task<int> GetChainIdAsync()
        {
            return InteropCallAsync<object, int>(WagmiMethods.GetChainId, null);
        }


        // -- Sign Message ---------------------------------------------
        public Task<string> SignMessageAsync(string message)
        {
            var parameter = new SignMessageParameter
            {
                message = message
            };

            return SignMessageAsync(parameter);
        }

        public Task<string> SignMessageAsync(SignMessageParameter parameter)
        {
            return InteropCallAsync<SignMessageParameter, string>(WagmiMethods.SignMessage, parameter);
        }


        // -- Verify Message -------------------------------------------

        public Task<bool> VerifyMessageAsync(string address, string message, string signature)
        {
            var parameter = new VerifyMessageParameters
            {
                address = address,
                message = message,
                signature = signature
            };

            return VerifyMessageAsync(parameter);
        }

        public Task<bool> VerifyMessageAsync(VerifyMessageParameters parameter)
        {
            return InteropCallAsync<VerifyMessageParameters, bool>(WagmiMethods.VerifyMessage, parameter);
        }


        // -- Sign Typed Data ------------------------------------------

        public Task<string> SignTypedDataAsync(string dataJson)
        {
            return InteropCallAsync<string, string>(WagmiMethods.SignTypedData, dataJson);
        }


        // -- Verify Typed Data ----------------------------------------

        public Task<bool> VerifyTypedDataAsync(string dataJson, string address, string signature)
        {
            var jObject = JObject.Parse(dataJson);

            jObject[nameof(address)] = JToken.FromObject(address);
            jObject[nameof(signature)] = JToken.FromObject(signature);

            var parameter = jObject.ToString(Formatting.None);

            return InteropCallAsync<string, bool>(WagmiMethods.VerifyTypedData, parameter);
        }


        // -- Switch Chain ---------------------------------------------

        public Task SwitchChainAsync(int chainId, AddEthereumChainParameter addEthereumChainParameter = null)
        {
            var switchChainParameter = new SwitchChainParameter
            {
                chainId = chainId,
                parameter = addEthereumChainParameter
            };

            return SwitchChainAsync(switchChainParameter);
        }

        public Task SwitchChainAsync(SwitchChainParameter parameter)
        {
            return InteropCallAsync<SwitchChainParameter, string>(WagmiMethods.SwitchChain, parameter);
        }

        // -- Read Contract -------------------------------------------

        public Task<string> ReadContractAsync(string contractAddress, string contractAbi, string method, string[] arguments = null)
        {
            var parameter = new ReadContractParameter
            {
                address = contractAddress,
                abi = JsonConvert.DeserializeObject<AbiItem[]>(contractAbi),
                functionName = method,
                args = arguments
            };

            return ReadContractAsync(parameter);
        }

        public Task<string> ReadContractAsync(ReadContractParameter parameter)
        {
            return InteropCallAsync<ReadContractParameter, string>(WagmiMethods.ReadContract, parameter);
        }


        // -- Send Transaction ----------------------------------------

        public Task<string> SendTransactionAsync(string to, string value = "0", string data = null, string gas = null, string gasPrice = null)
        {
            var parameter = new SendTransactionParameter
            {
                to = to,
                value = value,
                data = data,
                gas = gas,
                gasPrice = gasPrice
            };

            return SendTransactionAsync(parameter);
        }

        public Task<string> SendTransactionAsync(SendTransactionParameter parameter)
        {
            return InteropCallAsync<SendTransactionParameter, string>(WagmiMethods.SendTransaction, parameter);
        }
    }
}