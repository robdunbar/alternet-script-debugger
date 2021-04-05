using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RuntimeServer
{
    public sealed partial class Form1 : Form
    {
        private FileInfo _logFile;
        private CancellationTokenSource _ct;
        private readonly string _solutionDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(Form1).Assembly.Location), @"..\..\..\.."));
        private WeakReference _weakRefToContext;
        private Func<string> _actionToRun;

        public Form1()
        {
            InitializeComponent();

            Text = "TestApp";

            SetupLayout();
        }

        private void BtnCompileScript_Click(object sender, EventArgs e)
        {
            // If context already exists, then unload it.
            // https://docs.microsoft.com/en-us/dotnet/standard/assembly/unloadability
            if (_weakRefToContext != null)
            {
                _actionToRun = null;

                for (int i = 0; _weakRefToContext.IsAlive && (i < 10); i++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }

                Debug.Assert(_weakRefToContext.IsAlive == false);
                _weakRefToContext = null;
            }

            // Compile the script to a dll
            var projectFilePath = Path.Combine(_solutionDir, "UserScriptLib", "UserScriptLib.csproj");
            var compileProcess = Process.Start("dotnet", "build " + projectFilePath);
            compileProcess.WaitForExit();

            // Create new context for the plugin to be loaded into.
            var alc = new AssemblyLoadContext("PluginContext", isCollectible: true);
            _weakRefToContext = new WeakReference(alc, trackResurrection: true);
            var userScriptDll = Path.Combine(_solutionDir, @"UserScriptLib\bin\Debug\netcoreapp3.1\UserScriptLib.dll");
            var userScriptAssembly = alc.LoadFromAssemblyPath(userScriptDll);
            var typeToExecute = userScriptAssembly.GetTypes().FirstOrDefault(t => t.Name == "MyClass");
            var methodToInvoke = typeToExecute.GetMethod("Execute");
            var objectToActOn = Activator.CreateInstance(typeToExecute);
            _actionToRun = () => methodToInvoke.Invoke(objectToActOn, null) as string;
        }

        private void BtnRunOnce_Click(object sender, EventArgs e)
        {
            if (_actionToRun == null)
            {
                Trace.WriteLine("Script hasn't been compiled yet.");
                return;
            }

            var message = _actionToRun.Invoke();
            // Write to file to check that this thread is still running.
            File.AppendAllText("log.txt", message + Environment.NewLine);
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

        private void SetupLayout()
        {
            var output = new TextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Multiline = true
                };
            Controls.Add(output);
            Trace.Listeners.Add(new TextBoxWriter(output));

            var btnToggleContinuousRun = new Button
                {
                    Text = "Run Continuously",
                    Dock = DockStyle.Top
                };
            btnToggleContinuousRun.Click += (s, e) => BtnToggleContinuousRun_Click(btnToggleContinuousRun);
            Controls.Add(btnToggleContinuousRun);

            var btnRunOnce = new Button
                {
                    Text = "Run Once",
                    Dock = DockStyle.Top
                };
            btnRunOnce.Click += BtnRunOnce_Click;
            Controls.Add(btnRunOnce);

            var btnCompileScript = new Button
                {
                    Text = "Compile script and load context",
                    Dock = DockStyle.Top
                };
            btnCompileScript.Click += BtnCompileScript_Click;
            Controls.Add(btnCompileScript);

            _logFile = new FileInfo("log.txt");
            var btnOpenLog = new Button
                {
                    Dock = DockStyle.Bottom,
                    Text = "Open Log File - " + _logFile.FullName
                };
            btnOpenLog.Click += (s, e) => { Process.Start("explorer.exe", _logFile.FullName); };
            Controls.Add(btnOpenLog);

            var lblProcessId = new TextBox
                {
                    Dock = DockStyle.Top,
                    ReadOnly = true,
                    Text = Process.GetCurrentProcess().Id.ToString()
                };
            Controls.Add(lblProcessId);
        }
    }
}
