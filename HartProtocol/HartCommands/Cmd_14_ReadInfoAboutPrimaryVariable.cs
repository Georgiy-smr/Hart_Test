using HartProtocol.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.HartCommands
{
    internal class Cmd_14_ReadInfoAboutPrimaryVariable : CommandConstructor
    {
        public Cmd_14_ReadInfoAboutPrimaryVariable(byte preamblesCount, FrameType frameType) : base(preamblesCount, frameType)
        {
        }

        public override byte CommandIndex => 14;
    }
}
