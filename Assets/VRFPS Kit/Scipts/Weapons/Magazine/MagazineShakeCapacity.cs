using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using VRFPSKit;

public class MagazineShakeCapacity : MonoBehaviour
{
    public AnimationCurve transparencyCurve = AnimationCurve.Linear(0, 1, 1, 0);
    public TextMeshProUGUI[] amountTexts; 
    public TextMeshProUGUI[] typeTexts; 

    private const float _shakeVelocityTrackSpan = .2f;
    private const float _shakeVelocityChangeMagnitude = 0.15f;
    
    private float _shakeTime;
    private Queue<KeyValuePair<float, Vector3>> _velocityOverTime = new Queue<KeyValuePair<float, Vector3>>();
    private Vector3 _lastFramePosition;
    private float _grabTime;
    
    private Magazine _magazine;
    private CanvasGroup _canvasGroup;
    //TODO detect shakes
    //TODO emmissive UI text


    // Update is called once per frame
    void FixedUpdate()
    {
        _canvasGroup.alpha = transparencyCurve.Evaluate(Time.time - _shakeTime);
        
        //Disable canvas for performance when curve is done
        _canvasGroup.gameObject.SetActive(Time.time - _shakeTime < transparencyCurve.keys[transparencyCurve.length - 1].time);

        DetectShake();
    }
    
    private void DetectShake()
    {
        //Only do UI when player is holding
        XRGrabInteractable grabInteractable = _magazine.GetComponent<XRGrabInteractable>();
        if (grabInteractable == null) return;
        if (!grabInteractable.isSelected) return;
        
        //We need to retrieve the player controller from the selecting interactor
        if (_magazine.GetComponent<XRGrabInteractable>().interactorsSelecting[0] is not XRDirectInteractor handInteractor) return;
        if (handInteractor.GetComponentInParent<CharacterController>() is not CharacterController playerController) return;
        
        
        //Calculate velocity relative to player since last frame
        Vector3 positionRelativeToPlayer = transform.position - playerController.transform.position;
        Vector3 velocitySinceLastFrame = positionRelativeToPlayer - _lastFramePosition;
        _lastFramePosition = positionRelativeToPlayer;
        
        //Store velocity since last frame
        _velocityOverTime.Enqueue(new KeyValuePair<float, Vector3>(Time.time, velocitySinceLastFrame));
        
        //Clear all entries older than _shakeTrackSpan
        while (_velocityOverTime.Count > 0)
            if (Time.time - _velocityOverTime.Peek().Key > _shakeVelocityTrackSpan)
                _velocityOverTime.Dequeue();
            else break;
        
        if (Time.time - _grabTime < .5f) return; //Cooldown after grabbing
        
        //TODO needs more entries
        float totalMagnitudeChange = _velocityOverTime.Zip(_velocityOverTime.Skip(1), (a, b) => Math.Abs(b.Value.magnitude - a.Value.magnitude)).Sum();
        if (totalMagnitudeChange > _shakeVelocityChangeMagnitude && Time.time - _shakeTime > transparencyCurve.keys[^1].time)
            Shake();
    }
    
    [ContextMenu("Display Counter UI")]
    public void Shake()
    {
        _shakeTime = Time.time;
        
        //Update UI text
        foreach (var amountText in amountTexts)
            amountText.text = _magazine.GetCartridges().Length.ToString();
        foreach (var typeText in typeTexts)
        {
            string type = "Empty";
            if(_magazine.GetCartridges().Length > 0) type = _magazine.GetTopCartridge().bulletType.ToString();
                
            typeText.text = type;
        }
        
        //Vibrate 3 - 1 times depending on round count
        float roundCount01 = (float)_magazine.GetCartridges().Length / _magazine.capacity;
        int timesToVibrate = (int)(roundCount01 * 3);
        VibratePattern(timesToVibrate);
    }
    
    private async void VibratePattern(int pulseCount)
    {
        if (GetComponentInParent<XRBaseInteractable>().interactorsSelecting.Count == 0) return;
        XRBaseInputInteractor holdingHand = GetComponentInParent<XRBaseInteractable>().interactorsSelecting[0] as XRBaseInputInteractor;

        for (int i = 0; i < pulseCount; i++)
        {
            holdingHand.SendHapticImpulse(1, .05f);

            await Task.Delay(350);
        }
    }
    
    void Awake()
    {
        _canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        _magazine = GetComponentInParent<Magazine>();
        GetComponentInParent<XRBaseInteractable>().selectEntered.AddListener(_ => _grabTime = Time.time);
    }
}
