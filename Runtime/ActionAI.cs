using System;
using System.Collections.Generic;

namespace MindSpawner.Core
{
    public class ActionAI
    {
        public List<Agent> Agents { get; private set; }

        public ActionAI()
        {
            Agents = new List<Agent>();
        }

        public void AddAgent(string agentId, string jsFunction)
        {
            if (Agents.Exists(a => a.Id == agentId))
            {
                Console.WriteLine($"MindSpawner: Agent {agentId} already exists. Failed to add agent.");
                return;
            }

            var agent = new Agent(agentId, jsFunction);
            Agents.Add(agent);
        }

        public Agent GetAgent(string agentId)
        {
            return Agents.Find(a => a.Id == agentId);
        }

        public object ExecuteAgent(string agentId, Dictionary<string, object> inputData)
        {
            var agent = GetAgent(agentId);
            return agent != null ? agent.Execute(inputData) : null;
        }

        public void UpdateAgentFunction(string agentId, string jsFunction)
        {
            var agent = GetAgent(agentId);
            if (agent != null)
            {
                agent.UpdateFunction(jsFunction);
                Console.WriteLine($"MindSpawner: Agent {agentId} has been updated");
            }
            else
            {
                Console.WriteLine($"MindSpawner: Agent {agentId} could not be found. Failed to update.");
            }
        }

        public List<object> GetAgentHistory(string agentId)
        {
            var agent = GetAgent(agentId);
            return agent != null ? new List<object>(agent.History) : new List<object>();
        }
    }
}
