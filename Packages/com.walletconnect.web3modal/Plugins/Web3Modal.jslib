mergeInto(LibraryManager.library, {
    // Global variable to store the loaded modules and configuration
    _web3ModalConfig: null,

    // Method to preload the scripts from CDN
    PreloadWeb3Modal: function (projectIdPtr, appNameStrPtr, appLogoUrlStrPtr) {
        const projectId = UTF8ToString(projectIdPtr);
        const appName = UTF8ToString(appNameStrPtr);
        const appLogoUrl = UTF8ToString(appLogoUrlStrPtr);

        console.log("Preloading Web3Modal with Project ID:", projectId);

        // Load the scripts and initialize the configuration
        import("https://cdn.jsdelivr.net/npm/cdn-wagmi@3.0.0/dist/cdn-wagmi.js").then(CDNW3M => {
            const { WagmiCore, Chains, Web3modal, Connectors } = CDNW3M;
            const { createWeb3Modal, defaultWagmiConfig } = Web3modal;
            const { mainnet, polygon, sepolia } = Chains;
            const { coinbaseWallet, walletConnect, injected } = Connectors;
            const { createConfig, http, reconnect } = WagmiCore;
            
            console.log("Web3Modal loaded successfully");

            const metadata = {
                name: appName,
                description: 'Web3Modal Example',
                url: 'https://web3modal.com', // url must match your domain & subdomain
                icons: [appLogoUrl]
            };

            const config = createConfig({
                chains: [mainnet, polygon, sepolia],
                // transports: {
                //     [mainnet.id]: http(),
                //     [polygon.id]: http(),
                //     [sepolia.id]: http()
                // },
                connectors: [
                    walletConnect({ projectId, metadata, showQrModal: false }),
                    injected({ shimDisconnect: true }),
                    coinbaseWallet({
                        appName: metadata.name,
                        appLogoUrl: metadata.icons[0]
                    })
                ]
            });
            
            console.log(config.connectors);
            console.log("Web3Modal configuration loaded successfully");

            reconnect(config);

            const modal = createWeb3Modal({
                wagmiConfig: config,
                projectId,
                enableAnalytics: true, // Optional - defaults to your Cloud configuration
                enableOnramp: true // Optional - false as default
            });

            console.log("Web3Modal modal created successfully", modal);
            
            // Store the configuration and modal globally
            _web3ModalConfig = {
                config: config,
                modal: modal,
                wagmiCore: WagmiCore
            };
        });
    },
    OpenWeb3Modal: function () {
        console.log("Opening Web3Modal", _web3ModalConfig);
        if (_web3ModalConfig) {
            _web3ModalConfig.modal.open();
        } else {
            console.error("Web3Modal is not initialized. Call PreloadWeb3Modal first.");
        }
    },
    WagmiCall: async function(id, methodNameStrPtr, parameterStrPtr, callbackPtr) {
        if (!_web3ModalConfig) {
            console.error("Web3Modal is not initialized. Call PreloadWeb3Modal first.");
            return;
        }
        
        // Convert the method name and parameter to JS strings
        let methodName = UTF8ToString(methodNameStrPtr);
        let parameterStr = UTF8ToString(parameterStrPtr);
        
        let parameterObj = parameterStr === "" ? undefined : JSON.parse(parameterStr);
        
        try {
            if (typeof _web3ModalConfig.wagmiCore[methodName] !== 'function') {
                throw new Error(`Method ${methodName} does not exist on wagmiCore.`);
            }
            
            console.log("Calling WagmiCore method", methodName, parameterObj);
            
            // Call the method and get the result
            let result = await _web3ModalConfig.wagmiCore[methodName](_web3ModalConfig.config, parameterObj);
            
            // Convert the result to JSON
            let cache = [];
            let resultJson = JSON.stringify(result, (key, value) => {
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
            
            // Call the callback with the result
            let resultStrPtr = stringToNewUTF8(resultJson);
            {{{ makeDynCall('viii', 'callbackPtr') }}} (id, resultStrPtr, undefined);
            _free(resultStrPtr);
        } catch (error) {
            let errorJson = JSON.stringify(error, ['name', 'message']);
            let errorStrPtr = stringToNewUTF8(errorJson);
            {{{ makeDynCall('viii', 'callbackPtr') }}} (id, undefined, errorStrPtr);
            _free(errorStrPtr);
        }
    }
});
