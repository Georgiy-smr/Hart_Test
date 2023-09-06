using HartProtocol.HartCommands;
using HartProtocol.Models.Base;
using HartProtocol.Services;
using HartProtocol.Services.Interfaces;
using System;
using System.Linq;

namespace HartProtocol.Models
{
    public class Device : BaseDevice
    {
        public Device(IPort port, int microAdress) :
            base(port, microAdress)  {}

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
            //var fake_Ansver = FakeGenerator();
            switch (_CurrentCommandIndex)
            {
                case 0:
                    InitializeIdDevice(obj);  break;
                case 1:
                    ReadPrimaryVariable(obj); break;
                case 2:
                    ReadCurrentAndPercentOfTheRange(obj); break;
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
        public UnitPressure UnitPrimaryVariable
        {
            get;
            set;
        }

        public float PrimaryVariableValue { get; set; }

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
        /// <summary> Ток первичной переменной, мА </summary>
        public float Current_PV { get; set; }
        /// <summary>Процент от диапазона, % </summary>
        public float PercentOfTheRange { get; set; }
        /// <summary> Функция чтения параметров тока и процента по команде № 2 </summary>
        public void ReadCurrentAndPercentOfTheRange(byte[] buff)
        {
            var bytetohex = Convectors.ByteToHex(buff);

        }




        #endregion
    }
}
