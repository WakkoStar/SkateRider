using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer), typeof(BoxCollider))]
public class SpeedBooster : MonoBehaviour
{
    //STATE
    private MeshRenderer _meshRenderer;
    private SkateController _skateController;
    private bool isBoosterMaterialAnimated;
    private float _skateMaxSpeed;
    private float _skateBoostedSpeed;

    //SETTINGS
    [SerializeField] private BoosterScriptableObject props;
    [SerializeField] private GameObject Lights;

    void Start()
    {
        props.boosterMaterial.mainTextureScale = new Vector2(1 / (float)props.texStripLength, 1);

        _meshRenderer = GetComponent<MeshRenderer>();
        _meshRenderer.material = props.boosterMaterial;
    }

    void Update()
    {
        if (!isBoosterMaterialAnimated) StartCoroutine(AnimateBoosterMaterial());
    }

    private void OnTriggerEnter(Collider skate)
    {

        if (skate.GetComponent<SkateStateManager>() != null)
        {
            skate.GetComponent<SkateStateManager>().OnBoost.Invoke();
        }
    }

    private void OnTriggerExit(Collider skate)
    {
        if (skate.GetComponent<SkateStateManager>() != null)
        {
            _skateController = skate.GetComponent<SkateStateManager>().GetSkateController();
            _skateController.StopBoost();
        }
    }

    IEnumerator AnimateBoosterMaterial()
    {
        isBoosterMaterialAnimated = true;

        for (int i = 0; i < props.texStripLength; i++)
        {
            props.boosterMaterial.mainTextureOffset = new Vector2(i * (1 / (float)props.texStripLength), 0);
            Lights.transform.localPosition = new Vector3(props.lightsPositions[i / props.lightsPositions.Length], 0, 0);
            yield return new WaitForSeconds(0.1f);
        }
        isBoosterMaterialAnimated = false;
    }

}
