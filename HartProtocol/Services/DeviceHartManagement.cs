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

        #region Initialization process....
        public void Initialize()
        {
            if (_Devices != null) return;
            _Devices = new Device[__DevicesCount];
            _Devices = Enumerable.Range(0, __DevicesCount + 1) //adr 0 to 15
                .Select(i => new Device(_Port, i)).ToArray();

            new Thread(() => InitializeDevices()).Start();
        }
        private void InitializeDevices()
        {
            for (int i = 0; i <= __DevicesCount; i++)
            {
                Thread.Sleep(100);
                _Devices[i].ExecuteCommand(new RequestIndificationID_Command(5, FrameType.ShortFrame));
            }
            Thread.Sleep(100);
            _Devices = _Devices.Where(d => d.Adress_Device != null).ToArray();

            IsInitialized = _Devices.Length > 0 ? true : false;
        }
        #endregion

        #region Address record....
        public void SetNewMicroAddress(byte old_microAdress, byte new_microAdress)
        {
            //проверить существование нашего девайса 
            var olddev = _Devices.FirstOrDefault(d => d.RequestAddress == old_microAdress);
            if (olddev == null) return;
            //защита от перезаписи
            if (old_microAdress == new_microAdress) return;
            //проверить что нет устройства под старым адрессом
            var newdev = _Devices.FirstOrDefault(d => d.RequestAddress == new_microAdress);
            if (newdev != null) return;

            //формирование адреса
            var address = new byte[1] { new_microAdress };

            //сформировать команду которая будет записывать новый адрес
            var commandWriteAddress = new Cmd_6_WritingMicroAdress(olddev.ReceivedPreamblesCount, FrameType.LongFrame)
            {
                Data = address
            };

            olddev.ExecuteCommand(commandWriteAddress);

        }



        #endregion

    }
}
