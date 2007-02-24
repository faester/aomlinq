using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceTests
{
    public class TestOutput
    {
        ExcelWriter readOut;
        ExcelWriter writeOut;
        ExcelWriter clearOut;
        ExcelWriter simpleSelectOut;

        public TestOutput(string filename, string prefix)
        {
            readOut = new ExcelWriter(filename, prefix + "_read");
            writeOut = new ExcelWriter(filename, prefix + "_write");
            clearOut = new ExcelWriter(filename, prefix + "_clear");
            simpleSelectOut = new ExcelWriter(filename, prefix + "_simpleselect");
        }

        public void ReceiveReadTestResult(int objCount, double time)
        {
            readOut.WriteInformation(objCount, time);
        }

        public void ReceiveWriteTestResult(int objCount, double time)
        {
            writeOut.WriteInformation(objCount, time);
        }

        public void ReceiveClearTestResult(int objCount, double time)
        {
            clearOut.WriteInformation(objCount, time);
        }

        public void RecieveSimpleSelectTestResult(int objCount, double time)
        {
            simpleSelectOut.WriteInformation(objCount, time);
        }
    }
}
