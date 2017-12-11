using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace HicapsTools
{
    public class SerialClass
    {

        public static bool isComPortAvailable(string myComPort)
        {
            // Override check Code for Simulator Port
            if (myComPort == "COMX") return true;
            // IP Terminal
            if (myComPort == "COMT") return true;

            string[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                if (ports[i] == myComPort)
                {
                    return true;
                }
            }
            return false;
        }
        public static string[] getCommPortSearchList(string settingPorts)
        {
            string[] serialPorts;
            serialPorts = SerialPort.GetPortNames();

            List<string> portList = new List<string>();
            List<string> portListSorted = new List<string>();
            if (settingPorts != null && settingPorts != "" && settingPorts.IndexOf(":") >= 0)
            {
                // Override Windows Ports.

                foreach (string myPort in serialPorts)
                {
                    if (settingPorts.IndexOf(myPort + ":") >= 0)
                    {
                        portList.Add(myPort);
                    }

                }
                //serialPorts = portList.ToArray();
            }
            else
            {
                portList.AddRange(serialPorts);
            }
            /* Sort comports in integer order from COM1 to COM255 */
            /* Must be a better way to do this */
            if (portList.Count() > 0)
            {

                for (int i = 0; i < 255; i++)
                {
                    if (portList.IndexOf("COM" + i.ToString()) >= 0)
                    {
                        portListSorted.Add("COM" + i.ToString());
                    }
                }
            }
            // TODO.  If Terminal Simulator APplication installed, only then add COMX to search list
            if (ProcessClass.isSimulatorInstalled())
            {
                portListSorted.Insert(0, "COMX");
            }
            serialPorts = portListSorted.ToArray();
            return serialPorts;
        }

    }
}
