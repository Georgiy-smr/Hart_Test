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
            //var fake_Ansver = FakeGenerator();
            switch (_CurrentCommandIndex)
            {
                case 0:
                    InitializeIdDevice(obj); 
                    break;
                case 2:
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
                default:
                    return null;
            }
        }

        #region 1_CMD
        public void DefinitionPrimaryVariable(byte[] buff)
        {
            //проверки
            //искать байт начала 
            //добыть данные и применить их к свойству PrimeryUnit в Device
            //ответ на 3 команду
            //1C FF FF FF FF FF 86 2A 0B 6B CF 49 03 1A 00 48 40 80 00 00 0A BC F6 2D 77 0A BC F6 2D 77 0A BC F6 2D 77 0A BC F6 2D 77 DB
            //12 255 255 255 255 255 255 255 134 42 11 107 207 73 3 26 0 72 64 128 0 0 10 188 247 232 211 10 188 247 232 211 10 188 247 232 211 10 188 247 232 211 219
            string id = Convectors.ByteToHex(buff);
            byte value = 0;

            //ток - 00 48 40 80 (40 80 00 48 : 4.00003433) 
            //00 ед измерения первичной переменной
            //первичная переменная 00 0A BC F6 (BC F6 00 0A : -0.0300293155)
            //2D единицы измерения второй переменной
            //77 0A BC F6 (BC F6 77 0A : -0.03008606)
            //2D единицы измерения третьей переменной
            //77 0A BC F6 (BC F6 77 0A : -0.03008606)
            UnitPressure unit = new UnitPressure();
            if (Enum.IsDefined(typeof(UnitPressure), value))
                unit = (UnitPressure)value;
            
        }
        #endregion














    }
}
