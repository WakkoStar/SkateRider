using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(BoxCollider))]
public class SpeedBooster : MonoBehaviour
{
    //STATE
    private MeshRenderer _meshRenderer;
    private bool isBoosterMaterialAnimated;
    private float _skateMaxSpeed;
    private float _skateBoostedSpeed;
    private Material _boosterMaterial;

    //SETTINGS
    [SerializeField] private BoosterScriptableObject props;
    [SerializeField] private GameObject Lights;

    void Start()
    {
        _boosterMaterial = props.boosterMaterial;
        _boosterMaterial.mainTextureScale = new Vector2(1 / (float)props.texStripLength, 1);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = _boosterMaterial;
    }

    void Update()
    {
        if (!isBoosterMaterialAnimated) StartCoroutine(AnimateBoosterMaterial());
    }

    private void OnTriggerEnter(Collider skate)
    {

        if (
            skate.GetComponent<SkateStateManager>() != null
            && !skate.GetComponent<SkateStateManager>().GetSkateRotationReader().IsTrueUpsideDown()
        )
        {

            skate.GetComponent<SkateStateManager>().OnBoost.Invoke();
        }
    }

    private void OnTriggerExit(Collider skate)
    {
        if (skate.GetComponent<SkateStateManager>() != null)
        {
            skate.GetComponent<SkateStateManager>().GetSkatePhysicsController().StopBoost();
        }
    }

    IEnumerator AnimateBoosterMaterial()
    {
        isBoosterMaterialAnimated = true;

        for (int i = 0; i < props.texStripLength; i++)
        {
            _boosterMaterial.mainTextureOffset = new Vector2(i * (1 / (float)props.texStripLength), 0);
            var lightPos = props.lightsPositions[i / props.lightsPositions.Length];
            Lights.transform.localPosition = new Vector3(lightPos.x, lightPos.y, lightPos.z);
            yield return new WaitForSeconds(0.1f);
        }
        isBoosterMaterialAnimated = false;
    }

}
