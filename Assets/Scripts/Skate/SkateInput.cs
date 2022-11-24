using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class SkateInput : MonoBehaviour
{
    //SETTINGS
    public GameObject NoseZone;
    public GameObject TailZone;
    public Vector2 noseZoneUIOffset = new Vector2(-72f, -30f);
    public Vector2 tailZoneUIOffset;
    //STATE
    private CanvasGroup _noseZoneCanvas;
    private CanvasGroup _tailZoneCanvas;

    private bool _isTailTouch;
    private bool _isNoseTouch;
    private Touch _touchOne;
    private Touch _touchTwo;
    private bool _isTailTouchWithTouchOne;
    private bool _isTailTouchWithTouchTwo;
    private bool _isNoseTouchWithTouchOne;
    private bool _isNoseTouchWithTouchTwo;
    private Vector2 _noseOffset;
    private Vector2 _tailOffset;
    private Vector2 _tailPosition;
    private Vector2 _nosePosition;
    private bool _canResetTailOffset;
    private bool _canResetNoseOffset;
    private float _timeOnNose;
    private float _timeOnTail;
    private bool _shouldTakeTimeOnNose;
    private bool _shouldTakeTimeOnTail;
    private bool _isOllie;
    private bool _isNollie;

    private float _scaleCanvasX;
    private float _scaleCanvasY;

    void Start()
    {
        var canvasRect = GameObject.Find("Canvas").GetComponent<RectTransform>();
        _scaleCanvasX = canvasRect.rect.width / Screen.width;
        _scaleCanvasY = canvasRect.rect.height / Screen.height;

        _noseZoneCanvas = NoseZone.GetComponent<CanvasGroup>();
        _tailZoneCanvas = TailZone.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (Input.touchCount == 0)
        {
            _isTailTouch = false;
            _isNoseTouch = false;
        }

        if (Input.touchCount >= 1)
        {
            _touchOne = Input.GetTouch(0);
            _touchTwo = Input.GetTouch(0);

            var isTailNearest =
            (_touchOne.position - _tailPosition).sqrMagnitude < (_touchOne.position - _nosePosition).sqrMagnitude;

            _isTailTouchWithTouchOne = isTailNearest;
            _isNoseTouchWithTouchOne = !isTailNearest;

            _isTailTouch = _isTailTouchWithTouchOne;
            _isNoseTouch = _isNoseTouchWithTouchOne;
        }

        if (Input.touchCount >= 2)
        {
            _touchOne = Input.GetTouch(0);
            _touchTwo = Input.GetTouch(1);

            var isTouchOneOnLeft = _touchOne.position.x < _touchTwo.position.x;
            var isTouchTwoOnLeft = _touchTwo.position.x < _touchOne.position.x;

            _isTailTouchWithTouchOne = isTouchOneOnLeft;//IsPointerOverUITarget(GetEventSystemRaycastResults(_touchOne), TailZone);
            _isTailTouchWithTouchTwo = isTouchTwoOnLeft;//IsPointerOverUITarget(GetEventSystemRaycastResults(_touchTwo), TailZone);

            _isNoseTouchWithTouchOne = isTouchTwoOnLeft;//IsPointerOverUITarget(GetEventSystemRaycastResults(_touchOne), NoseZone);
            _isNoseTouchWithTouchTwo = isTouchOneOnLeft;//IsPointerOverUITarget(GetEventSystemRaycastResults(_touchTwo), NoseZone);

            if (_isTailTouchWithTouchOne)
            {
                _tailPosition = _touchOne.position;
                _nosePosition = _touchTwo.position;
            }

            if (_isTailTouchWithTouchTwo)
            {
                _tailPosition = _touchTwo.position;
                _nosePosition = _touchOne.position;
            }

            _isTailTouch =
                _isTailTouchWithTouchOne
                || _isTailTouchWithTouchTwo;

            _isNoseTouch =
                _isNoseTouchWithTouchOne
                || _isNoseTouchWithTouchTwo;
        }

        //HANDLE UI BEHAVIOR
        if (_isTailTouchWithTouchOne)
        {
            // Vector2 anchoredPos;
            // RectTransformUtility.ScreenPointToLocalPointInRectangle(
            //     NoseZone.GetComponentInParent<RectTransform>(), 
            //     _touchOne.position, 
            //     Camera.main.gameObject, 
            //     anchoredPos
            // )
            TailZone.transform.localPosition = ScaleToCanvas(_touchOne.position + tailZoneUIOffset);
        }
        if (_isTailTouchWithTouchTwo)
        {
            TailZone.transform.localPosition = ScaleToCanvas(_touchTwo.position + tailZoneUIOffset);
        }
        if (_isNoseTouchWithTouchOne)
        {
            NoseZone.transform.localPosition = ScaleToCanvas(_touchOne.position + noseZoneUIOffset);
        }
        if (_isNoseTouchWithTouchTwo)
        {
            NoseZone.transform.localPosition = ScaleToCanvas(_touchTwo.position + noseZoneUIOffset);
        }

        _tailZoneCanvas.alpha = _isTailTouch ? 1 : 0;
        _noseZoneCanvas.alpha = _isNoseTouch ? 1 : 0;

        //RESET TOUCH OFFSET
        if (!_isTailTouch)
        {
            _canResetTailOffset = true;
        }

        if (_isTailTouch && _canResetTailOffset)
        {
            _tailOffset = Vector2.zero;
            _canResetTailOffset = false;
        }

        if (!_isNoseTouch)
        {
            _canResetNoseOffset = true;
        }

        if (_isNoseTouch && _canResetNoseOffset)
        {
            _noseOffset = Vector2.zero;
            _canResetNoseOffset = false;
        }

        //GET TOUCH OFFSET
        if (_isTailTouchWithTouchOne)
        {
            _tailOffset += _touchOne.deltaPosition;
        }
        else if (_isTailTouchWithTouchTwo)
        {
            _tailOffset += _touchTwo.deltaPosition;
        }

        if (_isNoseTouchWithTouchOne)
        {
            _noseOffset += _touchOne.deltaPosition;
        }
        else if (_isNoseTouchWithTouchTwo)
        {
            _noseOffset += _touchTwo.deltaPosition;
        }

        //IS OLLIE OR NOLLIE
        if (!_isNoseTouch && _shouldTakeTimeOnNose)
        {
            _timeOnNose = Time.time;
            _shouldTakeTimeOnNose = false;
        }
        if (!_isTailTouch && _shouldTakeTimeOnTail)
        {
            _timeOnTail = Time.time;
            _shouldTakeTimeOnTail = false;
        }

        _isOllie = _timeOnTail >= _timeOnNose;
        _isNollie = _timeOnNose > _timeOnTail;

        if (_isNoseTouch) _shouldTakeTimeOnNose = true;
        if (_isTailTouch) _shouldTakeTimeOnTail = true;
    }

    public bool GetIsNoseTouch()
    {
        return _isNoseTouch;
    }

    public bool GetIsTailTouch()
    {
        return _isTailTouch;
    }

    public Vector2 GetNoseOffset()
    {
        return _noseOffset;
    }

    public Vector2 GetTailOffset()
    {
        return _tailOffset;
    }

    public void ResetNoseOffset()
    {
        _noseOffset = Vector2.zero;
    }

    public void ResetTailOffset()
    {
        _tailOffset = Vector2.zero;
    }

    public bool GetIsOllie()
    {
        return _isOllie;
    }
    public bool GetIsNollie()
    {
        return _isNollie;
    }

    private Vector2 ScaleToCanvas(Vector2 position)
    {
        return new Vector2(position.x * _scaleCanvasX, position.y * _scaleCanvasY);
    }

}
