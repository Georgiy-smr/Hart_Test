using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HartProtocol.Models;
using HartProtocol.Services.Interfaces;

namespace HartProtocol.Services
{
    public enum FrameType
    {
        Undefined,
        ShortFrame,
        LongFrame
    }
    public abstract class CommandConstructor : IConstructorCommandForHart
    {
        public abstract byte CommandIndex { get; }
        public FrameType FrameType { get; private set; }
        public byte PreamblesCount { get; private set; }
        public byte[] Data { get; set; }

        public byte DataLength
        {
            get
            {
                if (Data == null)
                {
                    return 0;
                }

                return (byte)Data.Length;
            }
        }

        public CommandConstructor(byte preamblesCount, FrameType frameType)
        {
            PreamblesCount = preamblesCount;
            FrameType = frameType;
        }

        public byte[] GetCommand(Device device, params object[] objects)
        {
            int num = PreamblesCount + DataLength + 4;
            byte[] array;
            switch (FrameType)
            {
                case FrameType.ShortFrame:
                    num++;
                    array = new byte[num];
                    array[PreamblesCount] = 2;
                    array[PreamblesCount + 1] = device.GetByteMicroAdress(true,false);
                    array[PreamblesCount + 2] = CommandIndex;
                    array[PreamblesCount + 3] = DataLength;
                    if (DataLength > 0)
                    {
                        Data.CopyTo(array, PreamblesCount + 4);
                    }
                    break;
               case FrameType.LongFrame:
                    if(device.Adress == null) 
                        throw new ArgumentException();

                    num += 5;
                    array = new byte[num];
                    array[PreamblesCount] = 130;
                    device.GetBytesLongAdress().CopyTo(array, PreamblesCount + 1);
                    array[PreamblesCount + 6] = CommandIndex;
                    array[PreamblesCount + 7] = DataLength;
                    if (DataLength > 0)
                    {
                        Data.CopyTo(array, PreamblesCount + 8);
                    }
                    break;
                default:
                    throw new ArgumentException();
            }
            for (int i = 0; i < PreamblesCount; i++)
            {
                array[i] = byte.MaxValue;
            }
            array[num - 1] = CrcXor.Calculate(array, PreamblesCount, num - PreamblesCount - 1);
            return array;

        }
    }
}
