using System;
using System.Collections.Generic;

namespace test_download
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> urls = new List<string>
            {
                 "http://157.245.160.201:7070",
                "http://95.217.6.171:7070",
                "http://88.99.190.191:7070",
                "http://94.130.78.183:7070",
                "http://94.130.24.163:7070",
                "http://94.130.110.127:7070",
                "http://94.130.110.95:7070",
                "http://94.130.58.63:7070",
                "http://88.99.86.166:7070",
                "http://88.198.78.106:7070",
                "http://88.198.78.141:7070",
                "http://88.99.126.144:7070",
                "http://88.99.87.58:7070",
                "http://95.217.6.234:7070"
            };

            PeerManager peerManager = new PeerManager(urls);
            NodeStorage nodeStorage = new NodeStorage();
            RequestManager requestManager = new RequestManager(nodeStorage);
            Downloader downloader = new Downloader(peerManager, requestManager);

            string[] names = new string[]
            {
                "Balances", "Transactions", "Events", "Storage", "Blocks", "Validators", "Contracts"
            };

            foreach(var name in names)
            {
                downloader.GetTrie(name);
            }
        }
    }
}
