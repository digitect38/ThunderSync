////////////////////////////////////////////////////////////////////////////////////////////
//
// ThunderSync : The ultra fast data syncronizer
// digitect38@gmail.com, by Sang Jeong Woo / Seoul /South Korea
//
////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ThunderSync
{
    /*
    public interface IThunderSync
    {
        void RegisterProperty(string Name, Type type);
        void SetProperty(string Name, Type type, object value);
        object GetProperty(string Name);
    }
    */
    public enum Mode
    {
        Client,
        Server
    }

    public delegate void ValueChangeHandler(Property prop, Control winControl);

    public class Property
    {
        static Dictionary<int, string> _id2NameMap    = new Dictionary<int, string>();
        static Dictionary<string, int> _name2IdDict = new Dictionary<string, int>(); // Reverse dict for fast conversion
        public ValueChangeHandler OnValueChange;
        public int ID
        {
            set;get;
        }
        public string Name
        {
            set; get;
        }
        public Type Type
        {
            set;get;
        }
        public object Value
        {
            set;get;
        }
        public SubscribeMode SubscribeMode
        {
            set;get;
        }      
        public ProtocolMode ProtocolMode
        {
            set;get;
        }

        public Control WindowsControl
        {
            set;get;
        }

        public static void Initialize()
        {

            // read from property table describes property ID and Name Pair
            // property table file should be looks like ...
            // --,-------------,--------
            // ID, Name        , Type
            // --,-------------,--------
            //  1, Recipe.Name , string
            //  2, Platen.RPM  , int
            //  3, DAQ.bitrate , int
            //  4, DAQ.Model   , string 
            //  5, 

            //todo: Initialize _propDict and _propDictRev both here.
            //  It simalar to folowing
            //
            //  while(true) {
            //      line = File.ReadLine();
            //      string [] token = line.Split(',');
            //      _propDict[Conver.ToInt32(token[0])] = token[1];
            // }
            _id2NameMap[1] = "volume";
            _name2IdDict["volume"] = 1;

            _id2NameMap[2] = "pressure";
            _name2IdDict["pressure"] = 2;

        }

        public static string IdToName(int id)
        {
            return _id2NameMap[id];
        }

        public static int NameToId(string name)
        {
            return _name2IdDict[name];
        }

        public Property(int id, Type type, SubscribeMode subsMode, ProtocolMode protoMode, ValueChangeHandler onValueChange = null, Control winControl = null)
        {
            ID = id;
            Name = IdToName(id);
            Type = type;
            SubscribeMode = subsMode;
            ProtocolMode = protoMode;
            OnValueChange = onValueChange;
            WindowsControl = winControl;
        }

        public void HandleValueChange()
        {
            OnValueChange.Invoke(this, WindowsControl);
        }
    }    

    public enum Message {
        Subscribe,
        UnSubscribe,
        ResisterProperty,
        SetProperty
    }

    enum TypeEnum
    {
        Undefined = -1,
        Byte = 1,
        Short,
        Int,
        Long,
        Float,
        Double,
        String
    }

    /// <summary>
    /// NetSyncBase
    /// </summary>
    abstract public class ThunderSyncBase
    {
        protected Dictionary<int, Property> _id2PropMap;
        protected bool _close = false;
        protected const int SERVER_PORT = 9051;

        Socket _socket = null;

        Thread _commThread;

        public Socket Socket
        {
            get { return _socket; }
        }
        protected ThunderSyncBase()
        {
            Property.Initialize();
            _id2PropMap = new Dictionary<int, Property>();
        }
        protected abstract void InitSocket();
        protected void Send(EndPoint ep, string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            _socket.SendTo(data, data.Length, SocketFlags.None, ep);
        }
        protected void Send(EndPoint ep, byte [] data)
        {
            _socket.SendTo(data, data.Length, SocketFlags.None, ep);
        }
        //public abstract void SetProperty(string name, Type propType, object value);
        public abstract void SetPropertyValue(string name, object value);
        protected abstract void Parse(byte[] rcvdBytes, EndPoint endPoint = null);
        protected abstract void CommThreadStartFunc();        
        enum StartState
        {
            Started,
            Stopped
        }
        StartState _startState = StartState.Stopped;
                
        public void Start()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            InitSocket();
            _commThread = new Thread(CommThreadStartFunc);
            _commThread.Start();
            _startState = StartState.Started;
        }

        public virtual void Stop()
        {
            if (_startState == StartState.Started)
            {
                _close = true;
                _socket.Close();
                _commThread.Abort();
            }
        }

        static public byte TypeToByte(Type type)
        {
            byte b;
            if (type == typeof(byte))           b = (byte)TypeEnum.Byte;
            else if (type == typeof(short))     b = (byte)TypeEnum.Short;
            else if (type == typeof(int))       b = (byte)TypeEnum.Int;
            else if (type == typeof(long))      b = (byte)TypeEnum.Long;
            else if (type == typeof(float))     b = (byte)TypeEnum.Float;
            else if (type == typeof(double))    b = (byte)TypeEnum.Double;
            else if (type == typeof(string))    b = (byte)TypeEnum.String;
            else b = (byte)TypeEnum.String;
            return b;
        }

        static public string ValueToString(Type type, object value)
        {
            string str = null;
            if (type == typeof(int))
            {
                str += ((int)value).ToString();
            }
            else if (type == typeof(float))
            {
                str += ((float)value).ToString();
            }
            else if (type == typeof(double))
            {
                str += ((double)value).ToString();
            }
            return str;
        }

        static public object StringToValue(Type type, string str)
        {
            object obj = null;

            if (type == typeof(int))
            {
                obj = Convert.ToInt32(str == "" ? "0" : str);
            }
            else if (type == typeof(double))
            {
                obj = Convert.ToDouble(str== "" ? "0.0":str);
            }

            return obj;
        }
        public void Close()
        {
            _close = true;
        }
        protected Type StringToType(string str)
        {
            Type type = null;

            if (typeof(int).ToString() == str)
                type = typeof(int);
            else if (typeof(float).ToString() == str)
                type = typeof(float);
            else if (typeof(double).ToString() == str)
                type = typeof(double);
            else
                type = typeof(object);  // unknown

            return type;
        }
        static public SubscribeMode StringToSubscribeMode(string str)
        {
            SubscribeMode smode = SubscribeMode.UnSubscribed;

            if (SubscribeMode.Consumer.ToString() == str)
                smode = SubscribeMode.Consumer;
            else if (SubscribeMode.Producer.ToString() == str)
                smode = SubscribeMode.Producer;
            else
                smode = SubscribeMode.UnSubscribed;

            return smode;
        }
        static public ProtocolMode StringToProtocolMode(string str)
        {
            ProtocolMode pmode = ProtocolMode.UnDefined;

            if (ProtocolMode.Binary.ToString() == str)
                pmode = ProtocolMode.Binary;
            else if (ProtocolMode.Text.ToString() == str)
                pmode = ProtocolMode.Text;
            else
                pmode = ProtocolMode.UnDefined;

            return pmode;
        }
    }
    public enum SubscribeMode
    {
        UnSubscribed  = 0,
        Producer = 1,
        Consumer = 2,
        Both = 3
    }

    public enum ProtocolMode
    {
        UnDefined = 0,
        Binary = 1,
        Text = 2
    }
    public abstract class ThunderSyncClient : ThunderSyncBase
    {
        private string _serverIP;     
        protected Dictionary<int, AutoResetEvent> _id2EventMap;
        protected IPEndPoint _serverEP;
        public ThunderSyncClient(string serverIP) : base()
        {
            _serverIP = serverIP;
            _id2EventMap = new Dictionary<int, AutoResetEvent>();
        }
        protected override void InitSocket()
        {
            if(_serverIP == null || _serverIP == "")
            {
                throw new Exception("Invalid Server IP");
            }
            _serverEP = new IPEndPoint(IPAddress.Parse(_serverIP), SERVER_PORT);
        }
        public abstract void Subscribe(string name, Type type, SubscribeMode subsMode, ValueChangeHandler OnValueChange = null, Control WindowsControl = null);
        public abstract void UnSubscribe(string name);

        protected void Send(string str)
        {
            Send(_serverEP, str);
        }
        protected void Send(byte [] data)
        {
            Send(_serverEP, data);
        }
        public void UnSubscribeAll()
        {
            foreach (Property p in _id2PropMap.Values)
            {

                int id = Property.NameToId(p.Name);

                if (_id2PropMap.ContainsKey(id))
                {
                    string str = "unscli|" + p.Name + "|";
                    Send(str);
                }
            }

            _id2PropMap.Clear();
        }
        public override void Stop()
        {            
            UnSubscribeAll();
            base.Stop();
        }
        protected override void CommThreadStartFunc()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint remoteEP = (EndPoint)sender;

            int recv = 0;
            byte[] data = new byte[1024];

            while (!_close)
            {
                try
                {
                    recv = Socket.Receive(data);
                }
                catch (Exception ex)
                {

                }
                if (recv > 0)
                {
                    Parse(data);
                }
            }
        }
    }
    public class ThunderSyncTextClient : ThunderSyncClient
    {
        public ThunderSyncTextClient(string serverIP) : base(serverIP)
        {
        }
        public override void Subscribe(string name, Type type, SubscribeMode subsMode, ValueChangeHandler OnValueChange = null, Control WindowsControl = null)
        {
            int id =  Property.NameToId(name);
            string pmode = ProtocolMode.Text.ToString();
            string str = "subscli|" + name + "|" + type.ToString() + "|" + subsMode.ToString() + "|" + pmode + "|";
            Send(str);
            _id2PropMap[id] = new Property(id, type, subsMode, ProtocolMode.Text, OnValueChange, WindowsControl);
        }               
        public override void UnSubscribe(string name)
        {
            int id = Property.NameToId(name);

            if (_id2PropMap.ContainsKey(id))
            {
                string str = "unscli|" + name + "|";
                _id2PropMap.Remove(id);
                Send(str);
            }
        }
        //public override void SetProperty(string name, Type type, object value)
        public override void SetPropertyValue(string name, object value)
        {
            int id = Property.NameToId(name);

            if (_id2PropMap.ContainsKey(id))
            {
                _id2PropMap[id].Value = value;
                //string str = "setprop|" + name + "|" + type.ToString() + "|" + ValueToString(type, value) + "|";
                Type type = _id2PropMap[id].Type;
                string str = "setprop|" + name + "|" + ValueToString(type, value) + "|";
                if (_id2PropMap[id].SubscribeMode == SubscribeMode.Producer)
                {
                    Send(str);
                }
            }
        }      
        protected override void Parse(byte [] data, EndPoint endPoint = null)
        {
            string rcvdString = Encoding.UTF8.GetString(data);
            string[] strs = rcvdString.Split('|');

            int id = Property.NameToId(strs[1]);

            switch (strs[0])
            {
                case "regprop":
                    {
                        Subscribe(strs[1], StringToType(strs[2]), StringToSubscribeMode(strs[3]));
                    }
                    break;

                case "setprop":
                    {

                        if (_id2PropMap.ContainsKey(id))
                        {
                            Type type = _id2PropMap[id].Type;
                            SetPropertyValue(strs[1], StringToValue(type, strs[2]));
                            _id2PropMap[id].HandleValueChange();
                        }
                    }
                    break;
            }
        }        
    }
    public class ThunderSyncBinaryClient : ThunderSyncClient
    {
        public ThunderSyncBinaryClient(string serverIP) : base(serverIP)
        {
        }        
        public override void Subscribe(string name, Type type, SubscribeMode subsMode, ValueChangeHandler OnValueChange = null, Control WindowsControl = null)
        {
            int id = Property.NameToId(name);
            byte[] data = new byte[] {
                (byte)id,
                (byte)TypeToByte(type),
                (byte)ProtocolMode.Binary,
            };
            Send(data);
            _id2PropMap[id] = new Property(id, type, subsMode, ProtocolMode.Binary, OnValueChange, WindowsControl);
        }
        public override void UnSubscribe(string name)
        {
            int id = Property.NameToId(name);
            byte[] data = new byte[] {
                (byte)Message.UnSubscribe,
                (byte)id,
            };
            Send(data);
        }

        //public override void SetProperty(string name, Type type, object value)
        public override void SetPropertyValue(string name, object value)
        {
            int id = Property.NameToId(name);

            if (_id2PropMap.ContainsKey(id))
            {
                _id2PropMap[id].Value = value;

                //string str = "setprop|" + name + "|" + type.ToString() + "|" + ValueToString(type, value) + "|";
                Type type = _id2PropMap[id].Type;
                string str = "setprop|" + name + "|" + ValueToString(type, value) + "|";
                if (_id2PropMap[id].SubscribeMode == SubscribeMode.Producer)
                {
                    Send(str);
                }
            }
        }
        protected override void Parse(byte[] data, EndPoint endPoint = null)
        {
            // todo
        }
    }
}