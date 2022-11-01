using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIController : MonoBehaviour
{

    [SerializeField] private GameObject TrickNameDisplayer;
    [SerializeField] private GameObject JumpScoreDisplayer;
    [SerializeField] private GameObject ScoreDisplayer;
    [SerializeField] private GameObject CollectibleDisplayer;
    [SerializeField] private GameObject NollieSign;
    [SerializeField] private GameObject SwitchSign;
    [SerializeField] private AudioManager audioManager;

    private CanvasGroup _trickNameCanvas;
    private Text _trickNameText;
    private CanvasGroup _jumpScoreCanvas;
    private Text _jumpScoreText;
    private Text _scoreText;
    private Text _collectibleText;
    private List<string> _tricksToDisplay = new List<string>();
    private bool _isDisplaying;

    void Start()
    {
        _trickNameText = TrickNameDisplayer.GetComponentInChildren<Text>();
        _trickNameCanvas = TrickNameDisplayer.GetComponent<CanvasGroup>();

        _jumpScoreText = JumpScoreDisplayer.GetComponentInChildren<Text>();
        _jumpScoreCanvas = JumpScoreDisplayer.GetComponent<CanvasGroup>();

        _scoreText = ScoreDisplayer.GetComponentInChildren<Text>();
        _collectibleText = CollectibleDisplayer.GetComponentInChildren<Text>();
    }

    void Update()
    {
        if (_tricksToDisplay.Count > 0 && !_isDisplaying)
        {
            StartCoroutine(DisplayTrick(_tricksToDisplay[0]));
            _tricksToDisplay.RemoveAt(0);
        }
    }

    public void AddTrickToDisplay(string trickName)
    {
        if (trickName == "") return;
        _tricksToDisplay.Add(trickName);
    }

    public void DisplayJumpScore(float jumpScore)
    {
        if (jumpScore > 0)
        {
            ShowJumpScore(jumpScore);
        }
        else
        {
            HideJumpScore();
        }
    }

    public void ShowJumpScore(float jumpScore)
    {
        _jumpScoreCanvas.alpha = 1;
        _jumpScoreText.text = "" + jumpScore;
    }

    public void HideJumpScore()
    {
        _jumpScoreCanvas.alpha = 0;
        _jumpScoreText.text = "";
    }

    public void DisplayScore(float score)
    {
        _scoreText.text = "" + score;
    }

    public void HandleSign(bool isActivate, GameObject ObjToActivate)
    {
        ObjToActivate.SetActive(isActivate);
    }

    public void DisplayCollectibleScore(float collectiblesCount)
    {
        _collectibleText.text = "" + collectiblesCount;
    }

    private IEnumerator DisplayTrick(string trick)
    {
        _isDisplaying = true;
        HandleSign(trick.Contains("Nollie"), NollieSign);
        HandleSign(trick.Contains("Switch"), SwitchSign);

        _trickNameText.text = trick.Replace("Switch ", "").Replace("Nollie ", "");

        var startEulerAng = new Vector3(0, 0, -10);
        var startScale = Vector3.one * 0.90f;
        var endEulerAng = Vector3.zero;
        var endScale = Vector3.one;

        TrickNameDisplayer.transform.eulerAngles = startEulerAng;
        TrickNameDisplayer.transform.localScale = startScale;
        for (float a = 0; a < 1; a += Time.deltaTime * 20)
        {
            TrickNameDisplayer.transform.eulerAngles = Vector3.Lerp(startEulerAng, endEulerAng, a);
            TrickNameDisplayer.transform.localScale = Vector3.Lerp(startScale, endScale, a);
            _trickNameCanvas.alpha = a;
            yield return null;
        }
        TrickNameDisplayer.transform.eulerAngles = endEulerAng;
        TrickNameDisplayer.transform.localScale = endScale;
        _trickNameCanvas.alpha = 1;

        audioManager.Play("trickSong");

        yield return new WaitForSeconds(0.5f);

        for (float a = 1; a > 0; a -= Time.deltaTime * 10)
        {

            _trickNameCanvas.alpha = a;
            yield return null;
        }

        _trickNameCanvas.alpha = 0;
        _isDisplaying = false;
    }
}
