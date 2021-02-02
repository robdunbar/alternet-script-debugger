using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MyNamespace
{
    public class MyClass
    {
        private static int _count = 0;

        public string Execute()
        {
            Thread.Sleep(200);

            string message = "Hello " + _count++;
            Trace.WriteLine(message);
            return message;
        }
    }
}
