using HartProtocol.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.HartCommands
{
    internal class Cmd_43_SetPrimaryToZERO : CommandConstructor
    {
        public Cmd_43_SetPrimaryToZERO(byte preamblesCount, FrameType frameType) : base(preamblesCount, frameType)
        {
        }

        public override byte CommandIndex => 43;
    }
}
