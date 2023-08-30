using HartProtocol.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HartProtocol.Connection
{
    /// <summary>Реализация serial-com Port</summary>
    internal class Serial : IPort
    {
        private SerialPort _serialPort;
        
        private string _PortName;
        private int _BaudRate;
        private Parity _Parity;
        private StopBits _StopBits;
        public Serial(string portName, int baudRate, Parity parity, StopBits stopBits)
        {
            _PortName = portName;
            _BaudRate = baudRate;
            _Parity = parity;
            _StopBits = stopBits;
        }

        public event Action<byte[]> DataReceivedChanged;
        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(ReadTimeout);

            int bytes = _serialPort.BytesToRead; //посчитали к-во принятых байтов

            byte[] buff = new byte[bytes]; //указали размер массива буффера

            _serialPort.Read(buff, 0, bytes);//считали определенное к-во байт

            DataReceivedChanged?.Invoke(buff);

            Flush();
        }
        public bool IsConnected
        {
            get => _serialPort.IsOpen;
            set { }
        }
        public PortTypes PortType { get; set; } = PortTypes.SerialPort;

        public int ReadTimeout { get; set; } = 500;
        public int WriteTimeout { get; set; } = 500;

        public void Connect()
        {
            _serialPort = new SerialPort(_PortName, _BaudRate, _Parity, 8, _StopBits);
            _serialPort.ReadTimeout = ReadTimeout;
            _serialPort.WriteTimeout = WriteTimeout; 
            try
            {
                _serialPort.Open();
                _serialPort.DataReceived += _serialPort_DataReceived;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка: " + ex.Message);
            }
        }

        public void Disconnect()
        {
            if (!IsConnected) return;
            _serialPort?.Close();
        }

        public void Flush()
        {
            if (_serialPort is null || !_serialPort.IsOpen) return;
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            if (_serialPort is null) return 0;
            return _serialPort.Read(buffer, offset, length);
        }

        public void Write(byte[] buffer, int offset, int length)
        {
            if (_serialPort is null) return;
            _serialPort.Write(buffer, offset, length);
            Thread.Sleep(WriteTimeout);
        }
    }
}
