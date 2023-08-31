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
        inH2O = 0x1, //inches of water gaug 68 °F.
        inHg = 0x2, //inches of Hg gaug 0 °C (32 °F).
        ftH2O = 0x3,//Foot water column 68 °F.
        mmH2O = 0x4,//mm. water column 68 °F.
        mmHg = 0x5, //mm. Hg column 0 °C (32 °F).
        psi = 0x6,   // pound-force per square inch, lbf/in²
        bar = 0x7,
        mbar = 0x8,
        gCm2 = 0x9,
        kgCm2 = 0x10,
        Pa = 0x11,
        kPa = 0x12,
        torr = 0x13,
        MPa = 237,
        inH2O_2 = 238, //inches of water gaug 4 °C.
        mmH2O_2 = 239, //mm of water gaug 4 °C.
    }
    internal class Cmd_1_ReadingThePrimaryVariable : CommandConstructor
    {
        public Cmd_1_ReadingThePrimaryVariable(byte preamblesCount, FrameType frameType) : base(preamblesCount, frameType)
        {
        }
        public override byte CommandIndex => 1;

        
    }
}
