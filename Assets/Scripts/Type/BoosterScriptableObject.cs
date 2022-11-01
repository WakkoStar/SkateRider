using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BoosterProps", menuName = "ScriptableObjects/BoosterScriptableObject", order = 1)]
public class BoosterScriptableObject : ScriptableObject
{
    public Material boosterMaterial;
    public int texStripLength;
    public float boostAmount;
}

