using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EGInterface
{
    public interface IEventGenerator
    {
        void SendEvent(object[] Params);
        string GetName();
    }
}
