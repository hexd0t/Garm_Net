using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Garm.Gui.WinForms
{
    public class CollapsibleGroupBox : GroupBox
    {
        private string _text;
        public new string Text
        {
            get { return _text; }
            set { _text = value; base.Text = @"   "+value; }
        }

        private bool _collapsed;
        public bool Collapsed
        {
            get { return _collapsed; }
            set { Switch(value); }
        }

        public void Switch(bool collapsed)
        {
            _collapsed = collapsed;
            base.Size = _collapsed ? new Size(_size.Width, _collapsedHeight) : _size;
            Invalidate();
        }

        private const int _collapsedHeight = 20;
        private Size _size;
        public Size OpenedSize
        {
            get { return _size; }
            set
            {
                _size = value;
                Switch(_collapsed);
            }
        }

        public new Size Size
        {
            get { return base.Size; }
            set
            {
                _size = value;
                Switch(_collapsed);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_collapsed)
                e.Graphics.DrawImage(Properties.Resources.Decollapse,6,2,11,11);
            else
                e.Graphics.DrawImage(Properties.Resources.Collapse, 6, 2,11,11);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if(e.Y<14)
                Switch(!_collapsed);
            base.OnMouseClick(e);
        }
    }
}
