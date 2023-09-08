using HartProtocol.Services;
using HartProtocol.Services.Interfaces;
using System;

namespace HartProtocol.Models.Base
{
    abstract public class BaseDevice
    {
        /// <summary>
        /// Строковый адрес
        /// </summary>
        public string AdressToString
        {
            get
            {
                if (_Adress_Device == null) return null;
                string id = Convectors.ByteToHex(Adress_Device);
                string[] idArray = id.Split(new char[] { ' ' });
                string ID = $"{idArray[0]}{idArray[1]}{idArray[2]}";
                return Convert.ToInt32(ID, 16).ToString();
            }
        }
        /// <summary>
        /// Длинный адрес устройства для формирования команды
        /// </summary>
        private byte[] _Adress_Device;
        /// <summary>
        /// Длинный адрес устройства для формирования команды
        /// </summary>
        public byte[] Adress_Device
        {
            get { return _Adress_Device; }
            set
            {
                if(value == null)
                {
                    _Adress_Device = value;
                    return;
                }
                _Adress_Device =value;
                if (value.Length != 3) throw new ArgumentException(nameof(Adress_Device));
                _Adress_Device = value;
            }
        }
        /// <summary> Короткий адрес для формирования команды </summary>
        public byte MicroAddress { get; set; }

        /// <summary>Адрес запроса от 0 до 15</summary>
        public byte RequestAddress { get; set; }
        /// <summary> Производитель </summary>
        public byte ReceivedManufacturer { get; set; }
        /// <summary> Тип устройства</summary>
        public byte ReceivedType { get; private set; }
        /// <summary>Кол-во преамбул для формирования фрейма</summary>
        public byte ReceivedPreamblesCount { get; private set; }
        public IPort _Port;
        public BaseDevice(IPort port, int requestAddress)
        {
            if (requestAddress > 15 || requestAddress < 0)
            {
                throw new ArgumentException();
            }

            RequestAddress = (byte)requestAddress;
            _Port = port;
        }
        /// <summary> Индекс выполняемой текущей команды </summary>
        public byte _CurrentCommandIndex;

        public abstract void ExecuteCommand(CommandConstructor command);

        /// <summary> Формирование короткого адреса (байта) на основании адреса запроса,
        /// первичного мастера и вида монополии </summary>
        internal byte GetByteMicroAdress(bool FirstMaster = true, bool Monopoly = false)
        {
            string array = FirstMaster ? 1.ToString() : 0.ToString();
            array += Monopoly ? 1.ToString() : 0.ToString();
            array += "00";
            var a = Convert.ToString(RequestAddress, 2);
            if (a.Length < 4)
                while (a.Length < 4)
                {
                    a = "0" + a;
                }
            else if (a.Length > 4)
                while (a.Length > 4)
                {
                    a = a.Substring(0, 1);
                }
            array += a;

            MicroAddress = Convert.ToByte(array, 2);
            return MicroAddress;
        }


        /// <summary>  Выдаёт 5 байт длинного адреса </summary>
        internal byte[] GetBytesLongAdress(params object[] objects)
        {
            if(Adress_Device == null) return null;

            return new byte[5]
            {
                ReceivedManufacturer,
                ReceivedType,
                Adress_Device[0],
                Adress_Device[1],
                Adress_Device[2]
            };
        }
        /// <summary>
        /// Инициализирует себя по 0 команде.(Узнаёт производителя, тип, кол-во преамбул, длинный адрес)
        /// </summary>
        /// <param name="buff"></param>
        public void InitializeIdDevice(byte[] buff)
        {
            if (buff is null || buff.Length==0) return;

            for (int i = Array.IndexOf(buff, byte.MaxValue); i < buff.Length; i++)
            {
                if (buff[i] != byte.MaxValue)
                {
                    if (buff[i] == 6)
                    {
                        //Проверяем CRC (Отбраковываем посылки)
                        try
                        {
                            if (MicroAddress != buff[i + 1])
                                return; 

                            var dataLength = buff[i + 3];
                            var checkCrc = CrcXor.Calculate(buff, i, dataLength + 4);
                            var device_crc = buff[dataLength + 4 + i];
                            if (checkCrc != device_crc) throw new Exception($"Не совпала контрольная сумма у {RequestAddress}");
                        }
                        catch (ArgumentOutOfRangeException ex)
                        {
                            Console.WriteLine($"На адрес {RequestAddress}: " + ex.Message);
                            return;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Ошибка: " + ex.Message);
                            return;
                        }

                    }
                    if (buff[i] == 254)
                    {
                        try
                        {
                            ReceivedManufacturer = buff[i + 1];
                            ReceivedType = buff[i + 2];
                            ReceivedPreamblesCount = buff[i + 3];
                            var status = buff[i + 8];
                            Adress_Device = new byte[3] { buff[i + 9], buff[i + 10], buff[i + 11] };
                        }
                        catch (Exception ex)
                        {
                            Adress_Device=null;
                            Console.WriteLine($"Ошибка адреса {RequestAddress} " + ex.Message);
                        }
                    }
                }
            }
        }
        
        /// <summary> Проверка адреса </summary>
        protected bool AddressVerification(byte[] Adres) 
        {
            if(Adres.Length!=5 || Adres.Length==0) return false;

            var DevBdr = new byte[5];
            this.GetBytesLongAdress().CopyTo(DevBdr, 0);
            for (int j = 0; j < 5; j++)
            {
                if (Adres[j] != DevBdr[j]) return false;
            }
            return true;
        }

        /// <summary> Проверка на индекс команды </summary>
        protected bool CommandIndexVerification(byte CommandIndex)
        {
            return (_CurrentCommandIndex == CommandIndex);
        }

    }
}
