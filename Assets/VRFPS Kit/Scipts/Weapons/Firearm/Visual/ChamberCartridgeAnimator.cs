using System;
using System.Linq;
using UnityEngine;
using VRFPSKit;

public class ChamberCartridgeAnimator : MonoBehaviour
{
    [Header("Flip feed animation on X axis to match magazine feeding. Default side depends on weapon so look up first")]
    public bool mirrorXToMatchMagazineFeed = true;
    public bool mirrorYToMatchMagazineFeed;
    public bool mirrorRotation = true;
    [Space]
    [Header("Feed Animation Keyframes")]
    public ChamberCartridgeAnimationKeyframe[] keyframes;

    private float _cartridgeMinPosition01;
    private Vector3 _lastMovingForwardBodyOffset;
    
    private FirearmCyclingAction _cyclingAction;
    private Firearm _firearm;

    private void Update()
    {
        if (IsPushingCartridge())
        {
            AnimatePath();
        }
        //TODO CartridgeRenderer 2 frame delay to show
    }

    private void LateUpdate()
    {
        if (!IsPushingCartridge() && _cartridgeMinPosition01 > .05f) FreezeFeedingCartridge();
        if (IsPushingCartridge())
        {
            _cartridgeMinPosition01 = _cyclingAction.actionPosition01;
            _lastMovingForwardBodyOffset = GetBodyTransform().InverseTransformPoint(transform.position);
        }
        if(_firearm.chamberCartridge.IsNull() && _cyclingAction.loadingCartridge.IsNull()) _cartridgeMinPosition01 = 1;
    }

    private void AnimatePath()
    {
        if (keyframes.Length < 2) return;
        
        //Find the keyframes that are before and after the current time
        //This assumes the list is sorted by time, which is enforced in OnValidate
        ChamberCartridgeAnimationKeyframe keyBefore = keyframes[0];
        ChamberCartridgeAnimationKeyframe keyAfter = keyframes[keyframes.Length - 1];
        float animationTime = _cyclingAction.actionPosition01;
        for (int i = 0; i < keyframes.Length; i++)
        {
            if (keyframes[i].time01 > animationTime)
            {
                keyAfter = keyframes[i];
                break;
            }
            keyBefore = keyframes[i];
        }
        
        //Interpolate between the two keyframes
        float interpolateVal01 = Mathf.InverseLerp(keyBefore.time01, keyAfter.time01, animationTime);
        transform.localPosition = Vector3.Lerp(keyBefore.position, keyAfter.position, interpolateVal01);
        transform.localRotation = Quaternion.Lerp(Quaternion.Euler(keyBefore.rotation), Quaternion.Euler(keyAfter.rotation), interpolateVal01);

        AdjustMirror();
    }

    private void FreezeFeedingCartridge()
    {
        transform.position = GetBodyTransform().TransformPoint(_lastMovingForwardBodyOffset);
    }
    
    private bool IsPushingCartridge() => _cyclingAction.actionPosition01 < _cartridgeMinPosition01;
    
    private Transform GetBodyTransform() => transform.parent.parent;

    private void AdjustMirror()
    {
        if (_firearm.magazine == null) return;
        if (_firearm.magazine.GetComponentInChildren<MagazineCartridgeFeedRenderer>() is not MagazineCartridgeFeedRenderer feedRenderer) return;
        if (feedRenderer.ShouldFlipCartridges()) return;
        
        if (mirrorXToMatchMagazineFeed && mirrorRotation)
        {
            transform.localRotation = new Quaternion(transform.localRotation.x,
                transform.localRotation.y * -1.0f,
                transform.localRotation.z * -1.0f,
                transform.localRotation.w);
        }
        if (mirrorYToMatchMagazineFeed && mirrorRotation)
        {
            transform.localRotation = new Quaternion(transform.localRotation.x * -1.0f,
                transform.localRotation.y,
                transform.localRotation.z* -1.0f,
                transform.localRotation.w);
        }
        
        //Can't use Vector3.Scale to mirror because it doesn't work with small values
        transform.localPosition = new Vector3(transform.localPosition.x * (mirrorXToMatchMagazineFeed?-1:1), transform.localPosition.y * (mirrorYToMatchMagazineFeed?-1:1), transform.localPosition.z);
    }

    private void Awake()
    {
        _cyclingAction = GetComponentInParent<FirearmCyclingAction>();
        _firearm = GetComponentInParent<Firearm>();
        
        //Sort once more to make sure
        Array.Sort(keyframes, (a, b) => a.time01.CompareTo(b.time01));
    }

    private void OnValidate()
    {
        //Always sort keyframes by time
        Array.Sort(keyframes, (a, b) => a.time01.CompareTo(b.time01));
    }
}

[Serializable]
public struct ChamberCartridgeAnimationKeyframe{
    [Range(0, 1)]public float time01;
    public Vector3 position;
    public Vector3 rotation;
    
    public ChamberCartridgeAnimationKeyframe(float time01, Vector3 position, Vector3 rotation)
    {
        this.time01 = time01;
        this.position = position;
        this.rotation = rotation;
    }
}