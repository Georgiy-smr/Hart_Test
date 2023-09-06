using HartProtocol.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.HartCommands
{
    /// <summary> Считать значения четырех (предопределенных) динамических переменных и ток первичной переменной </summary>
    internal class Cmd_3_ReadingFourVariables : CommandConstructor
    {
        public Cmd_3_ReadingFourVariables(byte preamblesCount, FrameType frameType) : base(preamblesCount, frameType){ }

        public override byte CommandIndex => 3;
    }
}
