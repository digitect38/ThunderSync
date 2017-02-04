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
        Dictionary<int, Dictionary<EndPoint, SubscribeMode>> _propMap;

        public SubscribeEventHandler OnSubscribe;
        public UnSubscribeEventHandler OnUnsubscribe;

        public ThunderSyncServer()
        {
            _propMap = new Dictionary<int, Dictionary<EndPoint, SubscribeMode>>();
        }

        void Send (int id, Type type, object value)
        {
            string name = Property.IdToName(id);
            string str = "setprop|" + name + "|" + type.ToString() + "|" + ValueToString(type, value) + "|";
            Dictionary<EndPoint, SubscribeMode> cliDic = _propMap[id];
            foreach (KeyValuePair<EndPoint, SubscribeMode> entry in cliDic)
            {
                EndPoint ep = entry.Key;
                if (entry.Value == SubscribeMode.Consumer)
                {
                    Send(ep, str);
                }
            }
        }                        
        public override void SetProperty(string name, Type type, object value)
        {
            int id = Property.NameToId(name);
            if (!_propMap.ContainsKey(id))
            {
                _propMap[id] = new Dictionary<EndPoint, SubscribeMode>();
            }            
            Send(id, type, value);
        }
        protected override void Parse(byte[] data, EndPoint senderEP = null)
        {
            string rcvdString = Encoding.UTF8.GetString(data);
            string[] strs = rcvdString.Split('|');

            switch (strs[0])
            {
                case "subscli":    // subscribe client
                    {
                        int id = Property.NameToId(strs[1]);
                        Type type = StringToType(strs[2]);
                        SubscribeMode subsMode = StringToSubscribeMode(strs[3]);

                        if (!_propMap.ContainsKey(id)) {
                            _propMap[id] = new Dictionary<EndPoint, SubscribeMode>();
                        }
                        _propMap[id][senderEP] = subsMode;
                        OnSubscribe.Invoke(senderEP, subsMode, strs[1]);
                    }
                    break;

                case "unscli":    // subscribe client
                    {
                        int id = Property.NameToId(strs[1]);
                        if (_propMap[id] != null) {                            
                            SubscribeMode subsMode = _propMap[id][senderEP];
                            OnUnsubscribe.Invoke(senderEP, subsMode, strs[1]);
                        }
                    }
                    break;

                case "setprop":     // set property
                    {
                        Type type = StringToType(strs[2]);
                        object value = StringToValue(type, strs[3]);
                        SetProperty(strs[1], type, (double)value);
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
