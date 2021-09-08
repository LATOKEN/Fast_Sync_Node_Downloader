using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;


namespace test_download
{
    class NodeStorage
    {
        private IDictionary<string, JObject> _nodeStorage = new Dictionary<string, JObject>();
        public NodeStorage()
        {
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool TryAddNode(JObject node)
        {
            string nodeHash = (string)node["Hash"];
            if (nodeHash == null) return false; 
            if(!_nodeStorage.TryGetValue(nodeHash, out var value))
            {
                _nodeStorage[nodeHash] = node;
                System.Console.WriteLine(node);
                return true;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool TryGetNode(string nodeHash, out JObject node)
        {
            return _nodeStorage.TryGetValue(nodeHash, out node);
        }

        public bool IsConsistent(JObject node)
        {
            return node != null && node.Count > 0;
        }
    }
}
