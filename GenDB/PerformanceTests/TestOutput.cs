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
        ExcelWriter selectOneOut;
        ExcelWriter selectOnePctOut;
        ExcelWriter selectTenPctOut;
        ExcelWriter selectHundredPctOut;
        ExcelWriter selectFiftySubFiftyPctOut;
        ExcelWriter selectNothingOut;
        ExcelWriter selectUnconditionalOut;

        public TestOutput(string filename, string prefix, string type)
        {
            if(type=="Q")
            {
                selectOneOut = new ExcelWriter(filename, prefix + "_selectone");
                selectOnePctOut = new ExcelWriter(filename, prefix + "_selectoneprocent");
                selectTenPctOut = new ExcelWriter(filename, prefix + "_selecttenprocent");
                selectHundredPctOut = new ExcelWriter(filename, prefix + "_selecthundredprocent");
                selectFiftySubFiftyPctOut = new ExcelWriter(filename, prefix + "_selectfiftysubfifty");
                selectNothingOut = new ExcelWriter(filename, prefix + "_selectnothing");
                selectUnconditionalOut = new ExcelWriter(filename, prefix + "_selectunconditional");
            }
            else
            {
                readOut = new ExcelWriter(filename, prefix + "_read");
                writeOut = new ExcelWriter(filename, prefix + "_write");
                clearOut = new ExcelWriter(filename, prefix + "_clear");
            }
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

        public void RecieveSelectOneTestResult(int objCount, double time)
        {
            selectOneOut.WriteInformation(objCount, time);
        }

        public void RecieveSelectOnePctTestResult(int objCount, double time)
        {
            selectOnePctOut.WriteInformation(objCount, time);
        }

        public void RecieveSelectTenPctTestResult(int objCount, double time)
        {
            selectTenPctOut.WriteInformation(objCount, time);
        }

        public void RecieveSelectHundredPctTestResult(int objCount, double time)
        {
            selectHundredPctOut.WriteInformation(objCount, time);
        }

        public void RecieveSelectFiftySubFiftyPctTestResult(int objCount, double time)
        {
            selectFiftySubFiftyPctOut.WriteInformation(objCount, time);
        }

        public void RecieveSelectNothingTestResult(int objCount, double time)
        {
            selectNothingOut.WriteInformation(objCount, time);
        }
        
        public void RecieveSelectUnconditionalTestResult(int objCount, double time)
        {
            selectUnconditionalOut.WriteInformation(objCount, time);
        }
    }
}
