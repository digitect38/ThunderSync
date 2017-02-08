using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ThunderSync
{
    public partial class ClientForm : Form
    {
        ThunderSyncServer server = null;

        ThunderSyncTextClient[] client = new ThunderSyncTextClient[25];

        public ClientForm()
        {
            InitializeComponent();
#if ACTIVATE_BUILTIN_SERVER
            server = new ThunderSyncServer();
            server.Start();
#endif
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void HandleValueChange(Property prop, Control windowsControl)
        {
            if (InvokeRequired) {
                try {
                    Invoke(new Action(() => HandleValueChange(prop, windowsControl)));
                }
                catch(Exception ex) {
                }
            }
            else {
                windowsControl.Text = ThunderSyncBase.ValueToString(prop.Type, prop.Value);
            }
        }

        private void Start()
        {
            int i = 0;

            foreach (ThunderSyncClient c in client)
            {
                client[i] = new ThunderSyncTextClient(comboBox1.Text);
                client[i].Start();
                i++;
            }

            Subscribe();
            timer1.Enabled = true;
        }

        private void Stop()
        {
            int i = 0;

            timer1.Enabled = false;
            UnSubscribe();

            foreach (ThunderSyncClient c in client)
            {
                client[i].Stop();
                i++;
            }            
        }

        private void UnSubscribe()
        {
            int i = 0;
            foreach (ThunderSyncClient c in client)
            {
                client[i].UnSubscribe("volume");
                client[i].UnSubscribe("pressure");
                i++;
            }
        }
        private void ReSubscribe()
        {
            UnSubscribe();
            Subscribe();
        }
        private void Subscribe()
        {

            client[0].Subscribe("volume", typeof(double), SubscribeMode.Producer);
            client[0].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox2);

            client[1].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox3);
            client[1].Subscribe("pressure", typeof(double), SubscribeMode.Producer);

            client[2].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox5);
            client[2].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox6);

            client[3].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox7);
            client[3].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox8);

            client[4].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox9);
            client[4].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox10);

            client[5].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox11);
            client[5].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox12);

            client[6].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox13);
            client[6].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox14);

            client[7].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox15);
            client[7].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox16);

            client[8].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox17);
            client[8].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox18);

            client[9].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox19);
            client[9].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox20);

            client[10].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox21);
            client[10].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox22);

            client[11].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox23);
            client[11].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox24);

            client[12].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox25);
            client[12].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox26);

            client[13].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox27);
            client[13].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox28);

            client[14].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox29);
            client[14].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox30);

            client[15].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox31);
            client[15].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox32);

            client[16].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox33);
            client[16].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox34);

            client[17].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox35);
            client[17].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox36);

            client[18].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox37);
            client[18].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox38);

            client[19].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox39);
            client[19].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox40);

            client[20].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox41);
            client[20].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox42);

            client[21].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox43);
            client[21].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox44);

            client[22].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox45);
            client[22].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox46);

            client[23].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox47);
            client[23].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox48);

            client[24].Subscribe("volume", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox49);
            client[24].Subscribe("pressure", typeof(double), SubscribeMode.Consumer, HandleValueChange, textBox50);            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReSubscribe();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer1.Enabled = false;
            foreach (ThunderSyncClient c in client) {
                c?.Stop();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //client[0].SetProperty("volume", typeof(double), ThunderSyncBase.StringToValue(typeof(double), (sender as Control).Text));
            client[0].SetPropertyValue("volume", ThunderSyncBase.StringToValue(typeof(double), (sender as Control).Text));
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            client[1].SetPropertyValue("pressure", ThunderSyncBase.StringToValue(typeof(double), (sender as Control).Text));
        }

        Random r1 = new Random();
        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox1.Text = r1.Next().ToString();
            textBox4.Text = r1.Next().ToString();
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            if (btStart.Text == "Start")
            {
                btStart.Text = "Stop";
                Start();

            }
            else
            {
                btStart.Text = "Start";
                Stop();
            }
        }
    }
}
