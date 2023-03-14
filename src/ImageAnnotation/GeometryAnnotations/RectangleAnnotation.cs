using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImageAnnotation
{
    [AnnotationTool(
        Icon = AnnotationResource.ICON_RECTANGLE, 
        Description = AnnotationResource.DESC_RECTANGLE)]
    public class RectangleAnnotation : GeometryAnnotation
    {
        private const int MIN_LENGTH = 10;
        private RectangleGeometry _rect;
        private List<Point> _points = new List<Point>();

        private bool CanAdd => _rect != null && _points.Count < 2;

        public override string Name => "Rectangle";
        public override bool DragDraw => true;

        public override bool Add(Point point)
        {
            if (!CanAdd)
                return true;
            if (_points.Count == 1)
            {
                var rect = new Rect(_points[0], point);
                if (rect.Width < MIN_LENGTH || rect.Height < MIN_LENGTH)
                    return false;
            }
            _points.Add(point);
            if (!CanAdd)
            {
                _rect.Rect = new Rect(_points[0], _points[1]);
                _points.Clear();
                return true;
            }
            return false;
        }

        public override void Move(double dx, double dy)
        {
            var r = _rect.Rect;
            r.Offset(dx, dy);
            _rect.Rect = r;
        }

        public override Point Move(Point point, double dx, double dy)
        {
            if (point == _rect.Rect.TopLeft)
            {
                var tl = _rect.Rect.TopLeft;
                tl.Offset(dx, dy);
                var rect = new Rect(tl, _rect.Rect.BottomRight);
                rect.Width = Math.Max(rect.Width, MIN_LENGTH);
                rect.Height = Math.Max(rect.Height, MIN_LENGTH);
                _rect.Rect = rect;
                return _rect.Rect.TopLeft;
            }
            else if (point == _rect.Rect.TopRight)
            {
                var tl = _rect.Rect.TopLeft;
                var br = _rect.Rect.BottomRight;
                tl.Offset(0, dy);
                br.Offset(dx, 0);
                var rect = new Rect(tl, br);
                rect.Width = Math.Max(rect.Width, MIN_LENGTH);
                rect.Height = Math.Max(rect.Height, MIN_LENGTH);
                _rect.Rect = rect;
                return _rect.Rect.TopRight;
            }
            else if (point == _rect.Rect.BottomLeft)
            {
                var tl = _rect.Rect.TopLeft;
                var br = _rect.Rect.BottomRight;
                tl.Offset(dx, 0);
                br.Offset(0, dy);
                var rect = new Rect(tl, br);
                rect.Width = Math.Max(rect.Width, MIN_LENGTH);
                rect.Height = Math.Max(rect.Height, MIN_LENGTH);
                _rect.Rect = rect;
                return _rect.Rect.BottomLeft;
            }
            else //(point == _rect.Rect.BottomRight)
            {
                var br = _rect.Rect.BottomRight;
                br.Offset(dx, dy);
                var rect = new Rect(_rect.Rect.TopLeft, br);
                rect.Width = Math.Max(rect.Width, MIN_LENGTH);
                rect.Height = Math.Max(rect.Height, MIN_LENGTH);
                _rect.Rect = rect;
                return _rect.Rect.BottomRight;
            }
        }

        public override List<Point> GetVertices()
        {
            var ret = new List<Point>();
            ret.Add(_rect.Rect.TopLeft);
            ret.Add(_rect.Rect.TopRight);
            ret.Add(_rect.Rect.BottomLeft);
            ret.Add(_rect.Rect.BottomRight);
            return ret;
        }

        public override void Next(Point point)
        {
            if (!CanAdd || _points.Count != 1)
                return;
            _rect.Rect = new Rect(_points[0], point);
        }

        public override void Reset()
        {
            _rect.Rect = Rect.Empty;
            _points.Clear();
        }

        protected override Geometry CreateGeometry()
        {
            return _rect = new RectangleGeometry();
        }

        public override ExpandoObject Serialize()
        {
            dynamic e = base.Serialize();
            var rect = _rect.Rect;
            e.X1 = rect.Left;
            e.Y1 = rect.Top;
            e.X2 = rect.Right;
            e.Y2 = rect.Bottom;
            return e;
        }

        public override void Deserialize(ExpandoObject expando)
        {
            base.Deserialize(expando);
            dynamic e = expando;
            _rect.Rect = new Rect(new Point(e.X1, e.Y1), new Point(e.X2, e.Y2));
        }
    }
}
