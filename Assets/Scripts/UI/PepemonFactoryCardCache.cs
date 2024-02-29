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
using UnityEngine.Events;

/// <summary>
/// Downloads and caches PepemonFactory card information, including metadata 
/// and textures
/// </summary>
class PepemonFactoryCardCache
{
    private static ConcurrentDictionary<ulong, Texture2D> cardTextures = new ConcurrentDictionary<ulong, Texture2D>();
    private static ConcurrentDictionary<ulong, PepemonFactory.CardMetadata> cardMetadata = new ConcurrentDictionary<ulong, PepemonFactory.CardMetadata>();
    public static List<ulong> CardsIds { get => cardMetadata.Keys.ToList(); }

    public UnityEvent<ulong> cardMetadataLoaded;
    public UnityEvent<ulong> cardImageLoaded;

    public static async Task PreloadAllMetadata(ulong parallelBatchSize, Action<ulong> cardLoadedCallback = null)
    {
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
                    batchLoadingTasks.Add(PreloadTokenMetadata(tokenId, cardLoadedCallback));
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
    }

    public static async Task PreloadAllImages(ulong parallelBatchSize, Action<ulong> cardLoadedCallback = null)
    {
        var cardList = cardMetadata.ToList();

        for (int batchStart = 0; batchStart < cardList.Count; batchStart += (int)parallelBatchSize)
        {
            int batchEnd = Math.Min(batchStart + (int)parallelBatchSize, cardList.Count);

            List<Task> imageLoadingTasks = new List<Task>();
            for (int i = batchStart; i < batchEnd; i++)
            {
                imageLoadingTasks.Add(PreloadTokenImage(cardList[i].Key, cardLoadedCallback));
            }
            await Task.WhenAll(imageLoadingTasks);
        }
    }

    private static async Task PreloadTokenMetadata(ulong tokenId, Action<ulong> cardLoadedCallback = null)
    {
        PepemonFactory.CardMetadata? metadata = await PepemonFactory.GetCardMetadata(tokenId);
        if (metadata != null)
        {
            cardMetadata[tokenId] = (PepemonFactory.CardMetadata)metadata;
        }
        Debug.Log($"Loaded token metadata for id: {tokenId}");
        cardLoadedCallback?.Invoke(tokenId);
    }

    private static async Task PreloadTokenImage(ulong tokenId, Action<ulong> cardLoadedCallback = null)
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
                cardLoadedCallback?.Invoke(tokenId);
                return;
            }
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.DataProcessingError:
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogWarning($"Error while downloading card image {tokenId}: {webRequest.error}");
                    cardTextures[tokenId] = new Texture2D(8, 8);
                    cardLoadedCallback?.Invoke(tokenId);
                    return;
            }
            Debug.Log("Download of card image successful. Card id: " + tokenId);
            cardTextures[tokenId] = DownloadHandlerTexture.GetContent(webRequest);
            cardTextures[tokenId] = GenerateMipmaps(cardTextures[tokenId]);
            cardLoadedCallback?.Invoke(tokenId);
        }
    }

    private static Texture2D GenerateMipmaps(Texture2D source)
    {
        Texture2D output = new Texture2D(source.width, source.height, source.format, true);
        output.SetPixelData(source.GetRawTextureData<byte>(), 0);
        output.Apply(true, true);
        return output;
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
