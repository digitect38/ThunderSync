using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Type t = typeof(int);
            int s = Marshal.SizeOf(t);
        }
    }

    public class GenericClass<T>
    where T : struct
    {
        T _value;

        public void SetValue(T value)
        {
            this._value = value;
        }

        public byte[] GetBytes()
        {
            if (typeof(T) == typeof(int))
            {
                return BitConverter.GetBytes((int)(object)_value);
            }
            else if (typeof(T) == typeof(double))
            {
                return BitConverter.GetBytes((double)(object)_value);
            }
            else if (typeof(T) == typeof(float))
            {
                return BitConverter.GetBytes((float)(object)_value);
            }
            else
            {
                return null;
            }
        }
    }
}
