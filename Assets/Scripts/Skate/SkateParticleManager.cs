using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SkateStateManager))]
public class SkateParticleManager : MonoBehaviour
{
    [SerializeField] private ParticleDisplayer NoseGrindSparks;
    [SerializeField] private ParticleDisplayer TailGrindSparks;
    [SerializeField] private ParticleDisplayer BoardJumpSlam;
    [SerializeField] private ParticleDisplayer TruckOnJumpLandingSlam;
    [SerializeField] private ParticleDisplayer BoostSpeedLines;
    [SerializeField] private GameObject NoseTruck;
    [SerializeField] private GameObject TailTruck;
    private SkateStateManager _skateState;
    private GameObject _cameraObj;
    private UnityAction _onJumpLandingAction;
    private UnityAction _onJumpAction;
    private UnityAction _onBoostAction;

    private UnityAction<Collider, GameObject> _onTriggerStayTruckNoseAction;
    private UnityAction<Collider, GameObject> _onTriggerExitTruckNoseAction;
    private UnityAction<Collider, GameObject> _onTriggerStayTruckTailAction;
    private UnityAction<Collider, GameObject> _onTriggerExitTruckTailAction;

    // Start is called before the first frame update
    void Start()
    {
        _skateState = GetComponent<SkateStateManager>();
        _cameraObj = Camera.main.gameObject;

        _onTriggerStayTruckNoseAction += DisplayGrindSparks;
        _onTriggerExitTruckNoseAction += HideGrindSparks;

        _onTriggerStayTruckTailAction += DisplayGrindSparks;
        _onTriggerExitTruckTailAction += HideGrindSparks;

        NoseTruck.GetComponent<TriggerColliderNotifier>().onTriggerStay.AddListener(_onTriggerStayTruckNoseAction);
        NoseTruck.GetComponent<TriggerColliderNotifier>().onTriggerExit.AddListener(_onTriggerExitTruckNoseAction);

        TailTruck.GetComponent<TriggerColliderNotifier>().onTriggerStay.AddListener(_onTriggerStayTruckTailAction);
        TailTruck.GetComponent<TriggerColliderNotifier>().onTriggerExit.AddListener(_onTriggerExitTruckTailAction);

        _onJumpLandingAction += DisplayTruckOnJumpLandingSlam;
        _skateState.OnJumpLandingEvent.AddListener(_onJumpLandingAction);

        _onBoostAction += DisplayBoostSpeedLines;
        _skateState.OnBoost.AddListener(_onBoostAction);

        _onJumpAction += DisplayBoardSlam;
        _skateState.OnJumpEvent.AddListener(DisplayBoardSlam);
    }

    void DisplayTruckOnJumpLandingSlam()
    {
        TruckOnJumpLandingSlam.Display(TailTruck.transform.position + Vector3.up * 0.6f);
    }

    void DisplayBoostSpeedLines()
    {
        BoostSpeedLines.Display(_cameraObj.transform.position - Vector3.back * 1.5f);
    }

    void DisplayBoardSlam()
    {
        BoardJumpSlam.Display(_skateState.transform.position + Vector3.up + Vector3.left * 1.5f);
    }

    void DisplayGrindSparks(Collider other, GameObject target)
    {
        if (other.gameObject.tag != "Grind") return;

        var isSwitch = _skateState.GetSkateRotationReader().IsSwitch();

        var basePos = target.transform.position;

        if (target.name == NoseTruck.name)
        {
            var pos = isSwitch ? basePos + Vector3.left * 2.7f : basePos + Vector3.left * 1.1f;
            NoseGrindSparks.Display(pos + Vector3.up * 0.2f, new Vector3(0, -90, 0));
        }
        else
        {
            var pos = isSwitch ? basePos + Vector3.left * 1.1f : basePos + Vector3.left * 2.7f;
            TailGrindSparks.Display(pos + Vector3.up * 0.2f, new Vector3(0, -90, 0));
        }
    }

    void HideGrindSparks(Collider other, GameObject target)
    {
        if (target.name == NoseTruck.name)
        {
            NoseGrindSparks.Hide();
        }
        else
        {
            TailGrindSparks.Hide();
        }
    }
}
