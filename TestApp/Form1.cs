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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

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

    /// <summary>
    /// Used to redirect Trace output to a TextBox.
    /// </summary>
    public class TextBoxWriter : TextWriterTraceListener
    {
        private readonly TextBox _textBox;

        public TextBoxWriter(TextBox textBox)
        {
            _textBox = textBox;
        }

        #region Overrides of TextWriterTraceListener

        /// <inheritdoc />
        public override void WriteLine(string message)
        {
            base.WriteLine(message);
            AddText(message + Environment.NewLine);
        }

        private void AddText(string message)
        {
            _textBox.BeginInvoke((Action)(() =>
                                                 {
                                                     _textBox.Text = message + _textBox.Text;
                                                 }));
        }

        #endregion
    }
}
