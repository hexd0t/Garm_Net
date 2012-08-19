using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Garm.Gui.WinForms
{
    public class ComboBoxHiddenContentContainer
    {
        public string Text;
        public object Data;
        public ComboBoxHiddenContentContainer(string text, object data)
        {
            Text = text;
            Data = data;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
