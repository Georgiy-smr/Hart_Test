using HartProtocol.Connection;
using HartProtocol.HartCommands;
using HartProtocol.Models;
using HartProtocol.Services;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HartProtocol
{
    internal class Program
    {
        static DeviceHartManagement Manager;
        static void Main(string[] args)
        {
            var serial = new Serial("COM6", 9600, Parity.None, StopBits.One);
            serial.Connect();
            Manager = new DeviceHartManagement(serial);
            Manager.InitializeStatusChanged += Manager_InitializeStatusChanged;
            Manager.Initialize();
            Console.WriteLine("Initialization...");
            Console.ReadKey();
            if (Manager.Devices.Any()) 
            {
                var device = Manager.Devices[0];
                device.ExecuteCommand(new Cmd_1_ReadingThePrimaryVariable(device.ReceivedPreamblesCount, FrameType.LongFrame));
            }
        }

        private static void Manager_InitializeStatusChanged(bool result)
        {
            Console.WriteLine($"Success:{Manager.IsInitialized}.\nCount Connected Device:{Manager.Devices.Length}");
            Console.ReadKey();
            if (Manager.IsInitialized)
            {
                for (int i = 0; i < Manager.Devices.Length; i++)
                {
                    Console.WriteLine($"№{i}:Адресс запроса {Manager.Devices[i].RequestAddress}");
                    Console.WriteLine($"Производитель: {Manager.Devices[i].ReceivedManufacturer}; Тип: {Manager.Devices[i].ReceivedType}");
                    Console.WriteLine($"Преамубулы: {Manager.Devices[i].ReceivedPreamblesCount}");
                    Console.WriteLine($"Заводской номер в строку: {Manager.Devices[i].AdressToString}");
                }
                Console.ReadKey();
            }

        }
    }
}
