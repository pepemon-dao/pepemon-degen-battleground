using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class StreamingVideoLoader : MonoBehaviour
{
    public string url;
    public bool autoStart;
    public bool isStreamingAsset;
    public UnityEvent onVideoStarted;
    public UnityEvent onVideoFinished;

    private bool finished = false;

    void Start()
    {
        if (TryGetComponent(out VideoPlayer player))
        {
            player.started += OnVideoStarted;
            player.errorReceived += OnVideoErrored;
            player.loopPointReached += OnVideoFinished;

            if (!string.IsNullOrWhiteSpace(url))
            {
                player.url = isStreamingAsset ? Path.Combine(Application.streamingAssetsPath, url) : url;
            }
            if (autoStart)
            {
                onVideoStarted?.Invoke();
                player.Play();
            }
        }
    }

    private async void OnVideoStarted(VideoPlayer source)
    {
        if (!autoStart)
        {
            onVideoStarted?.Invoke();
        }
        // Skip the video if it wasn't able to play in time
        await Task.Delay((int)(source.length * 1000));
        OnVideoFinished(source);
    }

    private void OnVideoFinished(VideoPlayer _)
    {
        if (!finished)
        {
            onVideoFinished?.Invoke();
            finished = true;
        }
    }

    private void OnVideoErrored(VideoPlayer _, string errorMessage)
    {
        Debug.Log("VideoPlayer encountered an error, skipping: " + errorMessage);
        onVideoFinished?.Invoke();
    }
}
