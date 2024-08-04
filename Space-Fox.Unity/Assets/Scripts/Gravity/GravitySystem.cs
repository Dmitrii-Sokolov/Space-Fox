using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceFox
{
    public class GravitySystem : DisposableComposer
    {
        private const float GravitationalConstant = 1f; // 6.674×10^−11;

        private readonly List<Rigidbody> Bodies = new();

        private GravitySystem(UpdateProxy updateProxy)
        {
            updateProxy.FixedUpdate.Subscribe(OnFixedUpdate).While(this);

            AddDisposable(new Subscription(() => Bodies.Clear()));
        }

        public IDisposable AddAgent(Rigidbody agent)
        {
            Bodies.Add(agent);

            return new Subscription(() => Bodies.Remove(agent));
        }

        private void OnFixedUpdate()
        {
            for (var i = 0; i < Bodies.Count; i++)
            {
                for (var k = i + 1; k < Bodies.Count; k++)
                {
                    var agent0 = Bodies[k];
                    var agent1 = Bodies[i];

                    var vector = agent0.position - agent1.position;
                    var force = GravitationalConstant * agent1.mass * agent0.mass * vector.normalized / vector.sqrMagnitude;

                    agent0.AddForce(-force);
                    agent1.AddForce(force);
                }
            }
        }
    }
}
