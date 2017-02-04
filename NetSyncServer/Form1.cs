using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ThunderSync;

namespace ThunderSync
{
    public partial class Form1 : Form
    {
        ThunderSyncServer s = null;
        public Form1()
        {
            InitializeComponent();
            s = new ThunderSyncServer();
            s.OnSubscribe = new SubscribeEventHandler(HandleSubscribe);
            s.OnUnsubscribe = new UnSubscribeEventHandler(HandleUnsubscribe);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(button1.Text == "Start")
            {
                s.Start();
                button1.Text = "Stop";
            }
            else 
            {
                s.Stop();
                button1.Text = "Start";
            }
        }
        void HandleSubscribe(EndPoint senderEP, SubscribeMode subMode, string name)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(() => HandleSubscribe(senderEP, subMode, name)));
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                string key = senderEP.ToString();
                ListViewItem li = new ListViewItem();
                listView1.Items.Add(li);
                li.Text = key;
                ListViewItem.ListViewSubItem si = new ListViewItem.ListViewSubItem();
                si.Text = subMode.ToString();
                li.SubItems.Add(si);
                li.SubItems.Add(name);
                label4.Text = string.Format("{0} Subscriptions", listView1.Items.Count);
            }            
        }
        void HandleUnsubscribe(EndPoint senderEP, SubscribeMode subMode, string name)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(() => HandleUnsubscribe(senderEP, subMode, name)));
                }
                catch (Exception ex)
                {
                }
            }
            else
            {
                string key = senderEP.ToString();
                foreach (ListViewItem li in listView1.Items)
                {
                    if (li.Text == key && li.SubItems[2].Text == name)
                    {
                        listView1.Items.Remove(li);
                    }
                }
                label4.Text = string.Format("{0} Subscriptions", listView1.Items.Count);
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            s.Stop();
            s.Close();
        }
    }
}
