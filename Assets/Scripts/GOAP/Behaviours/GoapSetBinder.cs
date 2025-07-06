using System;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace VRDefender.GOAP.Behaviours
{
    public class GoapSetBinder : MonoBehaviour
    {
        [SerializeField] private GoapBehaviour goapBehaviour;

        private void Awake()
        {
            GoapActionProvider provider = GetComponent<GoapActionProvider>();
            Debug.Log("here"+goapBehaviour.AgentTypes);
            provider.AgentType = goapBehaviour.GetAgentType("Enemy");
        }
    }
}

