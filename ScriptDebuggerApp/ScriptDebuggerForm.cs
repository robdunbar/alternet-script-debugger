// Copyright (c) 2021 Cognex Corporation. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Alternet.Common;
using Alternet.Scripter;
using Alternet.Scripter.Debugger;
using Alternet.Scripter.Debugger.UI;
using Alternet.Scripter.Integration;
using Alternet.Syntax.Parsers.Roslyn;
using Alternet.Syntax.Parsers.Roslyn.CodeCompletion;
using Microsoft.CodeAnalysis;

namespace ScriptDebuggerApp
{
    public sealed partial class ScriptDebuggerForm : Form
    {
        private readonly CsParser _csParser;
        private readonly Process _runtimeServerProcess;
        private DebugCodeEditContainer _codeEditContainer;
        private DebugCodeEdit _currentEditor;
        private ScriptDebugger _debugger;
        private DebuggerPanelsTabControl _debuggerPanels;
        private DebuggerControlToolbar _debuggerToolbar;
        private DebuggerUIController _debuggerUiController;
        private ScriptRun _scriptRun;

        public ScriptDebuggerForm()
        {
            InitializeComponent();

            // Setup layout
            SetupFormLayout();

            // Start the RuntimeServer (it will compile and trigger the script to run)
            _runtimeServerProcess = Process.Start(PathHelper.RuntimeServerExe);
            FormClosed += (s, e) => { _runtimeServerProcess.CloseMainWindow(); };


            // CS Parser
            _csParser = new CsParser(new CsSolution());
            _csParser.Repository.RegisterDefaultAssemblies(TechnologyEnvironment.System);
            // Workaround to initialize workspace before being displayed so that references can be added before load complete.
            // See https://www.alternetsoft.com/ForumRetrieve.aspx?ForumID=4089&TopicID=68576
            var workspace = _csParser.Repository.Solution.Workspace;

            // Set up ScriptRun and ScriptDebugger
            _scriptRun = new ScriptRun(this.components);
            _scriptRun.AssemblyKind = ScriptAssemblyKind.DynamicLibrary;
            _scriptRun.ScriptLanguage = ScriptLanguage.CSharp;
            _scriptRun.ScriptMode = ScriptMode.Debug;
            _scriptRun.ScriptSource.FromScriptFile(PathHelper.UserScriptSourceFile);

            _debugger = new ScriptDebugger
                {
                    ScriptRun = _scriptRun,
                    GeneratedModulesPath = PathHelper.UserScriptBuildDir
                };

            // Wire up UI to ScriptRun and ScriptDebugger
            _codeEditContainer.Debugger = _debugger;
            _debuggerPanels.Debugger = _debugger;
            _debuggerUiController.Debugger = _debugger;
            _debuggerToolbar.Debugger = _debugger;
            _debuggerToolbar.CommandsListener = new AutoAttachDebuggerUICommands(_debugger, _runtimeServerProcess.Id);

            // Set the editor to the source file
            _codeEditContainer.TryActivateEditor(PathHelper.UserScriptSourceFile);
        }

        // TODO Fix add reference to get xml docs to load for intellisense.
        private void BtnAddRef_Click(object sender, EventArgs e)
        {
            var assemblyLocation = PathHelper.PrecompiledLibraryToReferenceDll;
            Debug.Assert(File.Exists(assemblyLocation));
            Debug.Assert(File.Exists(Path.ChangeExtension(assemblyLocation, ".xml")));

            // Add reference to new dll.
            _csParser.Repository.AddFileReference(assemblyLocation);

            // Check that xml docs have loaded into roslyn
            foreach (var project in _csParser.Repository.Solution.Workspace.CurrentSolution.Projects)
            {
                var metaRef = project.MetadataReferences.SingleOrDefault(mr => mr.Display == assemblyLocation);
                var docProviderProperty = metaRef?.GetType().GetProperty("DocumentationProvider", BindingFlags.Instance | BindingFlags.NonPublic);
                var docProvider = docProviderProperty?.GetValue(metaRef) as DocumentationProvider;
                if (docProvider?.GetType().Name == "NullDocumentationProvider")
                {
                    // Update the provider
                    var xmlFile = Path.ChangeExtension(assemblyLocation, ".xml");
                    var xmlDocProvider = XmlDocumentationProvider.CreateFromFile(xmlFile);
                    // TODO Some how update the existing meta reference to use this documentation provider?
                }
            }

            _csParser.ForceReparseText();
        }

