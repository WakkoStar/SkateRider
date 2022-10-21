using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchors
{
    public static Vector3 GetGrindAnchors(Vector3 tEulerAng)
    {
        return new Vector3(
            Anchors.GetNearestAnchor(tEulerAng.x),
            Anchors.GetFullNearestAnchor(tEulerAng.y),
            Anchors.GetSimpleNearestAnchor(tEulerAng.z)
        );
    }

    public static Vector3 GetGroundAnchors(Vector3 tEulerAng)
    {
        return new Vector3(
            Anchors.GetNearestAnchor(tEulerAng.x),
            Anchors.GetNearestAnchor(tEulerAng.y),
            tEulerAng.z
        );
    }

    public static Vector3 GetAirAnchors(Vector3 tEulerAng)
    {
        return new Vector3(
            Anchors.GetNearestAnchor(tEulerAng.x),
            Anchors.GetNearestAnchor(tEulerAng.y),
            Anchors.GetNearestAnchor(tEulerAng.z)
        );
    }

    public static float GetNearestAnchor(float angle)
    {
        if (angle >= 0 && angle < 100) return 0;
        if (angle >= 100 && angle < 180) return 180;
        if (angle >= 180 && angle < 260) return 180;
        if (angle >= 260 && angle < 360) return 360;

        return angle;
    }

    public static float GetFullNearestAnchor(float angle)
    {
        if (angle >= 0 && angle < 45) return 0;
        if (angle >= 45 && angle < 90) return 90;
        if (angle >= 90 && angle < 135) return 90;
        if (angle >= 135 && angle < 180) return 180;
        if (angle >= 180 && angle < 225) return 180;
        if (angle >= 225 && angle < 270) return 270;
        if (angle >= 270 && angle < 315) return 270;
        if (angle >= 315 && angle < 360) return 360;

        return angle;
    }

    public static float GetSimpleNearestAnchor(float angle)
    {
        if (angle >= 0 && angle < 15) return 0;
        if (angle >= 15 && angle < 95) return 30;
        if (angle >= 95 && angle < 265) return 180;
        if (angle >= 265 && angle < 345) return 330;
        if (angle >= 345 && angle < 360) return 360;

        return angle;
    }
}
