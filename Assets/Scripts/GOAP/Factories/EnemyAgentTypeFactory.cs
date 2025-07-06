using System;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace VRDefender.GOAP.Factories
{
    public class EnemyAgentTypeFactory : AgentTypeFactoryBase
    {
        public override IAgentTypeConfig Create()
        {
            var factory = new AgentTypeBuilder("Enemy");
            BuildCapability(factory);
            return factory.Build();
        }

        private void BuildCapability(AgentTypeBuilder factory)
        {
            factory.AddCapability<GoapCapabilityFactory>();
        }
    }
}

