﻿using HartProtocol.HartCommands;
using HartProtocol.Models.Base;
using HartProtocol.Services;
using HartProtocol.Services.Interfaces;
using System;
using System.Linq;

namespace HartProtocol.Models
{
    public class Device : BaseDevice
    {
        public Device(IPort port, int microAdress)
            : base(port, microAdress) {}

        public override void ExecuteCommand(CommandConstructor command)
        {
            if (!_Port.IsConnected) _Port.Connect();
            if (!_Port.IsConnected) return;
            _Port.DataReceivedChanged += Port_DataReceivedChanged;
            _CurrentCommandIndex = command.CommandIndex;

            //Отправка и формирование фрейма 
            var Frame = command.GetCommand(this);
            _Port.Flush();
            _Port.Write(Frame, 0, Frame.Length);
            System.Threading.Thread.Sleep(10);
        }

        public event Action<int> FinishReceived;

        /// <summary>Обработчик входных данных </summary>
        public void Port_DataReceivedChanged(byte[] obj)
        {
            switch (_CurrentCommandIndex)
            {
                    case 0:
                    InitializeIdDevice(obj);  break;
                    case 1:
                    ReadPrimaryVariable(obj); break;
                    case 2:
                    ReadCurrentAndPercentOfTheRange(obj); break;
                    case 3:
                    ReadingFourVariableAndCurrentPV(obj); break;
                    case 6:
                    ReadNewMicroAddress(obj); break;
                    case 14:
                    ReadInfoPrimaryVariable(obj); break;

            }

            FinishReceived?.Invoke(MicroAddress);
            _Port.DataReceivedChanged -= Port_DataReceivedChanged;
        }

  
        private byte[] FakeGenerator()
        {
            System.Random Fck_Rnd = new System.Random();
            switch (_CurrentCommandIndex)
            {
                case 0:
                    var fake_bytes =  new byte[23]
                    {
                        255, 255, 255, 255, 6, this.GetByteMicroAdress(true,false), 0, 14, 0, 0,
                        254, 14, 171, 5, 5, 5, 21, 136, 00, (byte)Fck_Rnd.Next(255),
                        (byte)Fck_Rnd.Next(255), (byte)Fck_Rnd.Next(255), 0
                    };
                    fake_bytes[23-1] = CrcXor.Calculate(fake_bytes, 4, 23 - 4 - 1);
                    return fake_bytes;
                case 1:
                    return new byte[23]
                    {
                        12, 255, 255, 255, 255, 255, 255, 134, 42, 11, 
                        107, 207, 73, 1, 7, 0, 72, 10, 188, 199,
                        163, 233, 63
                    };
                default:
                    return null;
            }
        }

        #region 1_CMD

        public void ReadPrimaryVariable(byte[] buff)
        {
            if (buff == null || buff.Length == 0 ) return;

            for (int i = Array.IndexOf(buff, byte.MaxValue); i < buff.Length; i++)
            {
                if (buff[i] != byte.MaxValue)
                {
                    if (buff[i] == 134)
                    {
                        try
                        {
                            var startCr = i;
                            var adressReq = new byte[5]
                            {
                            buff[i+1],
                            buff[i+2],
                            buff[i+3],
                            buff[i+4],
                            buff[i+5]
                            };
                            if (!AddressVerification(adressReq)) throw new Exception();

                            if (!CommandIndexVerification(buff[i + 6])) throw new Exception();

                            var dataLengt = buff[i + 7];
                            if (!(CrcXor.Calculate(buff, startCr, dataLengt + 8) != buff[i + 9])) 
                                throw new Exception("Не совпала контрольная сумма");

                            byte Unit = buff[i + 10];
                            if (Enum.IsDefined(typeof(UnitPressure), Unit))
                                UnitPrimaryVariable = (UnitPressure)Unit;

                            byte[] bytes = new byte[4] { buff[i + 11], buff[i + 12], buff[i + 13], buff[i + 14] };

                            PrimaryVariableValue = BitConverter.ToSingle(bytes.Reverse().ToArray(), 0);

                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка: " + ex.Message);
                            return;
                        }

                    }
                }
            }
        }

        #endregion

