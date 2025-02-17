using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;

namespace MindSpawner.Core
{
    public class Agent
    {
        public string Id { get; private set; }
        private string functionCode;
        private Engine jsEngine;
        public DateTime LastUpdate { get; private set; }
        public List<Dictionary<string, object>> History { get; private set; }

        public Agent(string agentId, string jsFunction)
        {
            Id = agentId;
            functionCode = jsFunction;
            History = new List<Dictionary<string, object>>();
            CompileFunction();
            LastUpdate = DateTime.UtcNow;
        }

        private void CompileFunction()
        {
            try
            {
                string wrappedJsFunction = $@"
                {functionCode}
                function main(input) {{
                    return stateActionHandler(input);
                }}";

                jsEngine = new Engine().Execute(wrappedJsFunction);
            }
            catch (Exception)
            {
                Console.WriteLine($"MindSpawner: Failed to compile agent {Id}.");
            }
        }

        public object Execute(Dictionary<string, object> inputData)
        {
            if (jsEngine == null)
            {
                Console.WriteLine($"MindSpawner: Agent {Id} is not compiled.");
                return null;
            }

            try
            {
                object output = jsEngine.Invoke("main", inputData).ToObject();
                History.Add(new Dictionary<string, object> { { "i", inputData }, { "o", output } });
                return output;
            }
            catch (Exception)
            {
                Console.WriteLine($"MindSpawner: Failed to run agent {Id}.");
                return null;
            }
        }

        public void UpdateFunction(string jsFunction)
        {
            jsEngine = new Jint.Engine().Execute(jsFunction);
        }
    }
}
