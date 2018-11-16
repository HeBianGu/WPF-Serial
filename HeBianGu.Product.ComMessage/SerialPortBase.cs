using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HeBianGu.Product.ComMessage
{
    public abstract class SerialPortBase
    {
        protected SerialPort serialPort;

        public SerialPortBase(int com, int br)
        {
            this.serialPort = new SerialPort("COM" + com, br, Parity.None, 8, StopBits.One);
        }

        protected abstract void DataReceived(object sender, SerialDataReceivedEventArgs e);

        public bool Start(out string message)
        {
            message = string.Empty;

            this.serialPort.DataReceived += new SerialDataReceivedEventHandler(this.DataReceived);

            if (!this.serialPort.IsOpen)
            {
                try
                {
                    this.serialPort.Open();

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        public bool Stop(out string message)
        {
            message = string.Empty;

            this.serialPort.DataReceived -= new SerialDataReceivedEventHandler(this.DataReceived);

            try
            {
                if (this.serialPort.IsOpen)
                {
                    this.serialPort.Close();
                }

                message = "关闭成功";

                return true;
            }
            catch (Exception ex)
            {
                message = "关闭失败:" + ex.Message;
                return false;
            }
        }

        protected bool DataSend(byte[] byteArray, out string message)
        {
            message = string.Empty;

            try
            {
                Thread.Sleep(50);

                if (this.serialPort.IsOpen)
                {
                    this.serialPort.DiscardInBuffer();
                    this.serialPort.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
                else
                {
                    return false;
                }


            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
