using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkateTerrainReader : MonoBehaviour
{

    private float _terrainDistance;
    private bool _isGrindOnTerrain;
    private bool _isOnTerrain;
    private Vector3 _terrainInclination;

    void Update()
    {
        GetTerrainInformations();
    }

    private void GetTerrainInformations()
    {
        //GROUND
        RaycastHit hitBack;
        RaycastHit hitFront;
        Vector3 pos = transform.position;
        Vector3 posOffset = new Vector3(transform.right.x, transform.right.y, 0);

        Physics.Raycast(pos - posOffset, Vector3.down, out hitBack, Mathf.Infinity);
        Physics.Raycast(pos + posOffset, Vector3.down, out hitFront, Mathf.Infinity);


        bool isHitFront = hitFront.collider != null;
        bool isHitBack = hitBack.collider != null;

        SetTerrainDistance(hitFront.distance);
        SetIsOnTerrain(isHitBack && isHitFront ? (hitFront.distance < 1f || hitBack.distance < 1f) : false);


        //GRIND
        RaycastHit hitGrindBack;
        RaycastHit hitGrindFront;

        Physics.Raycast(pos + Vector3.up * 1 - Vector3.right, Vector3.down, out hitGrindBack, Mathf.Infinity);
        Physics.Raycast(pos + Vector3.up * 1 + Vector3.right, Vector3.down, out hitGrindFront, Mathf.Infinity);

        bool isHitGrindFront = hitGrindFront.collider != null;
        bool isHitGrindBack = hitGrindBack.collider != null;

        SetIsGrindOnTerrain(
            isHitGrindFront && isHitGrindBack
            ? (hitGrindFront.collider.gameObject.tag == "Grind" || hitGrindBack.collider.gameObject.tag == "Grind")
            : false
        );

        //TERRAIN INCLINAISON
        var terrainObject = hitFront.collider != null ? hitFront.collider.gameObject : null;
        if (terrainObject == null) return;
        SetTerrainInclination(terrainObject.transform.InverseTransformVector(hitFront.normal));
    }

    private void SetTerrainDistance(float distance)
    {
        _terrainDistance = distance;
    }

    public float GetTerrainDistance()
    {
        return _terrainDistance;
    }


    private void SetIsGrindOnTerrain(bool value)
    {
        _isGrindOnTerrain = value;
    }

    public bool IsGrindOnTerrain()
    {
        return _isGrindOnTerrain;
    }


    private void SetIsOnTerrain(bool value)
    {
        _isOnTerrain = value;
    }

    public bool IsOnTerrain()
    {
        return _isOnTerrain;
    }


    private void SetTerrainInclination(Vector3 inclination)
    {
        _terrainInclination = inclination;
    }

    public Vector3 GetTerrainInclination()
    {
        return _terrainInclination;
    }
}
