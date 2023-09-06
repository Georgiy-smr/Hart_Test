using HartProtocol.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.HartCommands
{
    
    public enum UnitPressure : byte
    {
        inH2O = 1, //inches of water gaug 68 °F.
        inHg = 2, //inches of Hg gaug 0 °C (32 °F).
        ftH2O = 3,//Foot water column 68 °F.
        mmH2O = 4,//mm. water column 68 °F.
        mmHg = 5, //mm. Hg column 0 °C (32 °F).
        psi = 6,   // pound-force per square inch, lbf/in²
        bar = 7,
        mbar = 8,
        gCm2 = 9,
        kgCm2 = 10,
        Pa = 11,
        kPa = 12,
        torr = 13,
        MPa = 237,
        inH2O_2 = 238, //inches of water gaug 4 °C.
        mmH2O_2 = 239, //mm of water gaug 4 °C.
    }
    internal class Cmd_1_ReadingThePrimaryVariable : CommandConstructor
    {
        public Cmd_1_ReadingThePrimaryVariable(byte preamblesCount, FrameType frameType) : base(preamblesCount, frameType)
        {}
        public override byte CommandIndex => 1;
    }
}
