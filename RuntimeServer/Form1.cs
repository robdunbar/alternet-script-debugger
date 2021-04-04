using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyNamespace;

namespace RuntimeServer
{
    public sealed partial class Form1 : Form
    {
        private FileInfo _logFile;
        private CancellationTokenSource _ct;

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

            var btnRunOnce = new Button
                {
                    Text = "Run Once",
                    Dock = DockStyle.Top
                };
            btnRunOnce.Click += BtnRunOnce_Click;
            Controls.Add(btnRunOnce);

            var btnToggleContinuousRun = new Button
                {
                    Text = "Run Continuously",
                    Dock = DockStyle.Top
                };
            btnToggleContinuousRun.Click += (s, e) => BtnToggleContinuousRun_Click(btnToggleContinuousRun);
            Controls.Add(btnToggleContinuousRun);

            _logFile = new FileInfo("log.txt");
            var lblLogLocation = new TextBox
                {
                    Dock = DockStyle.Top,
                    ReadOnly = true,
                    Text = _logFile.FullName,
                };
            lblLogLocation.DoubleClick += (s, e) => { Process.Start("explorer.exe", _logFile.FullName); };
            Controls.Add(lblLogLocation);

            var lblProcessId = new TextBox
            {
                Dock = DockStyle.Top,
                ReadOnly = true,
                Text = Process.GetCurrentProcess().Id.ToString()
            };
            Controls.Add(lblProcessId);
        }

        private void BtnToggleContinuousRun_Click(Button button)
        {
            if (_ct == null)
            {
                button.Text = "Stop Continuous Run";
                _ct = new CancellationTokenSource();
                var ct = _ct;
                Task.Run(() =>
                    {
                        while (!ct.IsCancellationRequested)
                        {
                            BtnRunOnce_Click(button, EventArgs.Empty);
                            Thread.Sleep(2000);
                        }
                    });
            }
            else
            {
                _ct.Cancel();
                _ct = null;
                button.Text = "Run Continuously";
            }
        }

        private void BtnRunOnce_Click(object sender, EventArgs e)
        {
            var message = new MyClass().Execute();
            // Write to file to check that this thread is still running.
            File.AppendAllText("log.txt", message + Environment.NewLine);
        }
    }
}
