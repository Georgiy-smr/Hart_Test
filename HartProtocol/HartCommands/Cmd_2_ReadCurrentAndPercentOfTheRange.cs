using HartProtocol.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.HartCommands
{
    /// <summary>
    /// Чтение первичной переменной как тока и процента диапазона PVPV всегда соответствует выходному току прибора, включая аварийные состояния и установленные величины. Процент диапазона не ограничен величинами между 0% и 100%, но и отслеживается за границами диапазона PV до границ диапазона сенсора (если они определены).
    /// </summary>
    internal class Cmd_2_ReadCurrentAndPercentOfTheRange : CommandConstructor
    {
        public Cmd_2_ReadCurrentAndPercentOfTheRange(byte preamblesCount, FrameType frameType) : base(preamblesCount, frameType)
        {}
        public override byte CommandIndex => 2;
    }
}
