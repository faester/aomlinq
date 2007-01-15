using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceTests
{
    class Program
    {
        static void Main(string[] args)
        {
            ExcelWriter ew =new ExcelWriter("tst.xls", "uddata");
            for (int i = 0; i < 100; i++)
            {
                ew.WriteInformation(i, i * 121);
            }
        }
    }
}
