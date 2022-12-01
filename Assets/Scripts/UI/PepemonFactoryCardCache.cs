using NBitcoin;
using Nethereum.Unity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

/// <summary>
/// Downloads and caches PepemonFactory card information, including metadata 
/// and textures
/// </summary>
class PepemonFactoryCardCache
{
    private static ConcurrentDictionary<ulong, Texture2D> cardTextures = new ConcurrentDictionary<ulong, Texture2D>();
    private static ConcurrentDictionary<ulong, PepemonFactory.CardMetadata> cardMetadata = new ConcurrentDictionary<ulong, PepemonFactory.CardMetadata>();
    public static List<ulong> CardsIds { get => cardMetadata.Keys.ToList(); }

    public async Task PreloadAll(ulong parallelBatchSize = 5)
    {
        Debug.Log($"Loading support and battle cards...");

        ulong batchStart = 1;
        ulong batchEnd;
        ulong batchLoadedCardsCount;
        ulong batchRequestedCardsCount;

        do
        {
            batchEnd = batchStart + parallelBatchSize;
            batchLoadedCardsCount = 0;
            batchRequestedCardsCount = 0;

            List<Task> batchLoadingTasks = new List<Task>();
            for (ulong i = batchStart; i < batchEnd; i++)
            {
                ulong tokenId = i;
                if (!cardMetadata.ContainsKey(tokenId))
                {
                    batchLoadingTasks.Add(PreloadToken(tokenId));
                    batchRequestedCardsCount++;
                }
            }
            await Task.WhenAll(batchLoadingTasks);

            for (ulong i = batchStart; i < batchEnd; i++)
            {
                if (cardMetadata.ContainsKey(i))
                {
                    batchLoadedCardsCount++;
                }
            }

            batchStart += parallelBatchSize;
        } while (batchLoadedCardsCount >= batchRequestedCardsCount);

        Debug.Log($"Finished loading {CardsIds.Count} cards.");
    }

    private async Task PreloadToken(ulong tokenId)
    {
        await PreloadTokenMetadata(tokenId);
        await PreloadTokenImage(tokenId);
    }

    private async Task PreloadTokenMetadata(ulong tokenId)
    {
        PepemonFactory.CardMetadata? metadata = await PepemonFactory.GetCardMetadata(tokenId);
        if (metadata != null)
        {
            cardMetadata[tokenId] = (PepemonFactory.CardMetadata)metadata;
        }
        Debug.Log($"Loaded token metadata for id: {tokenId}");
    }

    private async Task PreloadTokenImage(ulong tokenId)
    {
        if (!cardMetadata.ContainsKey(tokenId))
        {
            return;
        }

        PepemonFactory.CardMetadata metadata = cardMetadata[tokenId];
        string url = IpfsUrlService.ResolveIpfsUrlGateway(metadata.image);
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
        {

            DownloadHandler handle = webRequest.downloadHandler;
            try
            {
                await webRequest.SendWebRequest();
            }
            catch (UnityWebRequestException e)
            {
                Debug.LogWarning($"Error while downloading card image {tokenId}: {e.Message}");
                cardTextures[tokenId] = new Texture2D(8, 8);
                return;
            }
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogWarning($"Error while downloading card image {tokenId}: {webRequest.error}");
                    cardTextures[tokenId] = new Texture2D(8, 8);
                    return;
            }
            Debug.Log("Download of card image successful. Card id: " + tokenId);
            cardTextures[tokenId] = DownloadHandlerTexture.GetContent(webRequest);
        }
    }

    public static Texture2D GetImage(ulong tokenId)
    {
        return cardTextures.TryGet(tokenId);
    }

    public static PepemonFactory.CardMetadata? GetMetadata(ulong tokenId)
    {
        return cardMetadata.TryGet(tokenId);
    }
}
