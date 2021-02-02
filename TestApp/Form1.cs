using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyNamespace;

namespace TestApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var button = new Button
                {
                    Text = "Run",
                    Dock = DockStyle.Top
                };
            button.Click += Button_Click;
            Controls.Add(button);

            var label = new TextBox
            {
                Dock = DockStyle.Top,
                ReadOnly = true,
                Text = Process.GetCurrentProcess().Id.ToString()
            };
            Controls.Add(label);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            new MyClass().Execute();
        }
    }
}
