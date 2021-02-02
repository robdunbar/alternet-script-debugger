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

        public void Execute()
        {
            PerformNoOp();
        }

        private void PerformNoOp()
        {
            Thread.Sleep(200);
            
            Console.WriteLine("Hello " + _count++);
            Debug.WriteLine("Hello " + _count);
        }
    }
}
