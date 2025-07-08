using CrashKonijn.Goap.Behaviours;
using UnityEngine;
using VRDefender.GOAP.Config;
using VRFPSKit;

namespace VRDefender.GOAP.Behaviors
{
    public class TakeCoverBehaviour : MonoBehaviour
    {
        [SerializeField] public float Health { get; set; }
        
        [SerializeField] private Damageable damageable;
        
        [SerializeField] private GetToSafetyConfigSO getToSafetyConfigSO;

        private void Awake()
        {
            damageable = GetComponent<Damageable>();
        }
        private void Start()
        {
            Health = damageable.health;    
        }

        private void Update()
        {
            Health -= Time.deltaTime * 1;
            damageable.health = Health;
        }
    }
}

