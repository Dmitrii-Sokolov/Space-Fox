using System;
using System.Collections.Generic;

namespace SpaceFox
{
    public class GravitySystem : DisposableComposer
    {
        private const float GravitationalConstant = 1f; // 6.674×10^−11;

        private readonly List<GravityAgent> Agents = new();

        private GravitySystem(UpdateProxy updateProxy)
        {
            updateProxy.FixedUpdate.Subscribe(OnFixedUpdate).While(this);

            AddDisposable(new Subscription(() => Agents.Clear()));
        }

        public IDisposable AddAgent(GravityAgent agent)
        {
            Agents.Add(agent);

            return new Subscription(() => Agents.Remove(agent));
        }

        private void OnFixedUpdate()
        {
            //subject -->> agent
            foreach (var agent in Agents)
            {
                foreach (var subject in Agents)
                {
                    if (agent == subject)
                        continue;

                    var vector = agent.Body.position - subject.Body.position;
                    var force = GravitationalConstant * agent.Body.mass * subject.Body.mass * vector.normalized / vector.sqrMagnitude;
                    agent.Body.AddForce(force);
                }
            }
        }
    }
}
