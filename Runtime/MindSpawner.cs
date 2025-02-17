using System;
using System.Collections.Generic;
using SocketIOClient;
using SocketIOClient.Newtonsoft.Json;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace MindSpawner.Core
{
    public class MindSpawner
    {
        private string userId;
        private string actionAiId;
        private SocketIOUnity socket;
        private ActionAI actionAI;
        public bool isReady = false;

        public MindSpawner(string userId, string actionAiId)
        {
            this.userId = userId;
            this.actionAiId = actionAiId;
            this.actionAI = new ActionAI();

            var uri = new Uri("http://localhost:3000");
            socket = new SocketIOUnity(uri, new SocketIOClient.SocketIOOptions
            {
                Query = new Dictionary<string, string> { { "token", "UNITY" } },
                EIO = 4,
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
            });

            socket.JsonSerializer = new NewtonsoftJsonSerializer();

            // Event Handlers
            socket.OnConnected += (sender, e) =>
            {
                // Emit only AFTER the connection is confirmed
                socket.Emit("libraryData", new { id = new { user = userId, actionai = actionAiId } });
            };

            socket.OnError += (sender, error) => Debug.LogError($"❌ Error: {error}");
            socket.OnDisconnected += (sender, reason) => Debug.LogWarning($"🔌 Disconnected: {reason}");

            // Register event listeners
            socket.On("libraryData", response => InitializeActionAI(response.GetValue<LibraryData>()));
            socket.On("updateAgent", response => UpdateAgentFunction(response.GetValue<AgentUpdateData>()));

            socket.Connect();
        }


        private void InitializeActionAI(LibraryData data)
        {
            foreach (var agentData in data.Agents)
            {
                actionAI.AddAgent(agentData.Id, agentData.Function);
            }
            isReady = true;
        }

        public object Agent(string agentId, Dictionary<string, object> inputData)
        {
            if (!isReady) return null;
            return actionAI.ExecuteAgent(agentId, inputData); // Now returns an object
        }

        public void UpdateAgent(string agentId, object evaluationScore = null)
        {
            object score = evaluationScore switch
            {
                int intValue => intValue,
                string strValue => strValue,
                _ => "auto"
            };

            socket.Emit("updateAgent", new
            {
                id = new { user = userId, actionai = actionAiId, agent = agentId },
                score = score,
                history = actionAI.GetAgentHistory(agentId)
            });
        }

        private void UpdateAgentFunction(AgentUpdateData data)
        {
            actionAI.UpdateAgentFunction(data.AgentId, data.Function);
        }
    }

    public class LibraryData
    {
        public List<AgentData> Agents { get; set; }
    }

    public class AgentData
    {
        public string Id { get; set; }
        public string Function { get; set; }
    }

    public class AgentUpdateData
    {
        public string AgentId { get; set; }
        public string Function { get; set; }
    }
}