        #region 2_CMD
        /// <summary> Функция чтения параметров тока и процента по команде № 2 </summary>
        public void ReadCurrentAndPercentOfTheRange(byte[] buff)
        {
            if (buff == null || buff.Length == 0) return;

            for (int i = Array.IndexOf(buff, byte.MaxValue); i < buff.Length; i++)
            {
                if (buff[i] != byte.MaxValue)
                {
                    if (buff[i] == 134)
                    {
                        try
                        {
                            var startCr = i;
                            var adressReq = new byte[5]
                            {
                            buff[i+1],
                            buff[i+2],
                            buff[i+3],
                            buff[i+4],
                            buff[i+5]
                            };
                            if (!AddressVerification(adressReq)) throw new Exception();

                            if (!CommandIndexVerification(buff[i + 6])) throw new Exception();

                            var dataLengt = buff[i + 7];
                            if (!(CrcXor.Calculate(buff, startCr, dataLengt + 8) != buff[i + 9]))
                                throw new Exception("Не совпала контрольная сумма");


                            //1C FF FF FF FF 86 2A 0B 6B CF 49 02 0A 00 68 40 80 00 00 BB 77 1C D1 EB

                            var bytes_Current = new byte[4] { buff[i + 10], buff[i + 11], buff[i + 12], buff[i + 13] };
                            Current_PV = BitConverter.ToSingle(bytes_Current.Reverse().ToArray(), 0);

                            var bytes_Percent = new byte[4] { buff[i + 14], buff[i + 15], buff[i + 16], buff[i + 17] };
                            PercentOfTheRange = BitConverter.ToSingle(bytes_Percent.Reverse().ToArray(), 0);


                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка: " + ex.Message);
                            return;
                        }

                    }
                }
            }



        }

        #endregion

        #region 3_CMD

        private void ReadingFourVariableAndCurrentPV(byte[] buff)
        {
            if (buff == null || buff.Length == 0) return;

            for (int i = Array.IndexOf(buff, byte.MaxValue); i < buff.Length; i++)
            {
                if (buff[i] != byte.MaxValue)
                {
                    if (buff[i] == 134)
                    {
                        try
                        {
                            var startCr = i;
                            var adressReq = new byte[5]
                            {
                            buff[i+1],
                            buff[i+2],
                            buff[i+3],
                            buff[i+4],
                            buff[i+5]
                            };
                            if (!AddressVerification(adressReq)) throw new Exception();

                            if (!CommandIndexVerification(buff[i + 6])) throw new Exception();

                            var dataLengt = buff[i + 7];
                            if (!(CrcXor.Calculate(buff, startCr, dataLengt + 8) != buff[i + 9]))
                                throw new Exception("Не совпала контрольная сумма");


                            //0C FF FF FF FF 86 2A 0B 6B CF 49 03 1A 00 48 40 80 00 00 0A BC 05 45 E2 0A BC 05 45 E2 0A BC 05 45 E2 0A BC 05 45 E2 DB

                            var bytes_Current = new byte[4] { buff[i + 10], buff[i + 11], buff[i + 12], buff[i + 13] };
                            Current_PV = BitConverter.ToSingle(bytes_Current.Reverse().ToArray(), 0);


                            byte Unit_1 = buff[i + 14];
                            if (Enum.IsDefined(typeof(UnitPressure), Unit_1))
                                UnitPrimaryVariable = (UnitPressure)Unit_1;
                            var bytes_PrimaryVariable = new byte[4] { buff[i + 15], buff[i + 16], buff[i + 17], buff[i + 18] };
                            PrimaryVariableValue = BitConverter.ToSingle(bytes_PrimaryVariable.Reverse().ToArray(), 0);

                            byte Unit_2 = buff[i + 19];
                            if (Enum.IsDefined(typeof(UnitPressure), Unit_2))
                                UnitSecondaryVariable = (UnitPressure)Unit_2;
                            var bytes_SecondaryVariable = new byte[4] { buff[i + 20], buff[i + 21], buff[i + 22], buff[i + 23] };
                            SecondaryVariable = BitConverter.ToSingle(bytes_SecondaryVariable.Reverse().ToArray(), 0);

                            byte Unit_3 = buff[i + 24];
                            if (Enum.IsDefined(typeof(UnitPressure), Unit_3))
                                UnitTertiaryVariable = (UnitPressure)Unit_3;
                            var bytes_TertiaryVariable = new byte[4] { buff[i + 25], buff[i + 26], buff[i + 27], buff[i + 28] };
                            TertiaryVariable = BitConverter.ToSingle(bytes_TertiaryVariable.Reverse().ToArray(), 0);

                            byte Unit_4 = buff[i + 29];
                            if (Enum.IsDefined(typeof(UnitPressure), Unit_4))
                                UnitFourthVariable = (UnitPressure)Unit_4;
                            var bytes_FourthVariable = new byte[4] { buff[i + 30], buff[i + 31], buff[i + 32], buff[i + 34] };
                            FourthVariable = BitConverter.ToSingle(bytes_FourthVariable.Reverse().ToArray(), 0);

                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка: " + ex.Message);
                            return;
                        }

                    }
                }
            }
        }
        #endregion

