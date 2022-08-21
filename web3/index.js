// load web3gl to connect to unity
window.web3gl = {
  eventListenerGameObject: "",
  handleRequest,
};

// will be defined after connect()
let provider;
let web3;

async function handleRequest(req) {
  try {
    let response;
    if (req.action === "Connect") {
      response = await connectToWallet(req.args);
    }
    else if (req.action === "CallContract") {
      response = await callContract(req.args);
    }
    else if (req.action === "SendContract") {
      response = await sendContract(req.args);
    }
    else if (req.action === "SendTransaction") {
      response = await sendTransaction(req.args);
    }
    else if (req.action === "SignMessage") {
      response = await signMessage(req.args);
    }
    else if (req.action === "GetPastEvents") {
      response = await getPastEvents(req.args);
    }
    else if (req.action === "GetLatestBlockNumber") {
      response = await getLatestBlockNumber();
    }
    else {
      throw new Error("Unknown action: " + req.action);
    }
    
    unityInstance.SendMessage(window.web3gl.eventListenerGameObject, "OnResponse", JSON.stringify({
      requestId: req.requestId,
      value: response
    }));
  }
  catch (error) {
    unityInstance.SendMessage(window.web3gl.eventListenerGameObject, "OnError", JSON.stringify({
      requestId: req.requestId,
      code: error.code,
      message: error.message
    }));
  }
}

async function connectToWallet(args) {
  const web3Modal = new window.Web3Modal.default({
    providerOptions: {}
  });

  // set provider
  provider = await web3Modal.connect();
  web3 = new Web3(provider);

  // if current network id is not equal to desired network id, then switch
  if (parseInt(provider.chainId) !== args.chainId) {
    try {
      await window.ethereum.request({
        method: "wallet_switchEthereumChain",
        params: [{ chainId: `0x${args.chainId.toString(16)}` }], // chainId must be in hexadecimal numbers
      });
    } catch {
      // if network isn't added, pop-up metamask to add
      await addEthereumChain(args.chainId);
    }
  }

  const accounts = await web3.eth.getAccounts();

  provider.on("connect", (connectInfo) => {
    unityInstance.SendMessage(window.web3gl.eventListenerGameObject, "OnConnect", Number(connectInfo.chainId));
  });

  provider.on("disconnect", (error) => {
    unityInstance.SendMessage(window.web3gl.eventListenerGameObject, "OnDisconnect", error.message);
  });

  provider.on("accountsChanged", (accounts) => {
    let accountAddr = accounts[0] || "";
    unityInstance.SendMessage(window.web3gl.eventListenerGameObject, "OnAccountChanged", accountAddr);
  });

  provider.on("chainChanged", (chainId) => {
    unityInstance.SendMessage(window.web3gl.eventListenerGameObject, "OnChainChanged", Number(chainId));
  });

  return accounts[0] || "";
}

async function signMessage(args) {
  const from = (await web3.eth.getAccounts())[0];
  return await web3.eth.personal.sign(args.message, from, "");
}

async function sendTransaction(args) {
  const from = (await web3.eth.getAccounts())[0];
  return await new Promise((resolve, reject) => {
    web3.eth
      .sendTransaction({
        from,
        to: args.to,
        value: args.value,
        gas: args.gasLimit ? args.gasLimit : undefined,
        gasPrice: args.gasPrice ? args.gasPrice : undefined,
      })
      .on("transactionHash", resolve)
      .on("error", reject);
  });
}

async function callContract(args) {
  const from = (await web3.eth.getAccounts())[0];
  return JSON.stringify(
    await new web3.eth.Contract(JSON.parse(args.abi), args.contract)
      .methods[args.method](...args.parameters)
      .call({from})
  );
}

async function sendContract(args) {
  const from = (await web3.eth.getAccounts())[0];
  return await new Promise((resolve, reject) => {
    new web3.eth.Contract(JSON.parse(args.abi), args.contract)
      .methods[args.method](...args.parameters)
      .send({
        from,
        value: args.value,
        gas: args.gasLimit ? args.gasLimit : undefined,
        gasPrice: args.gasPrice ? args.gasPrice : undefined,
      })
      .on("transactionHash", resolve)
      .on("error", reject);
  });
}

async function getPastEvents(args) {
  const filters = {};
  args.filters.forEach(f => {
    const values = filters[f.param] || [];
    values.push(f.value);
    filters[f.param] = values;
  });

  const events = await new web3.eth.Contract(JSON.parse(args.abi), args.contract)
    .getPastEvents(
      args.eventName,
      {
        filters,
        fromBlock: args.fromBlock,
        toBlock: args.toBlock,
      }
    );

  return JSON.stringify(events.map(ev => {
      return {
        address: ev.address,
        blockNumber: ev.blockNumber,
        transactionHash: ev.transactionHash,
        topics: ev.raw.topics,
        parameters: Object.entries(ev.returnValues).map(entry => {
            return { param: entry[0], value: entry[1] };
        })
      };
    }));
}

async function getLatestBlockNumber() {
  return (await web3.eth.getBlockNumber()).toString();
}

// add new wallet to in metamask
async function addEthereumChain(chainId) {
  const account = (await web3.eth.getAccounts())[0];

  // fetch https://chainid.network/chains.json
  const response = await fetch("https://chainid.network/chains.json");
  const chains = await response.json();

  // find chain with network id
  const chain = chains.find((chain) => chain.chainId === chainId);

  const params = {
    chainId: "0x" + chain.chainId.toString(16), // A 0x-prefixed hexadecimal string
    chainName: chain.name,
    nativeCurrency: {
      name: chain.nativeCurrency.name,
      symbol: chain.nativeCurrency.symbol, // 2-6 characters long
      decimals: chain.nativeCurrency.decimals,
    },
    rpcUrls: chain.rpc,
    blockExplorerUrls: [chain.explorers && chain.explorers.length > 0 && chain.explorers[0].url ? chain.explorers[0].url : chain.infoURL],
  };

  await window.ethereum
    .request({
      method: "wallet_addEthereumChain",
      params: [params, account],
    });
}
