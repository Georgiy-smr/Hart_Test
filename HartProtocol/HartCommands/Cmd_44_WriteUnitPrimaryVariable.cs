using HartProtocol.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.HartCommands
{
    internal class Cmd_44_WriteUnitPrimaryVariable : CommandConstructor
    {
        public Cmd_44_WriteUnitPrimaryVariable
            (byte preamblesCount, FrameType frameType) : base(preamblesCount, frameType) { }

        public override byte CommandIndex => 44;
    }
}