        #region 6_CMD
        private void ReadNewMicroAddress(byte[] buff)
        {
            
            if (buff == null || buff.Length == 0) return;

            for (int i = Array.IndexOf(buff, byte.MaxValue); i < buff.Length; i++)
            {
                if (buff[i] != byte.MaxValue)
                {
                    if (buff[i] == 134)
                    {
                        try
                        {
                            var startCr = i;
                            var adressReq = new byte[5]
                            {
                            buff[i+1],
                            buff[i+2],
                            buff[i+3],
                            buff[i+4],
                            buff[i+5]
                            };
                            if (!AddressVerification(adressReq)) throw new Exception();

                            if (!CommandIndexVerification(buff[i + 6])) throw new Exception();

                            var dataLengt = buff[i + 7];
                            if (!(CrcXor.Calculate(buff, startCr, dataLengt + 8) != buff[i + 9]))
                                throw new Exception("Не совпала контрольная сумма");

                            RequestAddress = buff[i + 10];

                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка: " + ex.Message);
                            return;
                        }

                    }
                }
            }
        }

        #endregion

        #region 14_CMD

        private void ReadInfoPrimaryVariable(byte[] buff)
        {
            if (buff == null || buff.Length == 0) return;

            for (int i = Array.IndexOf(buff, byte.MaxValue); i < buff.Length; i++)
            {
                if (buff[i] != byte.MaxValue)
                {
                    if (buff[i] == 134)
                    {
                        try
                        {
                            var startCr = i;
                            var adressReq = new byte[5]
                            {
                            buff[i+1],
                            buff[i+2],
                            buff[i+3],
                            buff[i+4],
                            buff[i+5]
                            };
                            if (!AddressVerification(adressReq)) throw new Exception();

                            if (!CommandIndexVerification(buff[i + 6])) throw new Exception();

                            var dataLengt = buff[i + 7];
                            if (!(CrcXor.Calculate(buff, startCr, dataLengt + 8) != buff[i + 9]))
                                throw new Exception("Не совпала контрольная сумма");


                            ID_Sensor = new byte[3] { buff[i + 10], buff[i + 11], buff[i + 12] };

                            var unit_sensor = buff[i + 13];
                            if (Enum.IsDefined(typeof(UnitPressure), unit_sensor))
                                Unit_Sensor = (UnitPressure)unit_sensor;

                            var bytes_UpLimit_Sensor = new byte[4] { buff[i + 14], buff[i + 15], buff[i + 16], buff[i + 17] };
                            UpLimitSensor = BitConverter.ToSingle(bytes_UpLimit_Sensor.Reverse().ToArray(), 0);

                            var bytes_LowLimit_Sensor = new byte[4] { buff[i + 18], buff[i + 19], buff[i + 20], buff[i + 21] };
                            LowLimitSensor = BitConverter.ToSingle(bytes_LowLimit_Sensor.Reverse().ToArray(), 0);

                            var bytes_MinInterval_Sensor = new byte[4] { buff[i + 22], buff[i + 23], buff[i + 24], buff[i + 25] };
                            MinIntervalSensor = BitConverter.ToSingle(bytes_MinInterval_Sensor.Reverse().ToArray(), 0);

                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка: " + ex.Message);
                            return;
                        }

                    }
                }
            }
        }
        #endregion
    }
}
