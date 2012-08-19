using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Garm.Base.Interfaces
{
    public interface ICommandExecutor : IDisposable
    {
        void Parse (string command);
    }
}
