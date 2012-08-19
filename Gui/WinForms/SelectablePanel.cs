using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Garm.Gui.WinForms
{
    public class SelectablePanel : Panel
    {
        public SelectablePanel()
        {
            SetStyle(ControlStyles.Selectable,true);
            TabStop = true;
        }
    }
}
