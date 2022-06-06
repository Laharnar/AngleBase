using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTimed : MonoBehaviour
{
    public string timed = "timed";
    public float rate = 1f;
    float time;
    InteractState state;
    public int skipTimes = 0;
    public bool resetSkipTimesOnStart = false;
    int initialSkipTimes = int.MinValue;

    private void Awake()
    {
		state = GetComponent<InteractState>();
        initialSkipTimes = skipTimes;
    }

    void OnEnable()
    {
        time = Time.time + rate;
        if (resetSkipTimesOnStart)
            skipTimes = initialSkipTimes;
    }

    void Update()
    {
        if (Time.time > time)
        {
            time = Time.time + rate;
            if (skipTimes > 0)
            {
                skipTimes--;
            }
            else
            {
                state.OnTimed(timed);
            }
        }
    }
}
