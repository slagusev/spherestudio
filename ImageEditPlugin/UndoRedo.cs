﻿using System.Drawing;
using System.Drawing.Drawing2D;

using SphereStudio.Plugins.Components;
using SphereStudio.Utility;

namespace SphereStudio.Plugins.UndoRedo
{
    internal class ImageResizePage : HistoryPage
    {
        Bitmap _before, _after;
        ImageEditControl _parent;

        public ImageResizePage(ImageEditControl parent, Image before, Image after)
        {
            _parent = parent;
            _before = new Bitmap(before);
            _after = new Bitmap(after);
        }

        public override void Undo() => _parent.SetImage(_before);

        public override void Redo() => _parent.SetImage(_after);

        public override void Dispose()
        {
            _before.Dispose();
            _after.Dispose();
        }
    }

    internal class ImagePage : HistoryPage
    {
        Point _pos;
        Image _before, _after;
        ImageEditControl _parent;

        public ImagePage(ImageEditControl parent, Point pos, Image before, Image after)
        {
            _pos = pos;
            _before = before;
            _after = after;
            _parent = parent;
        }

        public override void Undo()
        {
            using (Graphics g = Graphics.FromImage(_parent.EditImage))
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(_before, _pos);
            }
        }

        public override void Redo()
        {
            using (Graphics g = Graphics.FromImage(_parent.EditImage))
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.CompositingQuality = CompositingQuality.HighSpeed;
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawImage(_after, _pos);
            }
        }

        public override void Dispose()
        {
            _after?.Dispose();
            _after = null;

            _before?.Dispose();
            _before = null;
        }
    }
}
