// Copyright (c) 2021 Cognex Corporation. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alternet.Scripter.Debugger;
using Alternet.Scripter.Debugger.UI;

namespace ScriptDebuggerApp
{
    public class AutoAttachDebuggerUICommands : DefaultDebuggerUICommands
    {
        private readonly IScriptDebugger _debugger;
        private readonly int _runtimeServerProcessId;

        public AutoAttachDebuggerUICommands(IScriptDebugger debugger, int runtimeServerProcessId) : base(debugger)
        {
            _debugger = debugger;
            _runtimeServerProcessId = runtimeServerProcessId;
        }

        /// <inheritdoc />
        public override bool Start()
        {
            if (Debugger.State == DebuggerState.Off)
            {
                // TODO Run Synchronously
                //_debugger.AttachToProcessAsync(_runtimeServerProcessId);
                
                // Does not work
                //var task = Task.Run(async () => { await _debugger.AttachToProcessAsync(_runtimeServerProcessId); });
                //task.Wait();
            }

            return base.Start();
        }

        /// <inheritdoc />
        public override bool Stop()
        {
            // Stopping while in "Stopped" state causes UI and remote application to mess up.
            // Instead, "Continue", to un-pause the application and then "fully" stop to detach.
            if (Debugger != null && Debugger.State == DebuggerState.Stopped)
            {
                base.Start();
            }

            return base.Stop();
        }
    }
}
