// Copyright (c) 2021 Cognex Corporation. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptDebuggerApp
{
    internal static class PathHelper
    {
        private const string PrecompileLibraryToReferenceName = "PrecompileLibraryToReference";
        private const string RuntimeServerName = "RuntimeServer";
        private const string UserScriptLibName = "UserScriptLib";

        static PathHelper()
        {
            SolutionDir = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(ScriptDebuggerForm).Assembly.Location), @"..\..\..\.."));

            RuntimeServerExe = Path.Combine(SolutionDir, RuntimeServerName, @"bin\Debug\netcoreapp3.1", RuntimeServerName + ".exe");

            PrecompiledLibraryToReferenceDll = Path.Combine(SolutionDir, PrecompileLibraryToReferenceName, @"bin\Debug\netcoreapp3.1", PrecompileLibraryToReferenceName + ".dll");

            UserScriptBuildDir = Path.Combine(SolutionDir, UserScriptLibName, @"bin\Debug\netcoreapp3.1");
            UserScriptDllFile = Path.Combine(UserScriptBuildDir, UserScriptLibName + ".dll");
            UserScriptPdbFile = Path.ChangeExtension(UserScriptDllFile, ".pdb");
            UserScriptXmlFile = Path.ChangeExtension(UserScriptDllFile, ".xml");
            UserScriptSourceFile = Path.Combine(SolutionDir, UserScriptLibName, "MyClass.cs");
        }

        public static string PrecompiledLibraryToReferenceDll { get; }

        public static string RuntimeServerExe { get; }

        public static string SolutionDir { get; }

        public static string UserScriptBuildDir { get; }

        public static string UserScriptDllFile { get; }

        public static string UserScriptPdbFile { get; }

        public static string UserScriptSourceFile { get; }

        public static string UserScriptXmlFile { get; }
    }
}
