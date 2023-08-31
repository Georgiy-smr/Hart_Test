using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HartProtocol.HartCommands;
using HartProtocol.Models;
using HartProtocol.Services.Interfaces;

namespace HartProtocol.Services
{
    public class DeviceHartManagement
    {
        private const int __DevicesCount = 15;
        private Device[] _Devices;
        public Device[] Devices 
        {
            get => _Devices;
        }
        private IPort _Port;

        public delegate void InitializeHandler(bool result);
        public event InitializeHandler InitializeStatusChanged;


        private bool _IsInitialized = false;
        public bool IsInitialized
        {
            get => _IsInitialized;
            private set 
            {
                _IsInitialized = value;
                InitializeStatusChanged?.Invoke(value);
            }
        }
        public DeviceHartManagement(IPort Port) { _Port = Port; }
        public DeviceHartManagement(IEnumerable<Device> Devices)
        {
            _Devices = Devices.ToArray();
        }
        public void Initialize()
        {
            if (_Devices != null) return;
            _Devices = new Device[__DevicesCount];
            _Devices = Enumerable.Range(0, __DevicesCount+1) //adr 0 to 15
                .Select(i=>new Device(_Port,i)).ToArray();

            new Thread(() => InitializeDevices()).Start();
        }
        private void InitializeDevices()
        {
            for (int i = 0; i <= __DevicesCount; i++)
            {
                
                _Devices[i].ExecuteCommand(new RequestIndificationID_Command(5, FrameType.ShortFrame));
            }
            _Devices = _Devices.Where(d => d.Adress != null).ToArray();

            IsInitialized = _Devices.Length > 0 ? true : false;
        }

    }
}
