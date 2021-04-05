using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MyNamespace
{
    /// <summary>
    /// My class that has a summary.
    /// </summary>
    public class MyClass
    {
        private static int _count = 0;

        /// <summary>
        /// A method with a summary.
        /// </summary>
        /// <returns>A message.</returns>
        public string Execute()
        {
            Thread.Sleep(200);

            string message = "Hello lasharn " + _count++;
            Trace.WriteLine(message);
            return message;
        }
    }
}