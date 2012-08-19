using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Garm.Base.Abstract;
using Garm.Base.Helper;

namespace Garm.Base.Interfaces
{
    public interface IRunManager
    {
        IOptionsProvider Opts { get; }
        IReadOnlyList<View> Views { get; }
        ICommandExecutor Commands { get; }
        ManualResetEvent AbortOnExit { get; }
        FileManager Files { get; }
        event Action OnExit;
        bool DoRun { get; set; }
        int WaitOnShutdown {get; set;}
    }
}
