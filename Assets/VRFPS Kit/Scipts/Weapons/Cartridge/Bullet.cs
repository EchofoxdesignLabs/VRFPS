using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VRFPSKit
{
    /// <summary>
    /// Represents a physical bullet that has been fired
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        private const float MaxLifetime = 15;
        private const float ImpactEffectSearchRadius = .3f;
        private const float MinimumVelocity01 = .005f;

        public AudioSource hitSound;
        public GameObject defaultImpactEffect;
        public GameObject tracerTail;
        [Space]
        public BallisticProfile ballisticProfile;
        public BulletType bulletType;

        public BulletShooter shooter;
        
        //NOTE events will only be called on server
        public event Action<Bullet> HitEvent;
        public event Action<Bullet, IDamageReciever> HitDamageableEvent;

        private Rigidbody _rigidbody;

        // Start is called before the first frame update
        void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            
            //Bullet apply randomSpreadAngle to rotation
            if(ballisticProfile.randomSpreadAngle != 0)
                transform.rotation *= Quaternion.Euler(
                    Random.Range(-ballisticProfile.randomSpreadAngle, ballisticProfile.randomSpreadAngle) , 
                    Random.Range(-ballisticProfile.randomSpreadAngle, ballisticProfile.randomSpreadAngle), 0);//TODO this doesnt work
            
            _rigidbody.AddForce(transform.forward * ballisticProfile.startVelocity, ForceMode.VelocityChange);
            _rigidbody.useGravity = false; //Use custom gravity solution
            
            //Rigidbody drag from profile
            _rigidbody.linearDamping = ballisticProfile.drag;
            
            tracerTail.SetActive(bulletType == BulletType.Tracer);
            
            Invoke(nameof(Despawn), MaxLifetime);
        }

        private void FixedUpdate()
        {
            //Custom gravity implementation
            _rigidbody.AddForce(Physics.gravity * ballisticProfile.gravityScale, ForceMode.Acceleration);
            
            //Despawn bullet if remaining velocity is to low, not if it is 0 though cause that means it hasn't started moving yet
            //1 is starting velocity, 0 is standing still
            if (GetRemainingVelocity01() < MinimumVelocity01 && GetRemainingVelocity01() != 0){ DestroyBullet();}
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_rigidbody.isKinematic) return; //Do nothing if bullet is destroyed
            
            //Call Hit Event
            HitEvent?.Invoke(this);
            
            //Get damageable component in collider or in parents
            if (collision.gameObject.GetComponentInParent<IDamageReciever>() is IDamageReciever damageReciever)
            {
                float damage = ballisticProfile.baseDamage;
                if(ballisticProfile.scaleDamageWithVelocity)
                {
                    float impulseScale01 = collision.GetContact(0).impulse.magnitude / _rigidbody.mass / ballisticProfile.startVelocity;
                    damage *= impulseScale01;
                }
                damage = Mathf.Max(damage); //Damage can't be less than 0
                
                //Apply damage to damageable
                damageReciever.TakeDamage(damage);
                
                //Call Hit Damageable Event
                HitDamageableEvent?.Invoke(this, damageReciever);
            }

            //Play hit effects on clients
            PlayImpactEffect(collision.GetContact(0).point, collision.GetContact(0).normal);

            
            //Bouncing is performed by physics material
            
            //Use impact angle to determine velocity loss.
            float impactAngle = Vector3.Angle(collision.GetContact(0).normal, -transform.forward); //0 degrees = completely along normal
            float impactAngle01 = Mathf.Clamp01(Mathf.InverseLerp(0, 90, impactAngle));//0 = completely along normal, 1 = completely perpendicular to normal
            _rigidbody.linearVelocity *= impactAngle01; //Straight on impact will result in complete stop, completely perpendicular will mean no energy loss
        }
        
        private void DestroyBullet()
        {
            //Stop moving
            _rigidbody.isKinematic = true;

            //Schedule destruction, let sound play out first
            Invoke(nameof(Despawn), 2);
        }

        private float GetRemainingVelocity01() => Mathf.InverseLerp(0, ballisticProfile.startVelocity, _rigidbody.linearVelocity.magnitude); 
        
        private void PlayImpactEffect(Vector3 impactPoint, Vector3 impactNormal)
        {
            hitSound.Play();
            
            //Hide bullet renderer on all clients
            foreach(MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>())
                Destroy(renderer);

            //Locally spawn the particle effect
            Quaternion normalQuaternion = Quaternion.LookRotation(impactNormal);
            GameObject impactEffectPrefab = GetImpactEffect(impactPoint);
            
            GameObject impactEffectObj = Instantiate(impactEffectPrefab, impactPoint, normalQuaternion);
        }

        /// <summary>
        /// Method that tries to fetch a BulletImpactEffect component on nearby colliders.
        /// Doing it this way allows for clients to decide which effect to use on their own,
        /// instead of having to network impact effects (complicated since we can't easily send
        /// Prefab references over the network).
        /// </summary>
        /// <param name="impactPoint"></param>
        /// <returns></returns>
        private GameObject GetImpactEffect(Vector3 impactPoint)
        {
            //Try to find active BulletImpactEffect components near where the bullet hit
            foreach (Collider nearbyCollider in Physics.OverlapSphere(impactPoint, ImpactEffectSearchRadius))
                if(nearbyCollider.GetComponentInParent<BulletImpactEffect>() is BulletImpactEffect bulletImpactEffect && 
                   bulletImpactEffect.impactEffect != null)
                    //If one was found, use it's particle effect instead
                    return bulletImpactEffect.impactEffect;
                
            //If no BulletImpactEffect was found, use the default impact effect
            return defaultImpactEffect;
        }

        private void Despawn()
        {
            Destroy(gameObject);
        }
    }
}