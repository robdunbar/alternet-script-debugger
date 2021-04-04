using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TestLib;

namespace MyNamespace
{
    public class MyClass
    {
        private static int _count = 0;

        public string Execute()
        {
            Thread.Sleep(200);

            SomeClassWithXmlDocs.SomeMethod("");

            string message = "Hello " + _count++;
            Trace.WriteLine(message);
            return message;
        }
    }
}
