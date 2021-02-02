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

            // Add Buttons
            AddButtons();

            // Set up syntax highlighting and code completion backend
            _csParser = new CsParser(new CsSolution());
            _csParser.Repository.RegisterDefaultAssemblies(TechnologyEnvironment.System);
            // Workaround to initialize workspace before being displayed so that references can be added properly.
            // See https://www.alternetsoft.com/ForumRetrieve.aspx?ForumID=4089&TopicID=68576
            var workspace = _csParser.Repository.Solution.Workspace;

            TextSource csharpSource = new TextSource();
            csharpSource.Lexer = _csParser;
            csharpSource.HighlightReferences = true;

            // Set up debug editor
            _editor = new DebugCodeEdit();
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
        }

        private void BtnLoadScript_Click(object? sender, EventArgs e)
        {
            // Our application does not have source files or projects.
            // Instead we receive only the text and an array of referenced DLLs.
            var references = new string[0]; // Lets keep it simple to start with
            var scriptToLoad = Path.Combine(_solutionDir, @"TestLib\MyClass.cs");
            string scriptSource = File.ReadAllText(scriptToLoad);
            _editor.Text = scriptSource;
            _csParser.ReparseText();
        }

        private void BtnSetupDebugger_Click(object sender, EventArgs e)
        {
            // Set up debugger
            _debugger = new ScriptDebugger();
            _debugger.GeneratedModulesPath = Path.Combine(_solutionDir, @"TestApp\bin\Debug\netcoreapp3.1");
            _debugger.MyCodeModules = new string[]
                {
                    Path.Combine(_debugger.GeneratedModulesPath, @"TestLib.dll")
                };

            _editor.Debugger = _debugger;
        }

        private async void BtnAttach_Click(object sender, EventArgs e)
        {
            await _debugger.AttachToProcessAsync(int.Parse(_txtProcessId.Text), new StartDebuggingOptions { MyCodeModules = new[] { "" } });
            _debugger.StartDebugging();
        }

        private void AddButtons()
        {
            var btnAttachAndDebug = new Button()
                {
                    Dock = DockStyle.Top,
                    Text = "Attach and Start Debugging"
                };
            btnAttachAndDebug.Click += BtnAttach_Click;
            Controls.Add(btnAttachAndDebug);

            var btnSetupDebugger = new Button()
                {
                    Dock = DockStyle.Top,
                    Text = "Set up Debugger"
                };
            btnSetupDebugger.Click += BtnSetupDebugger_Click;
            Controls.Add(btnSetupDebugger);

            var btnLoadScript = new Button()
                {
                    Dock = DockStyle.Top,
                    Text = "Load Script"
                };
            btnLoadScript.Click += BtnLoadScript_Click;
            Controls.Add(btnLoadScript);

            _txtProcessId = new TextBox
                {
                    Dock = DockStyle.Top,
                    Text = "..."
                };
            Controls.Add(_txtProcessId);
        }
    }
}
