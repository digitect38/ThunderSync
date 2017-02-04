using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetSync
{
    public class NetSyncServer : NetSyncBase
    {
        Dictionary<string, Dictionary<EndPoint, SubscribeMode>> _propMap;
        bool _close = false;

        public NetSyncServer()
        {
            _propMap = new Dictionary<string, Dictionary<EndPoint, SubscribeMode>>();
        }

        protected override void Send(string str)
        {
            // todo:
        }

        protected override void Send(EndPoint ep, string str)
        {
            // todo:
        }
        protected override void RegisterProperty(string name, Type type)
        {
            // todo:
        }
        public override void SetProperty(string name, Type type, object value)
        {
            Dictionary<EndPoint, SubscribeMode> cliDic = _propMap[name];

            foreach (KeyValuePair<EndPoint, string> entry in cliDic)
            {
                // do something with entry.Value or entry.Key
                //string packet = "setprop:" + name + type.ToString()
                //Send(entry.Key, )
            }   
            // todo:
        }
        protected override void Parse(string rcvdString, EndPoint endPoint = null)
        {
            string[] strs = rcvdString.Split(':');

            switch (strs[0])
            {
                case "subscli":    // subscribe client
                    {
                        SubscribeMode subsMode = StringToSubscribeMode(strs[2]);
                        string propName = strs[1];
                        if (_propMap[propName] == null)
                        {
                            _propMap[propName] = new Dictionary<EndPoint, SubscribeMode>();
                        }
                        _propMap[propName][endPoint] = subsMode;
                    }
                    break;

                case "unscli":    // subscribe client
                    {
                        string propName = strs[1];
                        if (_propMap[propName] != null)
                        {
                            _propMap[propName][endPoint] = SubscribeMode.UnSubscribed;
                        }
                    }
                    break;

                case "setprop":     // set property
                    {
                        string name = strs[1];
                        Type type = StringToType(strs[2]);
                        object value = StringToValue(strs[3]);
                        SetProperty(name,type,value);
                    }
                    break;
            }
        }

        void ReceiveThreadStart()
        {
            int recv = 0;
            byte[] data = new byte[1024];

            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 9050);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            server.Bind(ep);

            Console.WriteLine("Waiting for a client...");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remoteEP = (EndPoint)sender;

            while (!_close)
            {
                data = new byte[1024];
                recv = server.ReceiveFrom(data, ref remoteEP);
                string recvData = Encoding.UTF8.GetString(data, 0, recv);
                Parse(recvData, remoteEP);
            }

            server.Close();
        }
    }
}
