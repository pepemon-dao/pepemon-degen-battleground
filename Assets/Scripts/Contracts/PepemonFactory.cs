using Contracts.PepemonFactory.abi.ContractDefinition;
//using Nethereum.Unity.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Thirdweb;
using UnityEngine;

class PepemonFactory : ERC1155Common
{
    /// <summary>
    /// PepemonFactory address (Support cards, Battle cards)
    /// </summary>
    private static string Address => Web3Controller.instance.GetChainConfig().pepemonFactoryAddress;
    private static string Abi => "[{\"inputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"constructor\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"bool\",\"name\":\"approved\",\"type\":\"bool\"}],\"name\":\"ApprovalForAll\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"MinterAdded\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"MinterRemoved\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"previousOwner\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"OwnershipTransferred\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"ids\",\"type\":\"uint256[]\"},{\"indexed\":false,\"internalType\":\"uint256[]\",\"name\":\"values\",\"type\":\"uint256[]\"}],\"name\":\"TransferBatch\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"operator\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"from\",\"type\":\"address\"},{\"indexed\":true,\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"indexed\":false,\"internalType\":\"uint256\",\"name\":\"value\",\"type\":\"uint256\"}],\"name\":\"TransferSingle\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":false,\"internalType\":\"string\",\"name\":\"value\",\"type\":\"string\"},{\"indexed\":true,\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"}],\"name\":\"URI\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"WhitelistAdminAdded\",\"type\":\"event\"},{\"anonymous\":false,\"inputs\":[{\"indexed\":true,\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"WhitelistAdminRemoved\",\"type\":\"event\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"addMinter\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"addWhitelistAdmin\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"},{\"internalType\":\"address[]\",\"name\":\"_addresses\",\"type\":\"address[]\"}],\"name\":\"airdrop\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_owner\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"}],\"name\":\"balanceOf\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address[]\",\"name\":\"_owners\",\"type\":\"address[]\"},{\"internalType\":\"uint256[]\",\"name\":\"_ids\",\"type\":\"uint256[]\"}],\"name\":\"balanceOfBatch\",\"outputs\":[{\"internalType\":\"uint256[]\",\"name\":\"\",\"type\":\"uint256[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"baseMetadataURI\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"minId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxId\",\"type\":\"uint256\"}],\"name\":\"batchGetBattleCardStats\",\"outputs\":[{\"components\":[{\"internalType\":\"uint16\",\"name\":\"element\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"hp\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"speed\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"intelligence\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"defense\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"attack\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"specialAttack\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"specialDefense\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"level\",\"type\":\"uint16\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"description\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"ipfsAddr\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"rarity\",\"type\":\"string\"}],\"internalType\":\"struct PepemonStats.BattleCardStats[]\",\"name\":\"\",\"type\":\"tuple[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"minId\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"maxId\",\"type\":\"uint256\"}],\"name\":\"batchGetSupportCardStats\",\"outputs\":[{\"components\":[{\"internalType\":\"bytes32\",\"name\":\"currentRoundChanges\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"nextRoundChanges\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"specialCode\",\"type\":\"uint256\"},{\"internalType\":\"uint16\",\"name\":\"modifierNumberOfNextTurns\",\"type\":\"uint16\"},{\"internalType\":\"bool\",\"name\":\"isOffense\",\"type\":\"bool\"},{\"internalType\":\"bool\",\"name\":\"isNormal\",\"type\":\"bool\"},{\"internalType\":\"bool\",\"name\":\"isStackable\",\"type\":\"bool\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"description\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"ipfsAddr\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"rarity\",\"type\":\"string\"}],\"internalType\":\"struct PepemonStats.SupportCardStats[]\",\"name\":\"\",\"type\":\"tuple[]\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"start\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"end\",\"type\":\"uint256\"},{\"internalType\":\"address\",\"name\":\"to\",\"type\":\"address\"}],\"name\":\"batchMint\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"battleCardStats\",\"outputs\":[{\"internalType\":\"uint16\",\"name\":\"element\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"hp\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"speed\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"intelligence\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"defense\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"attack\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"specialAttack\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"specialDefense\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"level\",\"type\":\"uint16\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"description\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"ipfsAddr\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"rarity\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_account\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_amount\",\"type\":\"uint256\"}],\"name\":\"burn\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"contractURI\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"int16[14]\",\"name\":\"arr\",\"type\":\"int16[14]\"}],\"name\":\"convert\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"\",\"type\":\"bytes32\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"uint16\",\"name\":\"element\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"hp\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"speed\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"intelligence\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"defense\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"attack\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"specialAttack\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"specialDefense\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"level\",\"type\":\"uint16\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"description\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"ipfsAddr\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"rarity\",\"type\":\"string\"}],\"internalType\":\"struct PepemonStats.BattleCardStats\",\"name\":\"_stats\",\"type\":\"tuple\"},{\"internalType\":\"uint256\",\"name\":\"_maxSupply\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_initialSupply\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"_uri\",\"type\":\"string\"},{\"internalType\":\"bytes\",\"name\":\"_data\",\"type\":\"bytes\"}],\"name\":\"createBattleCard\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"components\":[{\"internalType\":\"bytes32\",\"name\":\"currentRoundChanges\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"nextRoundChanges\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"specialCode\",\"type\":\"uint256\"},{\"internalType\":\"uint16\",\"name\":\"modifierNumberOfNextTurns\",\"type\":\"uint16\"},{\"internalType\":\"bool\",\"name\":\"isOffense\",\"type\":\"bool\"},{\"internalType\":\"bool\",\"name\":\"isNormal\",\"type\":\"bool\"},{\"internalType\":\"bool\",\"name\":\"isStackable\",\"type\":\"bool\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"description\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"ipfsAddr\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"rarity\",\"type\":\"string\"}],\"internalType\":\"struct PepemonStats.SupportCardStats\",\"name\":\"_stats\",\"type\":\"tuple\"},{\"internalType\":\"uint256\",\"name\":\"_maxSupply\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_initialSupply\",\"type\":\"uint256\"},{\"internalType\":\"string\",\"name\":\"_uri\",\"type\":\"string\"},{\"internalType\":\"bytes\",\"name\":\"_data\",\"type\":\"bytes\"}],\"name\":\"createSupportCard\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"tokenId\",\"type\":\"uint256\"}],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"creators\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes32\",\"name\":\"num\",\"type\":\"bytes32\"}],\"name\":\"deconvert\",\"outputs\":[{\"internalType\":\"int16[14]\",\"name\":\"\",\"type\":\"int16[14]\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint16\",\"name\":\"\",\"type\":\"uint16\"}],\"name\":\"elementDecode\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"}],\"name\":\"endMinting\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"getLastTokenID\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_owner\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"_operator\",\"type\":\"address\"}],\"name\":\"isApprovedForAll\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"isOperator\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"isMinter\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"isWhitelistAdmin\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"}],\"name\":\"maxSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_quantity\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"_data\",\"type\":\"bytes\"}],\"name\":\"mint\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"name\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"owner\",\"outputs\":[{\"internalType\":\"address\",\"name\":\"\",\"type\":\"address\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"removeMinter\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"account\",\"type\":\"address\"}],\"name\":\"removeWhitelistAdmin\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"renounceMinter\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"renounceOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"renounceWhitelistAdmin\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"_to\",\"type\":\"address\"},{\"internalType\":\"uint256[]\",\"name\":\"_ids\",\"type\":\"uint256[]\"},{\"internalType\":\"uint256[]\",\"name\":\"_amounts\",\"type\":\"uint256[]\"},{\"internalType\":\"bytes\",\"name\":\"_data\",\"type\":\"bytes\"}],\"name\":\"safeBatchTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_from\",\"type\":\"address\"},{\"internalType\":\"address\",\"name\":\"_to\",\"type\":\"address\"},{\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"},{\"internalType\":\"uint256\",\"name\":\"_amount\",\"type\":\"uint256\"},{\"internalType\":\"bytes\",\"name\":\"_data\",\"type\":\"bytes\"}],\"name\":\"safeTransferFrom\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"_operator\",\"type\":\"address\"},{\"internalType\":\"bool\",\"name\":\"_approved\",\"type\":\"bool\"}],\"name\":\"setApprovalForAll\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"string\",\"name\":\"newURI\",\"type\":\"string\"}],\"name\":\"setBaseMetadataURI\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"components\":[{\"internalType\":\"uint16\",\"name\":\"element\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"hp\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"speed\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"intelligence\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"defense\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"attack\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"specialAttack\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"specialDefense\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"level\",\"type\":\"uint16\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"description\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"ipfsAddr\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"rarity\",\"type\":\"string\"}],\"internalType\":\"struct PepemonStats.BattleCardStats\",\"name\":\"x\",\"type\":\"tuple\"}],\"name\":\"setBattleCardStats\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"string\",\"name\":\"newURI\",\"type\":\"string\"}],\"name\":\"setContractURI\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint16\",\"name\":\"element\",\"type\":\"uint16\"},{\"internalType\":\"string\",\"name\":\"x\",\"type\":\"string\"}],\"name\":\"setElementDecode\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"id\",\"type\":\"uint256\"},{\"components\":[{\"internalType\":\"bytes32\",\"name\":\"currentRoundChanges\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"nextRoundChanges\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"specialCode\",\"type\":\"uint256\"},{\"internalType\":\"uint16\",\"name\":\"modifierNumberOfNextTurns\",\"type\":\"uint16\"},{\"internalType\":\"bool\",\"name\":\"isOffense\",\"type\":\"bool\"},{\"internalType\":\"bool\",\"name\":\"isNormal\",\"type\":\"bool\"},{\"internalType\":\"bool\",\"name\":\"isStackable\",\"type\":\"bool\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"description\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"ipfsAddr\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"rarity\",\"type\":\"string\"}],\"internalType\":\"struct PepemonStats.SupportCardStats\",\"name\":\"x\",\"type\":\"tuple\"}],\"name\":\"setSupportCardStats\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint16\",\"name\":\"element\",\"type\":\"uint16\"},{\"components\":[{\"internalType\":\"uint16\",\"name\":\"weakness\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"resistance\",\"type\":\"uint16\"}],\"internalType\":\"struct PepemonStats.elementWR\",\"name\":\"x\",\"type\":\"tuple\"}],\"name\":\"setWeakResist\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"supportCardStats\",\"outputs\":[{\"internalType\":\"bytes32\",\"name\":\"currentRoundChanges\",\"type\":\"bytes32\"},{\"internalType\":\"bytes32\",\"name\":\"nextRoundChanges\",\"type\":\"bytes32\"},{\"internalType\":\"uint256\",\"name\":\"specialCode\",\"type\":\"uint256\"},{\"internalType\":\"uint16\",\"name\":\"modifierNumberOfNextTurns\",\"type\":\"uint16\"},{\"internalType\":\"bool\",\"name\":\"isOffense\",\"type\":\"bool\"},{\"internalType\":\"bool\",\"name\":\"isNormal\",\"type\":\"bool\"},{\"internalType\":\"bool\",\"name\":\"isStackable\",\"type\":\"bool\"},{\"internalType\":\"string\",\"name\":\"name\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"description\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"ipfsAddr\",\"type\":\"string\"},{\"internalType\":\"string\",\"name\":\"rarity\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"bytes4\",\"name\":\"_interfaceID\",\"type\":\"bytes4\"}],\"name\":\"supportsInterface\",\"outputs\":[{\"internalType\":\"bool\",\"name\":\"\",\"type\":\"bool\"}],\"stateMutability\":\"pure\",\"type\":\"function\"},{\"inputs\":[],\"name\":\"symbol\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"tokenMaxSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"name\":\"tokenSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"}],\"name\":\"totalSupply\",\"outputs\":[{\"internalType\":\"uint256\",\"name\":\"\",\"type\":\"uint256\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"address\",\"name\":\"newOwner\",\"type\":\"address\"}],\"name\":\"transferOwnership\",\"outputs\":[],\"stateMutability\":\"nonpayable\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint256\",\"name\":\"_id\",\"type\":\"uint256\"}],\"name\":\"uri\",\"outputs\":[{\"internalType\":\"string\",\"name\":\"\",\"type\":\"string\"}],\"stateMutability\":\"view\",\"type\":\"function\"},{\"inputs\":[{\"internalType\":\"uint16\",\"name\":\"\",\"type\":\"uint16\"}],\"name\":\"weakResist\",\"outputs\":[{\"internalType\":\"uint16\",\"name\":\"weakness\",\"type\":\"uint16\"},{\"internalType\":\"uint16\",\"name\":\"resistance\",\"type\":\"uint16\"}],\"stateMutability\":\"view\",\"type\":\"function\"}]";
    private static Contract contract => ThirdwebManager.Instance.SDK.GetContract(Address, Abi);

    /// <summary>
    /// Gets the specified card's metadata
    /// </summary>
    /// <returns></returns>
    public static async Task<CardMetadata?> GetCardMetadata(ulong tokenId)
    {
        await Task.Delay(1);
        /*var request = new QueryUnityRequest<UriFunction, UriOutputDTO>(
            Web3Controller.instance.GetReadOnlyRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        UriOutputDTO response;
        try
        {
            response = await request.QueryAsync(
                        new UriFunction { Id = tokenId },
                        Address);
        }
        catch (Exception e)
        {
            // Usually tokenId was not found
            Debug.LogException(e);
            return null;
        }

        // example of ReturnValue1:
        // data:application/json;base64\r\n\r\neyJwb29sIjogeyJuYW1lIjogInJvb3QiLCJwb2ludHMiOiAxfSwiZXh0ZXJuYWxfdXJsIjogImh0dHBzOi8vcGVwZW1vbi53b3JsZC8iLCJpbWFnZSI6ICJodHRwczovL2JhZnliZWljNmJkbnRoanA0djU0c3JtN3JvbHp0ZGRqaDRzb2dxajN1Y3V6eXVha3J1dHNqdjY3b21tLmlwZnMuZHdlYi5saW5rL2JmYWZueWNhcmQucG5nIiwibmFtZSI6ICJGYWZueSIsImRlc2NyaXB0aW9uIjogIkZhZm55IChCYXR0bGUgdmVyLikiLCJhdHRyaWJ1dGVzIjpbeyJ0cmFpdF90eXBlIjoiU2V0IiwidmFsdWUiOiJQZXBlbW9uIEJhdHRsZSJ9LHsidHJhaXRfdHlwZSI6IkxldmVsIiwidmFsdWUiOjF9LHsidHJhaXRfdHlwZSI6IkVsZW1lbnQiLCJ2YWx1ZSI6IkZpcmUifSx7InRyYWl0X3R5cGUiOiJXZWFrbmVzcyIsInZhbHVlIjoiV2F0ZXIifSx7InRyYWl0X3R5cGUiOiJSZXNpc3RhbmNlIiwidmFsdWUiOiJHcmFzcyJ9LHsidHJhaXRfdHlwZSI6IkhQIiwidmFsdWUiOjQwMH0seyJ0cmFpdF90eXBlIjoiU3BlZWQiLCJ2YWx1ZSI6NX0seyJ0cmFpdF90eXBlIjoiSW50ZWxsaWdlbmNlIiwidmFsdWUiOjZ9LHsidHJhaXRfdHlwZSI6IkRlZmVuc2UiLCJ2YWx1ZSI6MTJ9LHsidHJhaXRfdHlwZSI6IkF0dGFjayIsInZhbHVlIjo1fSx7InRyYWl0X3R5cGUiOiJTcGVjaWFsIEF0dGFjayIsInZhbHVlIjoyMH0seyJ0cmFpdF90eXBlIjoiU3BlY2lhbCBEZWZlbnNlIiwidmFsdWUiOjEyfV19

        var regexGroups = Regex.Match(response.ReturnValue1, "^data:application/json;base64[^\\w]+(.+)").Groups;
        if (regexGroups.Count > 1)
        {
            var decoded = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(regexGroups[1].Value));
            return JsonUtility.FromJson<CardMetadata>(decoded);
        }
        Debug.LogWarning("Unable to parse metadata of tokenId " + tokenId);*/
        return null;
    }

    /// <summary>
    /// Returns a list of owned cards by checking the owner of a list of card IDs.
    /// </summary>
    /// <param name="address">User address</param>
    /// <param name="tokenIds">Card IDs to be checked</param>
    /// <returns>List of IDs of owned cards</returns>
    public static async Task<Dictionary<ulong, int>> GetOwnedCards(string address, List<ulong> tokenIds)
    {
        /*var request = new QueryUnityRequest<BalanceOfBatchFunction, BalanceOfBatchOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        // using NFTsOfUserUnityRequest fails because PepemonFactory uses ERC1155 but NFTsOfUserUnityRequest is for ERC721
        var response = await request.QueryAsync(
            new BalanceOfBatchFunction
            {
                Ids = tokenIds,
                Owners = Enumerable.Repeat(address, tokenIds.Count).ToList()
            },
            Address);

        var ownedCards = new Dictionary<ulong, int>();
        for (int i = 0; i < (response.ReturnValue1?.Count ?? 0); i++)
            if (response.ReturnValue1[i] > 0)
                ownedCards[tokenIds[i]] = (int)response.ReturnValue1[i];

        return ownedCards;*/

        await Task.Delay(1);
        return new Dictionary<ulong, int>();
    }

    public static async Task<BigInteger> GetTokenSupply(ulong tokenId)
    {
        /*var request = new QueryUnityRequest<TotalSupplyFunction, TotalSupplyOutputDTO>(
            Web3Controller.instance.GetUnityRpcRequestClientFactory(),
            Web3Controller.instance.SelectedAccountAddress);

        var response = await request.QueryAsync(new TotalSupplyFunction { Id = tokenId }, Address);
        return response.ReturnValue1;*/

        await Task.Delay(1);
        return BigInteger.One;
    }

    public static async Task<ulong> FindMaxTokenId(ulong parallelBatchSize = 5)
    {
        //ulong batch = 0;
        ulong maxTokenId = 0;
        //ulong batchTokenMaxId = 0;
        /*
        do
        {
            batchTokenMaxId = 0;
            List<Task<ulong>> tasks = new List<Task<ulong>>();
            Debug.Log($"Checking supply of tokens {batch * parallelBatchSize}...{(batch + 1) * parallelBatchSize - 1}");
            for (ulong i = 0; i < parallelBatchSize; i++)
            {
                ulong tokenId = batch * parallelBatchSize + i;
                tasks.Add(GetTokenSupply(tokenId).ContinueWith(supply => supply.Result > 0 ? tokenId : 0));
            }
            await Task.WhenAll(tasks);

            batchTokenMaxId = tasks.Select(t => t.Result).Max();
            maxTokenId = Math.Max(maxTokenId, batchTokenMaxId);
            batch++;
        } while (batchTokenMaxId > 0);
        */

        await Task.Delay(1);
        return maxTokenId;
    }

    /// <summary>
    /// Tells whether or not a contract/wallet can transfer NFTs
    /// </summary>
    /// <param name="operatorAddress">Address of the contract/wallet</param>
    /// <returns>result of IsApprovedForAll</returns>
    public static async Task<bool> GetApprovalState(string operatorAddress)
    {
        return await contract.ERC1155.IsApprovedForAll(Address, operatorAddress);
    }

    /// <summary>
    /// Approval is necessary to prevent this error in some cases: ERC1155#safeTransferFrom: INVALID_OPERATOR
    /// </summary>
    /// <param name="approved">Approval state to allow moving cards</param>
    /// <param name="operatorAddress">Contract/wallet which will be given/revoked permission to transfer the NFTs</param>
    /// <returns>Transaction hash</returns>
    public static async Task SetApprovalState(bool approved, string operatorAddress)
    {
        await contract.ERC1155.SetApprovalForAll(operatorAddress, approved);
    }

    [Serializable]
    public struct CardMetadata
    {
        public string external_url;
        public string image;
        public string name;
        public string description;
        public CardAttribute[] attributes;
    }

    [Serializable]
    public struct CardAttribute
    {
        public string trait_type;
        public string value;
    }
}
