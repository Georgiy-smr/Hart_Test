using HartProtocol.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.HartCommands
{
    internal class RequestIndificationID_Command : CommandConstructor
    {
        public RequestIndificationID_Command(byte preamblesCount, FrameType frameType)
            : base(preamblesCount, frameType)
        {
        }
        public override byte CommandIndex => 0;
    }




}
