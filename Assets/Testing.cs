using System.Threading.Tasks;
using UnityEngine;
using VRFPSKit;

public class Testing : MonoBehaviour
{
    [SerializeField] Firearm firearm;
    [SerializeField] FirearmTrigger trigger;
    public float countDown = 5;
    private float currentTime;
    void Start()
    {
        firearm.TryShoot();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTime > 0)
        {
            // Subtract the time that has passed since the last frame.
            currentTime -= Time.deltaTime;
        }
        else
        {

            LoadRound(trigger, new Cartridge(Caliber.Cal_9x19, BulletType.FMJ));
            PullTrigger(trigger);
            currentTime = countDown;
        }
    }
    
    private void LoadRound(FirearmTrigger trigger, Cartridge cartridge)
        {
            Firearm firearm = trigger.GetComponent<Firearm>();

            firearm.chamberCartridge = cartridge;
        }

        private async void PullTrigger(FirearmTrigger trigger)
        {
            trigger.PressTrigger();
            trigger.GetComponent<Firearm>().TryShoot();
            
            await Task.Delay(500);
            trigger.ReleaseTrigger();
            trigger.ResetTrigger();
        }
}
