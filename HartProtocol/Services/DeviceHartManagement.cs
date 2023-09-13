using System;
using System.Collections.Generic;
using System.Data;
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
        /// <summary>
        /// Максимальное количество устройств
        /// </summary>
        private const int __DevicesCount = 15;
        /// <summary>
        /// Массив устройтв
        /// </summary>
        private Device[] _Devices;
        /// <summary>
        /// Свойство Устройств
        /// </summary>
        public Device[] Devices => _Devices;
        private IPort _Port;
        /// <summary>
        /// Делегат на событие завершения инициализации
        /// </summary>
        /// <param name="result"></param>
        public delegate void InitializeHandler(bool result);
        /// <summary>
        /// Событие завершения инициализации
        /// </summary>
        public event InitializeHandler InitializeStatusChanged;
        /// <summary>
        /// Свойство Выполнения инициализации
        /// </summary>
        private bool _IsInitialized = false;
        /// <summary>
        /// Свойство инициализации 
        /// </summary>
        public bool IsInitialized
        {
            get => _IsInitialized;
            private set 
            {
                _IsInitialized = value;
                InitializeStatusChanged?.Invoke(value);
            }
        }

        public bool IsReading { get; private set; } = false;


        #region CTOR
        public DeviceHartManagement(IPort Port)
        {
            _Port = Port;
        }
        public DeviceHartManagement(IEnumerable<Device> Devices, IPort Port)
            : this(Port)
        {
            _Devices = Devices.ToArray();
        }
        #endregion

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
        public void SetNewMicroAddress(byte old_RequestAddress, byte new_RequestAddress)
        {
            //проверить существование нашего девайса 
            var olddev = _Devices.FirstOrDefault(d => d.RequestAddress == old_RequestAddress);
            if (olddev == null) return;
            //защита от перезаписи
            if (old_RequestAddress == new_RequestAddress) return;
            //проверить что нет устройства под старым адрессом
            var newdev = _Devices.FirstOrDefault(d => d.RequestAddress == new_RequestAddress);
            if (newdev != null) return;

            //формирование адреса
            var address = new byte[1] { new_RequestAddress };

            //сформировать команду которая будет записывать новый адрес
            var commandWriteAddress = new Cmd_6_WritingMicroAdress(olddev.ReceivedPreamblesCount, FrameType.LongFrame)
            {
                Data = address
            };

            olddev.ExecuteCommand(commandWriteAddress);

        }

        #endregion

        #region Unit Record

        /// <summary> Записывает в устройство желаемые единицы измерения. По адресу запроса</summary>
        public void SetNewUnitPrimaryVariable(byte RequestAddress, UnitPressure WantUnit)
        {
            var device = _Devices.FirstOrDefault(d => d.RequestAddress == RequestAddress);
            //проверка на существование устройства
            if (device == null) return;


            //достать единицу девайса
            var unit = new byte[1] { (byte)WantUnit };
            if (unit[0] == 0) return;

            //создать команду 
            var commandWriteUnit = new Cmd_44_WriteUnitPrimaryVariable
                (device.ReceivedPreamblesCount, FrameType.LongFrame)
            {
                Data = unit
            };

            //выполнить команду записи на данный девайс 

            device.ExecuteCommand(commandWriteUnit);

        }

        #endregion

        #region Setting the primary variable to 0

        public void SetPrimatyVariableToZERO(byte RequestAddress)
        {
            var device = _Devices.FirstOrDefault(d => d.RequestAddress == RequestAddress);
            //проверка на существование устройства
            if (device == null) return;

            device.ExecuteCommand(new Cmd_43_SetPrimaryToZERO(device.ReceivedPreamblesCount, FrameType.LongFrame));
        }


        #endregion

        #region Reading current Variable Devices...

        public void StartUpdateAllVariable()
        {
            if (_Devices is null)
                Initialize();

            IsReading = true;
            new Thread(() => UpdateVariablesDevice()).Start();
        }

        public void StopUpdateAllVariable()
        {
            IsReading = false;
        }

        private void UpdateVariablesDevice()
        {
            int countDevices = _Devices.Length;
            int currentDevices = 0;
            int step =1;
            while (IsReading)
            {
                _Devices[currentDevices].ExecuteCommand(new Cmd_2_ReadCurrentAndPercentOfTheRange(
                    Devices[currentDevices].ReceivedPreamblesCount, FrameType.LongFrame));
                Thread.Sleep(550);

                Devices[currentDevices].ExecuteCommand(new Cmd_3_ReadingFourVariables
                    (Devices[currentDevices].ReceivedPreamblesCount, FrameType.LongFrame));
                Thread.Sleep(550);

                currentDevices += step;
                if (currentDevices > countDevices-1) currentDevices = 0;
            }
        }
        #endregion


    }
}
