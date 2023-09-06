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
            var serial = new Serial("COM8", 64, Parity.None, StopBits.One);
            serial.Connect();
            Manager = new DeviceHartManagement(serial);
            Manager.InitializeStatusChanged += Manager_InitializeStatusChanged;
            Manager.Initialize();
            Console.WriteLine("Initialization...");

            while (!Manager.IsInitialized){ }

            while (Manager.Devices.Any()) 
            {
                Thread.Sleep(550);
                var device = Manager.Devices[0];
                device.ExecuteCommand(new Cmd_2_ReadCurrentAndPercentOfTheRange(device.ReceivedPreamblesCount, FrameType.LongFrame));
                Thread.Sleep(550);
                device.ExecuteCommand(new Cmd_3_ReadingFourVariables(device.ReceivedPreamblesCount, FrameType.LongFrame));
                Console.WriteLine($"{DateTime.Now}: I={device.Current_PV}mA\n1:{device.PrimaryVariableValue}{device.UnitPrimaryVariable}\n2:{device.SecondaryVariable}{device.UnitSecondaryVariable}\n3:{device.TertiaryVariable}{device.UnitTertiaryVariable}\n4:{device.FourthVariable}{device.UnitFourthVariable}\nPercentOfRange:{device.PercentOfTheRange}%");

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
