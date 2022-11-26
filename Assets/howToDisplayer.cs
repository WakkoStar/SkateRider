using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class howToDisplayer : MonoBehaviour
{
    [SerializeField] private SkateStartScreen skateStartScreen;
    [SerializeField] private List<VideoClip> videoClips;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Text title;

    [SerializeField] private ScreenManager canvasManager;
    private int _currentIndex = 0;

    private void CheckForNewPlayer()
    {
        if (PlayerPrefs.GetInt("isNewPlayer", 1) == 1)
        {
            skateStartScreen.GetComponent<Animator>().SetTrigger("DisplayHowTo");
            PlayerPrefs.SetInt("isNewPlayer", 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Awake()
    {
        CheckForNewPlayer();
    }

    void Start()
    {
        _currentIndex = 0;

        SetVideoClip(_currentIndex);

        canvasManager.Hide("Done");
        canvasManager.Hide("Back");
        canvasManager.Display("Next");
    }

    public void GoToHowTo(bool isNext)
    {
        _currentIndex += isNext ? 1 : -1;

        SetVideoClip(_currentIndex);

        if (_currentIndex == 0)
        {
            canvasManager.Hide("Back");
        }
        else
        {
            canvasManager.Display("Back");
        }

        if (_currentIndex == videoClips.Count - 1)
        {
            canvasManager.Display("Done");
            canvasManager.Hide("Next");
        }
        else
        {
            canvasManager.Display("Next");
            canvasManager.Hide("Done");
        }

    }


    void SetVideoClip(int index)
    {
        videoPlayer.clip = videoClips[index];
        videoPlayer.Play();

        title.text = videoClips[index].name.ToUpper();
    }
}
