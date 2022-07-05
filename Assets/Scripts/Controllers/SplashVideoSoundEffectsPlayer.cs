using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashVideoSoundEffectsPlayer : MonoBehaviour
{
    public GameObject splashVideoSoundEffectsPlayer;

    void Start()
    {
        // instantiate prefab using invoke
        Invoke("InstantiatePrefab", 0.75f);

        // invoke again after 1.5s
        Invoke("InstantiatePrefab", 1.45f);

        // invoke again after 2.5s
        Invoke("InstantiatePrefab", 2.15f);

        // destroy
        Destroy(gameObject, 3.0f);        
    }

    private void InstantiatePrefab()
    {
        GameObject splashVideoSoundEffectsPlayerInstance = Instantiate(splashVideoSoundEffectsPlayer, transform.position, transform.rotation);

        // destroy the prefab after a certain amount of time
        Destroy(splashVideoSoundEffectsPlayerInstance, .5f);
    }
}
