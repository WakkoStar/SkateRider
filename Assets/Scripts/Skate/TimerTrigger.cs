using System;
using System.Collections.Generic;
using UnityEngine;

public class TimerTrigger : MonoBehaviour
{
    public float triggerTime;
    public Action onTimeTriggered;
    public Action onResponse;

    private float _time = 0;
    private bool _isActionTriggered = false;


    private void Update()
    {
        if (_time > triggerTime)
        {
            onTimeTriggered();
            _isActionTriggered = true;
        }
        if (_isActionTriggered && _time < triggerTime)
        {
            onResponse();
            _isActionTriggered = false;
        }
    }


    public void IncrementTime(float value)
    {
        _time += value;
    }

    public void ResetTime()
    {
        _time = 0;
    }


}
