using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DifficultyIncreaser : MonoBehaviour
{
    [SerializeField] private float totalTime; //in secs
    public delegate void DelValueModifier(object value);

    public struct Increaser<T>
    {
        public T startValue;
        public T endValue;
        public DelValueModifier SetValue;
    }

    [HideInInspector] public List<Increaser<object>> increasers = new List<Increaser<object>>();
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(IncreaseDifficulty());
    }

    public void AddIncreaser<T>(T startValue, T endValue, DelValueModifier callback)
    {
        var increaser = new Increaser<object>();
        increaser.startValue = startValue;
        increaser.endValue = endValue;
        increaser.SetValue = callback;

        increaser.SetValue(startValue);

        increasers.Add(increaser);
    }

    private IEnumerator IncreaseDifficulty()
    {
        yield return new WaitForSeconds(1f); //Wait till all increasers are initialized

        for (double a = 0; a < 1; a += Time.deltaTime / totalTime)
        {
            foreach (var increaser in increasers)
            {
                if (increaser.startValue.GetType() == typeof(Vector3))
                {
                    increaser.SetValue(Vector3.Lerp((Vector3)increaser.startValue, (Vector3)increaser.endValue, (float)a));
                }

                if (increaser.startValue.GetType() == typeof(float))
                {
                    increaser.SetValue(Mathf.Lerp((float)increaser.startValue, (float)increaser.endValue, (float)a));
                }
            }

            yield return null;
        }
    }
}
