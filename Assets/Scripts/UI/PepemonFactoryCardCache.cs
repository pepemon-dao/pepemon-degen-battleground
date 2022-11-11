using NBitcoin;
using Nethereum.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Downloads and caches PepemonFactory card information, including metadata 
/// and textures
/// </summary>
class PepemonFactoryCardCache
{
    // TODO: Implement local cache using Application.persistentDataPath

    private const ulong MAX_TOKEN_ID = ulong.MaxValue;
    private static Dictionary<ulong, Texture2D> cardTextures = new Dictionary<ulong, Texture2D>();
    private static Dictionary<ulong, PepemonFactory.CardMetadata> cardMetadata = new Dictionary<ulong, PepemonFactory.CardMetadata>();
    public static List<ulong> CardsIds { get => cardMetadata.Keys.ToList(); }

    public async Task PreloadAll()
    {
        await PreloadAllMetadata();
        await PreloadAllImages();
    }

    private async Task PreloadAllMetadata()
    {
        // Iterate through card IDs and store the card metadata
        // No way to get the maximum token id, so iteration is
        // complete when we cannot get the data.
        for (ulong i = 1; i < MAX_TOKEN_ID; i++)
        {
            Debug.Log($"Loading metadata of tokenId {i}");

            // returning null does not explode the game in WebGL
            var result = await PepemonFactory.GetCardMetadata(i);
            if (result != null)
                cardMetadata[i] = (PepemonFactory.CardMetadata)result;
            else
            {
                Debug.Log($"Preloading finished at tokenId: {i}");
                break;
            }
        }
    }

    private async Task PreloadAllImages()
    {
        // Start a task for image to download
        // These will all run in parallel
        List<Task<KeyValuePair<ulong, Texture2D>>> textureDownloadTasks = cardMetadata.Select(async entry =>
        {
            string url = IpfsUrlService.ResolveIpfsUrlGateway(entry.Value.image);
            using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url))
            {

                DownloadHandler handle = webRequest.downloadHandler;
                await webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.DataProcessingError:
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogWarning($"Error while downloading card image {entry.Key}: {webRequest.error}");
                        return new KeyValuePair<ulong, Texture2D>(entry.Key, new Texture2D(8, 8));
                }
                Debug.Log("Download of card image successful. Card id: " + entry.Key);
                Texture2D tex = DownloadHandlerTexture.GetContent(webRequest);
                return new KeyValuePair<ulong, Texture2D>(entry.Key, tex);
            }
        }).ToList();

        // Wait for all downloads
        await Task.WhenAll(textureDownloadTasks);

        // Add the textures to the cache
        foreach (var task in textureDownloadTasks)
        {
            cardTextures[task.Result.Key] = task.Result.Value;
        }
    }

    public static Texture2D GetImage(ulong tokenId)
    {
        return cardTextures[tokenId];
    }

    public static PepemonFactory.CardMetadata? GetMetadata(ulong tokenId)
    {
        return cardMetadata.TryGet(tokenId);
    }
}
