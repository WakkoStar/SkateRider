using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateRotationReader : MonoBehaviour
{
    /**
    Riding switch is when a skateboarder uses the opposite footing from their normal stance
    */
    public bool IsSwitch()
    {
        var isSwitch = transform.localEulerAngles.y > 90 && transform.localEulerAngles.y < 270;
        if (IsUpsideDown()) isSwitch = !isSwitch;

        return isSwitch;
    }

    public bool IsUpsideDown()
    {
        var isUpsideDown =
        (Mathf.Abs(transform.localEulerAngles.x) > 90 && Mathf.Abs(transform.localEulerAngles.x) < 270)
         || (Mathf.Abs(transform.localEulerAngles.z) > 90 && Mathf.Abs(transform.localEulerAngles.z) < 270);

        return isUpsideDown;
    }

    public bool IsPerpendicular()
    {
        var tEulerAng = transform.localEulerAngles;
        var rotOffset = Mathf.Abs(tEulerAng.y) % 360;
        return (rotOffset > 45 && rotOffset < 135) || (rotOffset > 235 && rotOffset < 315);
    }
}
