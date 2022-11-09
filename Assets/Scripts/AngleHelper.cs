using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AngleHelper
{
    public static float FormatAngle(float angleToFormat)
    {
        return angleToFormat > 180 ? angleToFormat - 360 : angleToFormat;
    }
}
