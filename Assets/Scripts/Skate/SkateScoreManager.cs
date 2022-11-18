using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateScoreManager : MonoBehaviour
{
    public SkateStateManager skateState;
    private int _trickScore;
    private int _grindScore;
    private float _totalScore;
    private bool _shouldStopScore;

    private void Start()
    {

    }

    void FixedUpdate()
    {
        if (!ShouldStopScore()) IncrementScore(1);
        skateState.GetSkateMainScreen().DisplayScore(GetTotalScore());

        if (GetGrindScore() > 0)
        {
            skateState.GetSkateMainScreen().DisplayJumpScore(GetGrindScore());
        }
        else if (GetTrickScore() > 0)
        {
            skateState.GetSkateMainScreen().DisplayJumpScore(GetTrickScore());
        }
        else
        {
            skateState.GetSkateMainScreen().HideJumpScore();
        }
    }

    public void SetTrickScore(int value)
    {
        _trickScore = value;
    }
    public int GetTrickScore()
    {
        return _trickScore;
    }



    public void SetGrindScore(int value)
    {
        _grindScore = value;
    }
    public int GetGrindScore()
    {
        return _grindScore;
    }



    private void IncrementScore(float value)
    {
        _totalScore += value;
    }
    private float GetTotalScore()
    {
        return _totalScore;
    }



    public void AddAndResetTrickScore()
    {
        if (GetTrickScore() > 5) IncrementScore(GetTrickScore());
        SetTrickScore(0);
    }
    public void AddAndResetGrindScore()
    {
        if (GetGrindScore() > 5) IncrementScore(GetGrindScore());
        SetGrindScore(0);
    }


    public void SetShouldStopScore(bool value)
    {
        _shouldStopScore = value;
    }
    private bool ShouldStopScore()
    {
        return _shouldStopScore;
    }



    public void SetInit()
    {
        SetShouldStopScore(true);
    }
    public void SetStartGame()
    {
        SetShouldStopScore(false);
    }
    public void SetGameOver()
    {
        SetShouldStopScore(true);
        SetTrickScore(0);
        SetGrindScore(0);
    }
    public void SetRestartGame()
    {
        SetShouldStopScore(false);
    }


}
