using HartProtocol.HartCommands;
using HartProtocol.Models.Base;
using HartProtocol.Services;
using HartProtocol.Services.Interfaces;
using System;

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

        /// <summary>Обработчик входных данных </summary>
        public void Port_DataReceivedChanged(byte[] obj)
        {
            var fake_Ansver = FakeGenerator();
            switch (_CurrentCommandIndex)
            {
                case 0:
                    InitializeIdDevice(fake_Ansver); 
                    break;
                case 1:
                    DefinitionPrimaryVariable(obj);
                    break;
            }
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


        public void DefinitionPrimaryVariable(byte[] buff)
        {
            buff = FakeGenerator();
            
            if (buff == null || buff.Length == 0 ) return;

            for (int i = Array.IndexOf(buff, byte.MaxValue); i < buff.Length; i++)
            {
                if (buff[i] != byte.MaxValue)
                {
                    if (buff[i] == 134)
                    {
                        byte value = buff[17];
                        if (Enum.IsDefined(typeof(UnitPressure), value))
                            UnitPrimaryVariable = (UnitPressure)value;
                        break;
                    }
                }
            }

            //искать байт начала 
            //добыть данные и применить их к свойству PrimeryUnit в Device
            ////"0C FF FF FF FF FF FF 86 2A 0B 6B CF 49 01 07 00 48 0A BC C7 A3 E9 3F"
            //string id = Convectors.ByteToHex(buff);
        }
        #endregion














    }
}
