using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Garm.Base.Interfaces;

namespace Garm.Base.Abstract
{
    public abstract class View : Base
    {
        protected View(IRunManager manager) : base (manager)
        { }

        public abstract void Run();
        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
