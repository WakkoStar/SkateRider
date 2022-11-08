using UnityEngine;

public class SpeedLineDisplayer : MonoBehaviour
{
    private ParticleSystem _speedLinePS;
    // Start is called before the first frame update
    void Start()
    {
        _speedLinePS = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DisplaySpeedLine()
    {
        _speedLinePS.Play();
    }
}
