using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThunderSync
{
    public delegate void SubscribeEventHandler(EndPoint senderEP, SubscribeMode subsMode, string name);
    public delegate void UnSubscribeEventHandler(EndPoint senderEP, SubscribeMode subsMode, string name);
    public class ThunderSyncServer : ThunderSyncBase
    {
        Dictionary<int, Dictionary<EndPoint, SubscribeMode>> _subscMap;

        public SubscribeEventHandler OnSubscribe;
        public UnSubscribeEventHandler OnUnsubscribe;

        public ThunderSyncServer()
        {
            _subscMap = new Dictionary<int, Dictionary<EndPoint, SubscribeMode>>();
        }

        void Send (int id, object value)
        {
            string name = Property.IdToName(id);
            Type type = _id2PropMap[id].Type;
            string str = "setprop|" + name + "|" + ValueToString(type, value) + "|";
            Dictionary<EndPoint, SubscribeMode> cliDic = _subscMap[id];
            foreach (KeyValuePair<EndPoint, SubscribeMode> entry in cliDic)
            {
                EndPoint ep = entry.Key;
                if (entry.Value == SubscribeMode.Consumer)
                {
                    Send(ep, str);
                }
            }
        }

        public override void SetPropertyValue(int id, object value)
        {
            if (!_subscMap.ContainsKey(id))
            {
                _subscMap[id] = new Dictionary<EndPoint, SubscribeMode>();
            }
            Send(id, value);
        }
        public override void SetPropertyValue(string name, object value)
        {
            SetPropertyValue(Property.NameToId(name),value);
        }
        protected override void Parse(byte[] data, EndPoint senderEP = null)
        {
            string rcvdString = Encoding.UTF8.GetString(data);
            string[] strs = rcvdString.Split('|');

            switch (strs[0])
            {
                case "subscli":    // subscribe client
                    {
                        Type type = StringToType(strs[2]);
                        SubscribeMode subsMode = StringToSubscribeMode(strs[3]);
                        ProtocolMode protoMode = StringToProtocolMode(strs[4]);
                        //int id = Property.NameToId(strs[1]);
                        int id = Convert.ToInt32(strs[1]);

                        if (!_subscMap.ContainsKey(id)) {
                            _subscMap[id] = new Dictionary<EndPoint, SubscribeMode>();
                            _id2PropMap[id] = new Property(id, type, subsMode, protoMode);
                        }
                        _subscMap[id][senderEP] = subsMode;
                        OnSubscribe.Invoke(senderEP, subsMode, Property.IdToName(id));
                    }
                    break;

                case "unscli":    // subscribe client
                    {
                        //int id = Property.NameToId(strs[1]);
                        int id = Convert.ToInt32(strs[1]);
                        if (_subscMap.ContainsKey(id))
                        {
                            Dictionary<EndPoint, SubscribeMode> cliDic = _subscMap[id];
                            if (cliDic.ContainsKey(senderEP))
                            {
                                SubscribeMode subsMode = cliDic[senderEP];
                                OnUnsubscribe.Invoke(senderEP, subsMode, Property.IdToName(id));
                            }
                        }
                    }
                    break;

                case "setprop":     // set property
                    {
                        int id = Convert.ToInt32(strs[1]);
                        if (_subscMap.ContainsKey(id))
                        {
                            Type type = _id2PropMap[id].Type;
                            object value = StringToValue(type, strs[2]);
                            SetPropertyValue(id, (double)value);
                        }
                    }
                    break;
            }
        }

        protected override void InitSocket()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, SERVER_PORT);
            Socket.Bind(ep);
        }

        protected override void CommThreadStartFunc()
        {
            int recv = 0;
            byte[] data = new byte[1024];


            Console.WriteLine("Waiting for a client...");

            EndPoint senderEP = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

            while (!_close)
            {
                data = new byte[1024];
                try
                {
                    recv = Socket.ReceiveFrom(data, ref senderEP);
                }
                catch (Exception ex)
                {
                }
                Parse(data, senderEP);
            }

            Socket.Close();
        }
    }
}
