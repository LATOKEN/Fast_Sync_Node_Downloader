using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace test_download
{
    class RequestManager
    {
        private Queue<string> _queue = new Queue<string>();
        private HashSet<string> _pending = new HashSet<string>();
        private NodeStorage _nodeStorage;
        private uint _batchSize = 20;

        public RequestManager(NodeStorage nodeStorage)
        {
            _nodeStorage = nodeStorage;
        }

        public bool TryGetHashBatch(out List<string> hashBatch)
        {
            hashBatch = new List<string>();
            lock(this)
            {
                while(_queue.Count > 0 && hashBatch.Count < _batchSize)
                {
                    var hash = _queue.Dequeue();
                    if(_nodeStorage.TryGetNode(hash, out var node)) continue;
                    hashBatch.Add(hash);
                    _pending.Add(hash);
                }
            }
            if (hashBatch.Count == 0) return false;

            return true;
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Done()
        {
            return _queue.Count == 0 && _pending.Count == 0;
        }

        public void HandleResponse(List<string> hashBatch, JArray response)
        {
            List<string> successfulHashes = new List<string>();
            List<string> failedHashes = new List<string>();
            List<JObject> successfulNodes = new List<JObject>();

            if (hashBatch.Count != response.Count)
            {
                failedHashes = hashBatch;
            }
            else
            {
                for (var i = 0; i < hashBatch.Count; i++)
                {
                    var hash = hashBatch[i];
                    JObject node = (JObject)response[i];
                    if (_nodeStorage.IsConsistent(node))
                    {
                        successfulHashes.Add(hash);
                        successfulNodes.Add(node);
                    }
                    else
                    {
                        failedHashes.Add(hash);
                    }
                }
            }

            lock (this)
            {
                foreach (var hash in failedHashes)
                {
                    if (!_pending.TryGetValue(hash, out var foundHash) || !hash.Equals(foundHash))
                    {
                        // do nothing, this request was probably already served
                    }
                    else
                    {
                        _pending.Remove(hash);
                        _queue.Enqueue(hash);
                    }
                }


                for(var i = 0; i < successfulHashes.Count; i++)
                {
                    var hash = successfulHashes[i];
                    var node = successfulNodes[i];
                    if (!_pending.TryGetValue(hash, out var foundHash) || !hash.Equals(foundHash))
                    {
                        // do nothing, this request was probably already served
                    }
                    else
                    {
                        _nodeStorage.TryAddNode(node);
                        _pending.Remove(hash);

                        var nodeType = (string)node["NodeType"];
                        if (nodeType == null) continue;

                        if (nodeType.Equals("0x1")) // internal node 
                        {
                            var jsonChildren = (JArray)node["ChildrenHash"];
                            foreach (var jsonChild in jsonChildren)
                            {
                                _queue.Enqueue((string)jsonChild);
                            }
                        }
                    }
                }
            }
        }
        public void AddHash(string hash)
        {
            lock(_queue)
            {
                _queue.Enqueue(hash);
            }
        }
    }
}
