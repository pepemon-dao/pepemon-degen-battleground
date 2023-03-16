using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerDelay : MonoBehaviour
{
    public int delaySeconds = 5;
    public bool triggerOnEnable = true;
    public bool triggerOnStart = false;
    public UnityEvent action;

    void OnEnable()
    {
        if (triggerOnEnable)
        {
            StartTimer();
        }
    }
    void Start()
    {
        if (triggerOnStart)
        {
            StartTimer();
        }
    }

    public void StartTimer()
    {
        StartCoroutine(Coroutine());
    }

    private IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(delaySeconds);
        action?.Invoke();
    }
}
