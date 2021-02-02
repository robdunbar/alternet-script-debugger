using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Alternet.Common;
using Alternet.Editor;
using Alternet.Editor.TextSource;
using Alternet.Scripter;
using Alternet.Scripter.Debugger;
using Alternet.Scripter.Integration;
using Alternet.Syntax.Parsers.Roslyn;
using Alternet.Syntax.Parsers.Roslyn.CodeCompletion;

namespace CoreFormsApp
{
    public partial class Form1 : Form
    {
        public string _solutionDir { get; } = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(Form1).Assembly.Location), @"..\..\..\.."));

        private CsParser _csParser;
        private DebugCodeEdit _editor;
        private ScriptDebugger _debugger;
        private TextBox _txtProcessId;
        private ScriptRun _scriptRun;

        public Form1()
        {
            InitializeComponent();

            var btnAttachAndDebug = new Button()
            {
                Dock = DockStyle.Top,
                Text = "Attach and Start Debugging"
            };
            btnAttachAndDebug.Click += BtnAttach_Click;
            Controls.Add(btnAttachAndDebug);

            _txtProcessId = new TextBox
            {
                Dock = DockStyle.Top,
                Text = "..."
            };
            Controls.Add(_txtProcessId);

            FormClosing += Form1_FormClosing;



            _csParser = new CsParser(new CsSolution());
            _csParser.Repository.RegisterDefaultAssemblies(TechnologyEnvironment.System);
            // Workaround to initialize workspace before being displayed so that references can be added properly.
            // See https://www.alternetsoft.com/ForumRetrieve.aspx?ForumID=4089&TopicID=68576
            var workspace = _csParser.Repository.Solution.Workspace;

            TextSource csharpSource = new TextSource();
            csharpSource.Lexer = _csParser;
            csharpSource.HighlightReferences = true;

            _editor = new DebugCodeEdit();
            _debugger = new ScriptDebugger();
            _debugger.GeneratedModulesPath = Path.Combine(_solutionDir, @"TestApp\bin\Debug\netcoreapp3.1");
            _debugger.MyCodeModules = new string[]
            {
                Path.Combine(_debugger.GeneratedModulesPath, @"TestLib.dll")
            };

            _editor.Debugger = _debugger;
            _editor.Source = csharpSource;
            _editor.Dock = DockStyle.Fill;
            _editor.Gutter.Options |= GutterOptions.PaintLineNumbers;

            _editor.Spelling.SpellColor = Color.Navy;
            _editor.Outlining.AllowOutlining = true;
            _editor.DisplayLines.AllowHiddenLines = true;

            Controls.Add(_editor);

            // Start up the TestApp that will run the TestLib's code.
            var testAppExe = Path.Combine(_solutionDir, @"TestApp\bin\Debug\netcoreapp3.1\TestApp.exe");
            var testApp = Process.Start(testAppExe);
            _txtProcessId.Text = testApp.Id.ToString();
            System.Threading.Thread.Sleep(500);

            // Simulate the real application's use of NO source code files.
            var scriptToLoad = Path.Combine(_solutionDir, @"TestLib\MyClass.cs");
            string scriptSource = File.ReadAllText(scriptToLoad);
            _editor.Text = scriptSource;
            _csParser.ReparseText();

            // Simulate that the real application needs to compile the library itself and cannot use the ScriptRun object...
            //_scriptRun = new ScriptRun(this.components);
            //_scriptRun.ScriptLanguage = ScriptLanguage.CSharp;
            //_scriptRun.ScriptMode = ScriptMode.Debug;
            //_scriptRun.ScriptSource.WithDefaultReferences();
            //_scriptRun.ScriptSource.FromScriptCode(ScriptText);

            //var testLibDll = Path.Combine(_solutionDir, @"TestApp\bin\Debug\netcoreapp3.1\TestLib.dll");
            //_scriptRun.ScriptHost.AssemblyFileName = testLibDll;
            //_scriptRun.ScriptHost.ModulesDirectoryPath = Path.Combine(_solutionDir, @"TestApp\bin\Debug\netcoreapp3.1");
            //_scriptRun.AssemblyKind = ScriptAssemblyKind.DynamicLibrary;
            //_debugger.ScriptRun = _scriptRun;
        }

        private async void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            await _debugger.StopDebuggingAsync();
        }

        private async void BtnAttach_Click(object sender, EventArgs e)
        {
            await _debugger.AttachToProcessAsync(int.Parse(_txtProcessId.Text));
            _debugger.StartDebugging();
        }
    }
}
