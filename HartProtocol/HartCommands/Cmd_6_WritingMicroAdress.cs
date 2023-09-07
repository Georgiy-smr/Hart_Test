using HartProtocol.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.HartCommands
{
    internal class Cmd_6_WritingMicroAdress : CommandConstructor
    {
        public Cmd_6_WritingMicroAdress(byte preamblesCount, FrameType frameType) : base(preamblesCount, frameType)
        {
        }
        
        public override byte CommandIndex => 6;
    }
}
