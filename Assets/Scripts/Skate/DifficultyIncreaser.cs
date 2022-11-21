using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class DifficultyIncreaser : MonoBehaviour
{
    [SerializeField] private float totalTime; //in secs

    [Serializable]
    public class FloatIncreaser
    {
        public float startValue;
        public float endValue;
        public UnityEvent<float> SetFloatIncreaser;
    }

    [Serializable]
    public class Vector3Increaser
    {
        public Vector3 startValue;
        public Vector3 endValue;
        public UnityEvent<Vector3> SetVectorIncreaser;
    }

    public List<FloatIncreaser> floatIncreasers = new List<FloatIncreaser>();
    public List<Vector3Increaser> vectorIncreasers = new List<Vector3Increaser>();
    private Coroutine _difficultyIncreaserCoroutine = null;

    public void StartGame()
    {
        if (_difficultyIncreaserCoroutine != null) StopCoroutine(_difficultyIncreaserCoroutine);
        _difficultyIncreaserCoroutine = StartCoroutine(IncreaseDifficulty());
    }
    public void OnGameOver()
    {
        StopCoroutine(_difficultyIncreaserCoroutine);
    }

    private IEnumerator IncreaseDifficulty()
    {
        for (double a = 0; a < 1; a += Time.deltaTime / totalTime)
        {
            foreach (var increaser in floatIncreasers)
            {
                increaser.SetFloatIncreaser.Invoke(Mathf.Lerp(increaser.startValue, increaser.endValue, (float)a));
            }
            foreach (var increaser in vectorIncreasers)
            {
                increaser.SetVectorIncreaser.Invoke(Vector3.Lerp(increaser.startValue, increaser.endValue, (float)a));
            }

            yield return null;
        }
    }
}
