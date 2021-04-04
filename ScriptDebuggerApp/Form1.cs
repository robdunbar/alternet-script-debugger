using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Alternet.Common;
using Alternet.Editor.TextSource;
using Alternet.Scripter;
using Alternet.Scripter.Debugger;
using Alternet.Scripter.Integration;
using Alternet.Syntax.Parsers.Roslyn;
using Alternet.Syntax.Parsers.Roslyn.CodeCompletion;

namespace ScriptDebuggerApp
{
    public sealed partial class Form1 : Form
    {
        private readonly string _sourceFile;
        private readonly string _pdbFile;
        private readonly string _dllFile;
        private readonly string _buildDir;
        private readonly ScriptDebugger _debugger;
        private readonly ScriptRun _scriptRun;
        private readonly CsParser _csParser;
        private DebugCodeEditContainer _codeEditContainer;
        private DebuggerPanelsTabControl _debuggerPanels;
        private Process _testAppProcess;

        public Form1()
        {
            InitializeComponent();

            Load += Form1_Load;
            Text = "ScriptDebuggerApp";

            // Setup layout
            SetupFormLayout();
            
            // Set up paths to files that our application gives us.
            var solutionDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(Form1).Assembly.Location), @"..\..\..\.."));
            _buildDir = Path.Combine(solutionDir, @"TestApp\bin\Debug\netcoreapp3.1");
            _sourceFile = Path.Combine(solutionDir, @"TestLib\MyClass.cs");
            _pdbFile = Path.Combine(_buildDir, "TestLib.pdb");
            _dllFile = Path.Combine(_buildDir, "TestLib.dll");
            
            // CS Parser
            _csParser = new CsParser(new CsSolution());
            _csParser.Repository.RegisterDefaultAssemblies(TechnologyEnvironment.System);
            // Workaround to initialize workspace before being displayed so that references can be added before load complete.
            // See https://www.alternetsoft.com/ForumRetrieve.aspx?ForumID=4089&TopicID=68576
            var workspace = _csParser.Repository.Solution.Workspace;

            // Add reference to new dll.
            _csParser.Repository.AddFileReference(_dllFile);

            TextSource csharpSource = new TextSource();
            csharpSource.Lexer = _csParser;
            csharpSource.HighlightReferences = true;

            // Set up debug editor
            _codeEditContainer.EditorRequested += EditorContainer_EditorRequested;
            _codeEditContainer.TryActivateEditor(_sourceFile);

            _scriptRun = new ScriptRun(this.components);
            _scriptRun.AssemblyKind = ScriptAssemblyKind.DynamicLibrary;
            _scriptRun.ScriptSource.FromScriptFile(_sourceFile);
            _scriptRun.ScriptMode = ScriptMode.Debug;
            _debugger = new ScriptDebugger
                {
                    ScriptRun = _scriptRun
                };
            _debuggerPanels.Debugger = _debugger;
            
            var controller = new DebuggerUIController(this, _codeEditContainer);
            controller.Debugger = _debugger;
            controller.DebuggerPanels = _debuggerPanels;
            _codeEditContainer.Debugger = _debugger;

            // Start the TestApp
            _testAppProcess = Process.Start(Path.Combine(_buildDir, "TestApp.exe"));
            FormClosed += (s, e) => { _testAppProcess.CloseMainWindow(); };
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _csParser.ReparseText();
        }

        private void BtnStartDebugging_Click(object sender, EventArgs e)
        {
            if(_debugger.IsStarted)
            {
                _debugger.Continue();
            }
            else
            {
                if (_debugger.State == DebuggerState.Startup)
                    return;
                
                _debugger.GeneratedModulesPath = _buildDir;
                _debugger.AttachToProcessAsync(_testAppProcess.Id,
                                               new StartDebuggingOptions
                                                   {
                                                       MyCodeModules = new[] { _dllFile }
                                                   });
            }
        }

        private void EditorContainer_EditorRequested(object sender, DebugEditRequestedEventArgs e)
        {
            var edit = new DebugCodeEdit();
            edit.LoadFile(e.FileName);
            edit.Lexer = _csParser;
            
            e.DebugEdit = edit;
        }
        
        private void SetupFormLayout()
        {
            var btnStartDebugging = new Button { Dock = DockStyle.Top, Text = "Start Debugging" };
            btnStartDebugging.Click += BtnStartDebugging_Click;
            Controls.Add(btnStartDebugging);

            var splitter = new SplitContainer();
            splitter.Dock = DockStyle.Fill;
            splitter.Orientation = Orientation.Horizontal;
            splitter.SplitterDistance = 400;

            var editTabControl = new TabControl { Dock = DockStyle.Fill };
            splitter.Panel1.Controls.Add(editTabControl);

            _codeEditContainer = new DebugCodeEditContainer(editTabControl);

            _debuggerPanels = new DebuggerPanelsTabControl { Dock = DockStyle.Fill };
            splitter.Panel2.Controls.Add(_debuggerPanels);

            Controls.Add(splitter);
        }
    }
}
