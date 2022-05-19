using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class VideoFinishedEvent : MonoBehaviour
{
    public UnityEvent videoFinished;

    // Start is called before the first frame update
    void Start()
    {
        if (TryGetComponent<VideoPlayer>(out VideoPlayer player))
        {
            player.loopPointReached += Finished;
        }
    }

    void Finished(VideoPlayer source)
    {
        videoFinished.Invoke();
    }
}
