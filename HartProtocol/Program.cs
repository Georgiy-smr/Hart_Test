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

            Manager.SetNewUnitPrimaryVariable(Manager.Devices[0].RequestAddress, UnitPressure.bar);

            Thread.Sleep(550);

            Manager.SetPrimatyVariableToZERO(Manager.Devices[0].RequestAddress);

            while (Manager.Devices.Any()) 
            {
                ////Установка нового адреса
                //Thread.Sleep(550);
                //Manager.SetNewMicroAddress(Manager.Devices[0].RequestAddress, 0);

                //Проверка команд
                Thread.Sleep(550);
                Manager.Devices[0].ExecuteCommand(new Cmd_2_ReadCurrentAndPercentOfTheRange(Manager.Devices[0].ReceivedPreamblesCount, FrameType.LongFrame));
                Thread.Sleep(550);
                Manager.Devices[0].ExecuteCommand(new Cmd_3_ReadingFourVariables(Manager.Devices[0].ReceivedPreamblesCount, FrameType.LongFrame));
                Console.WriteLine($"{DateTime.Now}: I={Manager.Devices[0].Current_PV}mA\n1:{Manager.Devices[0].PrimaryVariableValue}{Manager.Devices[0].UnitPrimaryVariable}\n2:{Manager.Devices[0].SecondaryVariable}{Manager.Devices[0].UnitSecondaryVariable}\n3:{Manager.Devices[0].TertiaryVariable}{Manager.Devices[0].UnitTertiaryVariable}\n4:{Manager.Devices[0].FourthVariable}{Manager.Devices[0].UnitFourthVariable}\nPercentOfRange:{Manager.Devices[0].PercentOfTheRange}%");


                ////запрос диапазона
                //Manager.Devices[0].ExecuteCommand(new Cmd_14_ReadInfoAboutPrimaryVariable(Manager.Devices[0].ReceivedPreamblesCount, FrameType.LongFrame));

                
            }
            Console.WriteLine("Выдать инфу по сенсору");
            Console.ReadKey();
            if (Manager.Devices.Any())
            Console.WriteLine($"От {Manager.Devices[0].UpLimitSensor} до {Manager.Devices[0].LowLimitSensor} {Manager.Devices[0].Unit_Sensor}");

            Console.ReadKey();
        }

        private static void Manager_InitializeStatusChanged(bool result)
        {
            Console.WriteLine($"Success:{Manager.IsInitialized}.\nCount Connected Device:{Manager.Devices.Length}");
            if (Manager.IsInitialized)
            {
                for (int i = 0; i < Manager.Devices.Length; i++)
                {
                    Console.WriteLine($"№{i}:Адресс запроса {Manager.Devices[i].RequestAddress}");
                    Console.WriteLine($"Производитель: {Manager.Devices[i].ReceivedManufacturer}; Тип: {Manager.Devices[i].ReceivedType}");
                    Console.WriteLine($"Преамубулы: {Manager.Devices[i].ReceivedPreamblesCount}");
                    Console.WriteLine($"Заводской номер в строку: {Manager.Devices[i].AdressToString}");
                }
            }

        }
    }
}