        private async void BtnAttachDebugger_Click(object sender, EventArgs e)
        {
            if (_debugger.State == DebuggerState.Startup)
                return;

            var button = FindButton(sender as Control);

            if (_debugger.State == DebuggerState.Off)
            {
                button.Text = "Attaching...";
                button.Enabled = false;
                await _debugger.AttachToProcessAsync(_runtimeServerProcess.Id,
                                                     new StartDebuggingOptions
                                                         {
                                                             MyCodeModules = new[] { PathHelper.UserScriptDllFile }
                                                         });

                button.Text = "Attached :)";
                _debugger.DebuggingStopped += (s1, e1) =>
                    {
                        button.Invoke((Action)(() =>
                                                      {
                                                          button.Text = "Attach";
                                                          button.Enabled = true;
                                                      }));
                    };
            }
        }

        private void BtnSendCompileCommandToServer_Click(object sender, EventArgs e)
        {
            string message = "Still need to automate this. But manually please do the following: " + Environment.NewLine;

            if (_debugger.IsStarted)
            {
                message += "Please stop the debugging session. " + Environment.NewLine;
            }

            if (_debugger.ScriptRun.ScriptSource.Changed)
            {
                File.WriteAllText(PathHelper.UserScriptSourceFile, _currentEditor.Text);
                message += "Please press the 'Compile script and load context' button on the RuntimeServer application.";
            }

            MessageBox.Show(message);
        }
        
        private void EditorContainer_EditorRequested(object sender, DebugEditRequestedEventArgs e)
        {
            var edit = new DebugCodeEdit();
            edit.LoadFile(e.FileName);
            edit.Lexer = _csParser;
            e.DebugEdit = edit;
            _currentEditor = edit;
        }

        private Button FindButton(Control sender)
        {
            if (sender == null)
            {
                throw new Exception("Sender is not part of a button.");
            }
            else if (sender is Button button)
            {
                return button;
            }
            else
            {
                return FindButton(sender.Parent);
            }
        }

        private void SetupFormLayout()
        {
            var splitter = new SplitContainer();
            splitter.Dock = DockStyle.Fill;
            splitter.Orientation = Orientation.Horizontal;
            splitter.SplitterDistance = 400;

            var editTabControl = new TabControl { Dock = DockStyle.Fill };
            splitter.Panel1.Controls.Add(editTabControl);

            _codeEditContainer = new DebugCodeEditContainer(editTabControl);
            _codeEditContainer.EditorRequested += EditorContainer_EditorRequested;

            _debuggerPanels = new DebuggerPanelsTabControl { Dock = DockStyle.Fill };
            _debuggerUiController = new DebuggerUIController(this, _codeEditContainer);
            _debuggerUiController.DebuggerPanels = _debuggerPanels;
            splitter.Panel2.Controls.Add(_debuggerPanels);

            Controls.Add(splitter);

            _debuggerToolbar = new DebuggerControlToolbar { Dock = DockStyle.Top };
            Controls.Add(_debuggerToolbar);

            var btnSendCompileCommandToServer = new Button { Dock = DockStyle.Top, Text = "Save and Compile" };
            btnSendCompileCommandToServer.Click += BtnSendCompileCommandToServer_Click;
            Controls.Add(btnSendCompileCommandToServer);

            var btnAttachDebugger = new Button { Dock = DockStyle.Top, Text = "Attach" };
            btnAttachDebugger.Click += BtnAttachDebugger_Click;
            Controls.Add(btnAttachDebugger);

            var btnAddRef = new Button { Dock = DockStyle.Top, Text = "Add Ref to 'SomeOtherLibrary'" };
            btnAddRef.Click += BtnAddRef_Click;
            Controls.Add(btnAddRef);
        }
    }
}
