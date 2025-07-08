using UnityEngine;

namespace VRDefender.GOAP.Config
{
    [CreateAssetMenu(menuName = "AI/Get To Safty Config", fileName = "Get_To_Safety_Config", order = 2)]
    public class GetToSafetyConfigSO : ScriptableObject
    {
        public float coverSearchRadius = 20f;
        public LayerMask coverLayer;
        
    }
}

