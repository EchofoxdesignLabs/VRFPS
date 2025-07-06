using CrashKonijn.Agent.Core;
using UnityEngine;

namespace VRDefender.GOAP.Actions
{
    public class CommonData : IActionData
    {
        public ITarget Target { get; set; }
        public float Timer { get; set; }
    }
}

