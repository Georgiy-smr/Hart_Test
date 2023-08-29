using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.Services.Interfaces
{
    [Flags]
    public enum PortTypes
    {
        Undefined = 0x0,
        SerialPort = 0x1,
        SerialModemPort = 0x2,
        CellularModemPort = 0x4,
        EthernetPort = 0x8
    }
    public interface IPort
    {
        bool IsConnected { get; }

        PortTypes PortType { get; }

        int ReadTimeout { get; set; }

        int WriteTimeout { get; set; }

        void Connect();

        void Disconnect();

        void Flush();

        int Read(byte[] buffer, int offset, int length);

        void Write(byte[] buffer, int offset, int length);

        event Action<byte[]> DataReceivedChanged;


    }
}
