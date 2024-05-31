mergeInto(LibraryManager.library, {
    // Global variable to store the loaded modules and configuration
    _web3ModalConfig: null,

    $SerializeJson: function (obj){
        let cache = [];
        let resultJson = JSON.stringify(obj, (key, value) => {
            // Handle circular references
            if (typeof value === 'object' && value !== null) {
                if (cache.includes(value)) return;
                cache.push(value);
            }
            // Check if the value is a BigInt and convert it to a string
            if (typeof value === 'bigint') {
                return value.toString();
            }
            return value;
        });
        cache = null;
        return resultJson;
    },
    
    $ExecuteCall__deps: ['$SerializeJson'],
    $ExecuteCall: async function (callFn, id, methodNameStrPtr, parameterStrPtr, callbackPtr) {
        console.log("Executing call");
        if (!_web3ModalConfig) {
            console.error("Web3Modal is not initialized. Call Initialize first.");
            return;
        }

        // Convert the method name and parameter to JS strings
        let methodName = UTF8ToString(methodNameStrPtr);
        let parameterStr = UTF8ToString(parameterStrPtr);

        let parameterObj = parameterStr === "" ? undefined : JSON.parse(parameterStr);

        try {
            console.log(`Calling method`, methodName, parameterObj);

            // Call the method using the provided function
            let result = await callFn(_web3ModalConfig, methodName, parameterObj);
            
            if (!result) {
                {{{makeDynCall('viii', 'callbackPtr')}}} (id, undefined, undefined);
                return;
            }

            // Convert the result to JSON
            let cache = [];
            let resultJson = SerializeJson(result);

            // Call the callback with the result
            let resultStrPtr = stringToNewUTF8(resultJson);
            {{{makeDynCall('viii', 'callbackPtr')}}} (id, resultStrPtr, undefined);
            _free(resultStrPtr);
        } catch (error) {
            console.log("Error!", error);
            let errorJson = JSON.stringify(error, ['name', 'message']);
            let errorStrPtr = stringToNewUTF8(errorJson);
            {{{makeDynCall('viii', 'callbackPtr')}}} (id, undefined, errorStrPtr);
            _free(errorStrPtr);
        }
    },

    // Preload the scripts from CDN, initialize the configuration and create the modal
    Initialize: function (parametersJsonPtr, callbackPtr) {
        const parametersJson = UTF8ToString(parametersJsonPtr);
        const parameters = JSON.parse(parametersJson);
        
        const projectId = parameters.projectId;
        const metadata = parameters.metadata;
        const chains = parameters.chains;
        
        const enableOnramp = parameters.enableOnramp;
        
        console.log("Parameters", parameters);

        const addCanvasActiveStyles = () => {
            const styleElement = document.createElement('style');
            styleElement.id = 'canvas-active-styles';
            styleElement.innerHTML = `
                .canvas-active {
                    position: fixed !important;
                    top: 0 !important;
                    right: 0 !important;
                    bottom: 0 !important;
                    left: 0 !important;
                    width: 100% !important;
                    height: 100% !important;
                }
            `;
            document.head.appendChild(styleElement);
        };

        const removeCanvasActiveStyles = () => {
            const styleElement = document.getElementById('canvas-active-styles');
            if (styleElement) {
                document.head.removeChild(styleElement);
            }
        };

        const html = document.querySelector('html');
        html?.addEventListener('fullscreenchange', () => {
            const canvas = document.querySelector('canvas');
            console.log('fullscreenchange', canvas)
            if (document.fullscreenElement) {
                console.log('fullscreenchange true')
                if (!canvas?.classList.contains('canvas-active')) {
                    console.log('fullscreenchange true add canvas-active')

                    addCanvasActiveStyles();
                    canvas?.classList.add('canvas-active');
                }
            } else {
                console.log('fullscreenchange false')
                if (canvas?.classList.contains('canvas-active')) {
                    console.log('fullscreenchange false remove canvas-active')
                    
                    canvas?.classList.remove('canvas-active');
                    removeCanvasActiveStyles();
                }
            }
        });
        
        // Load the scripts and initialize the configuration
        import("https://cdn.jsdelivr.net/npm/cdn-wagmi@3.0.0/dist/cdn-wagmi.js").then(CDNW3M => {
            const { WagmiCore, Chains, Web3modal, Connectors } = CDNW3M;
            const { createWeb3Modal } = Web3modal;
            const { coinbaseWallet, walletConnect, injected } = Connectors;
            const { createConfig, http, reconnect } = WagmiCore;

            console.log("Web3Modal loaded from CDN");
            
            const chainsMap = chains.map(chainName => Chains[chainName]);

            const config = createConfig({
                chains: chainsMap,
                transport: http,
                connectors: [
                    walletConnect({ projectId, metadata, showQrModal: false }),
                    injected({ shimDisconnect: true }),
                    coinbaseWallet({
                        appName: metadata.name,
                        appLogoUrl: metadata.icons[0]
                    })
                ]
            });
            
            reconnect(config);

            const modal = createWeb3Modal({
                wagmiConfig: config,
                projectId,
                enableAnalytics: false, // Optional - defaults to your Cloud configuration
                enableOnramp: enableOnramp // Optional - false as default
            });
            
            modal.subscribeEvents(event => console.log("New Event!", event));

            console.log("Web3Modal modal created", modal);

            // Store the configuration and modal globally
            _web3ModalConfig = {
                config: config,
                modal: modal,
                wagmiCore: WagmiCore
            };

            {{{makeDynCall('v', 'callbackPtr')}}}();
        });
    },

    ModalCall__deps: ['$ExecuteCall'],
    ModalCall: async function (id, methodNameStrPtr, parameterStrPtr, callbackPtr) {
        const callFn = async (web3modalConfig, methodName, parameterObj) => {
            console.log("ModalCall", methodName, parameterObj, web3modalConfig);
            return await web3modalConfig.modal[methodName](parameterObj);
        };
        await ExecuteCall(callFn, id, methodNameStrPtr, parameterStrPtr, callbackPtr);
    },

    WagmiCall__deps: ['$ExecuteCall'],
    WagmiCall: async function(id, methodNameStrPtr, parameterStrPtr, callbackPtr) {
        const callFn = async (web3modalConfig, methodName, parameterObj) => {
            return await web3modalConfig.wagmiCore[methodName](web3modalConfig.config, parameterObj);
        };
        await ExecuteCall(callFn, id, methodNameStrPtr, parameterStrPtr, callbackPtr);
    },
    
    WagmiWatchAccount__deps: ['$SerializeJson'],
    WagmiWatchAccount: function(callbackPtr) {
        _web3ModalConfig.wagmiCore.watchAccount(_web3ModalConfig.config, {
            onChange(data) {
                const dataStr = stringToNewUTF8(SerializeJson(data));
                {{{makeDynCall('vi', 'callbackPtr')}}}(dataStr);
                _free(dataStr);
            }
        });
    },
    
    WagmiWatchChainId__deps: ['$SerializeJson'],
    WagmiWatchChainId: function(callbackPtr) {
        _web3ModalConfig.wagmiCore.watchChainId(_web3ModalConfig.config, {
            onChange(data) {
                {{{makeDynCall('vi', 'callbackPtr')}}}(data);
                _free(dataStr);
            }
        });
    },

    ModalSubscribeState__deps: ['$SerializeJson'],
    ModalSubscribeState: function(callbackPtr) {
        _web3ModalConfig.modal.subscribeState(newState => {
            const newStateStr = stringToNewUTF8(SerializeJson(newState));
            {{{makeDynCall('vi', 'callbackPtr')}}}(newStateStr);
            _free(newStateStr);
        });
    },
});
