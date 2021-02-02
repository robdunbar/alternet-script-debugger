using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyNamespace;

namespace TestApp
{
    public sealed partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            Text = "TestApp";

            var output = new TextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Multiline = true
                };
            Controls.Add(output);
            Trace.Listeners.Add(new TextBoxWriter(output));

            var button = new Button
                {
                    Text = "Run",
                    Dock = DockStyle.Top
                };
            button.Click += Button_Click;
            Controls.Add(button);

            var lblProcessId = new TextBox
            {
                Dock = DockStyle.Top,
                ReadOnly = true,
                Text = Process.GetCurrentProcess().Id.ToString()
            };
            Controls.Add(lblProcessId);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            new MyClass().Execute();
        }
    }
}
