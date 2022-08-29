using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SpriteVideoPlayer : MonoBehaviour
{
    public Texture2DArray sourceTexture;
    public Material outputMaterial;

    public bool autoPlay;
    public bool loop;
    public float durationSeconds;
    public UnityEvent onStart;
    public UnityEvent onFinish;

    private bool playing;
    private float startTimestamp = 0;

    void Start()
    {
        if (autoPlay)
        {
            Play();
        }
    }

    public void Play()
    {
        if (!playing)
        {
            playing = true;
            startTimestamp = Time.realtimeSinceStartup;
            onStart?.Invoke();
        }
    }

    void Update()
    {
        if (playing)
        {
            float passedSeconds = Time.realtimeSinceStartup - startTimestamp;
            int currentFrame = (int)(sourceTexture.depth / durationSeconds * passedSeconds) % sourceTexture.depth;
            outputMaterial.SetInteger("_Frame", currentFrame);
            if (!loop && passedSeconds >= durationSeconds)
            {
                playing = false;
                onFinish?.Invoke();
            }
        }
    }
}
