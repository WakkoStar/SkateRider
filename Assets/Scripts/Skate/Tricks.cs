using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tricks
{

    public static string GetTrick(bool isOllie, float flipAmount, float rotateAmount, bool isSwitch)
    {
        string trickName = "";

        //ROTATE
        var rotateLoop = Mathf.Abs(rotateAmount) / 360;
        if (rotateLoop >= 0.25)
        {
            trickName += Mathf.Sign(rotateAmount) == 1 ? "Backside " : "Frontside ";
        }
        if (rotateLoop >= 0.25 && rotateLoop < 0.75)
        {
            trickName += "180 ";
        }
        if (rotateLoop >= 0.75 && rotateLoop < 1.25)
        {
            trickName += "360 ";
        }
        if (rotateLoop >= 1.25 && rotateLoop < 1.75)
        {
            trickName += "540 ";
        }
        if (rotateLoop >= 1.75 && rotateLoop < 2.25)
        {
            trickName += "720 ";
        }
        if (rotateLoop >= 2.25 && rotateLoop < 2.75)
        {
            trickName += "900 ";
        }
        if (rotateLoop >= 2.75 && rotateLoop < 3.25)
        {
            trickName += "1080 ";
        }
        if (rotateLoop >= 3.25 && rotateLoop < 3.75)
        {
            trickName += "1260 ";
        }
        if (rotateLoop >= 3.75 && rotateLoop < 4.25)
        {
            trickName += "1440 ";
        }

        //FLIP
        var flipLoop = Mathf.Abs(flipAmount) / 360;
        if (rotateLoop < 0.25)
        {
            if (flipLoop >= 0.75)
            {
                trickName += Mathf.Sign(flipAmount) == 1 ? "Heel flip " : "Kick flip ";
            }
            if (flipLoop >= 1.5 && flipLoop < 2.5)
            {
                trickName = "Double " + trickName;
            }
            if (flipLoop >= 2.5 && flipLoop < 3.5)
            {
                trickName = "Triple " + trickName;
            }
            if (flipLoop >= 3.5 && flipLoop < 3.5)
            {
                trickName = "Quadruple " + trickName;
            }
        }
        else
        {
            if (flipLoop >= 0.5)
            {
                trickName += Mathf.Sign(flipAmount) == 1 ? "Heel flip " : "Flip ";
            }
        }

        //PREFIXES
        if (trickName != "")
        {
            if (!isOllie) trickName = "Nollie " + trickName;
            if (isSwitch) trickName = "Switch " + trickName;
        }
        return trickName;
    }

    public static string GetGrindTrick(float boardAngle, float rotateAmount, bool isSwitch)
    {
        bool isManual = boardAngle > 25 && boardAngle < 35 ? true : false;
        bool isNose = boardAngle > 325 && boardAngle < 335 ? true : false;

        if (isSwitch)
        {
            var tempNose = isNose;
            var tempManual = isManual;

            isManual = tempNose;
            isNose = tempManual;
        }


        if (boardAngle > 150 && boardAngle < 210)
        {
            return "Darkslide";
        }

        var formattedRotateAmount = Mathf.Abs(rotateAmount) % 360;
        if (
            (formattedRotateAmount >= 45 && formattedRotateAmount < 135)
            || (formattedRotateAmount >= 225 && formattedRotateAmount < 315)
        )
        {
            if (isManual) return "Nose Bluntslide";
            if (isNose) return "Bluntslide";
            return "Boardslide";
        }
        else
        {
            if (isManual) return "5-0 Grind";
            if (isNose) return "Nose Grind";
            return "50-50";
        }
    }
}
