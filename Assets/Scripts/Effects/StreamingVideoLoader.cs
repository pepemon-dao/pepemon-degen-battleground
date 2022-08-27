using System;
using System.Collections;
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
    public float onVideoFinishedDelay = 2f;

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

            player.Prepare();
            if (autoStart)
            {
                player.Play();
            }
        }
    }

    private IEnumerator ForceCompleteCoroutine()
    {
        if (TryGetComponent(out VideoPlayer player))
        {
            yield return new WaitForSeconds((float)player.length);
        }
        OnVideoFinished(null);
    }

    private IEnumerator DelayedCompletionCoroutine()
    {
        yield return new WaitForSeconds(onVideoFinishedDelay);
        onVideoFinished?.Invoke();
    }

    private void OnVideoStarted(VideoPlayer _)
    {
        onVideoStarted?.Invoke();
        StartCoroutine(ForceCompleteCoroutine());
    }

    private void OnVideoFinished(VideoPlayer _)
    {
        if (!finished)
        {
            finished = true;
            StartCoroutine(DelayedCompletionCoroutine());
        }
    }

    private void OnVideoErrored(VideoPlayer _, string errorMessage)
    {
        Debug.Log("VideoPlayer encountered an error, skipping: " + errorMessage);
        finished = true;
        onVideoFinished?.Invoke();
    }
}
