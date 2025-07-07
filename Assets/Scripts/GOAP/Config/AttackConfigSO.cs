using UnityEngine;

namespace VRDefender.GOAP.Config
{
    [CreateAssetMenu(menuName = "AI/Attack Config", fileName = "Attack_Config", order = 1)]
    public class AttackConfigSO : ScriptableObject
    {
        public float attackDelay = 1f;
        public LayerMask attackableLayerMask;
        public float meleeAttackRadius = 1f;
        public int meleeAttackCost = 1;
        public float rangedAttackRadius = 2f;
        public int rangedAttackCost = 2;
        public float sensorRadius = 10f;


    }
}

