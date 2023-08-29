using HartProtocol.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HartProtocol.Services.Interfaces
{
    internal interface IConstructorCommandForHart
    {
        byte[] GetCommand(Device device, params object[] objects);

    }
}
