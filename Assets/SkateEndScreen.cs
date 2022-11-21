using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SkateEndScreen : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private Text bestScoreText;
    [SerializeField] private Text collectibleCountText;
    [SerializeField] private Text collectibleAllCountText;
    [SerializeField] private CanvasGroup bestScoreCanvas;

    public void DisplayScore(float score)
    {
        scoreText.text = "" + score;
    }

    public void DisplayBestScore(float score)
    {
        bestScoreText.text = "" + score;
    }

    public void DisplayCollectibleCount(int count)
    {
        collectibleCountText.text = "" + count;
    }

    public void DisplayAllCollectibleCount(float count)
    {
        collectibleAllCountText.text = "" + count;
    }

    public void DisplayBestScoreSign(bool shouldDisplay)
    {
        bestScoreCanvas.alpha = shouldDisplay ? 1 : 0;
    }
}
