using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImageAnnotation
{
    [AnnotationTool(
        Icon = AnnotationResource.ICON_POLYGON,
        Description = AnnotationResource.DESC_POLYGON)]
    public class PolygonAnnotation : GeometryAnnotation
    {
        private List<Point> _points = new List<Point>();
        private PathGeometry _path;
        private PathFigure _pathFigure;
        private PolyLineSegment _segment;
        private EllipseGeometry _sensitiveArea;
        private GeometryGroup _group;
        private const double _sensitiveDistance = 10;

        public override string Name => "Polygon";

        private bool CanAdd => !_pathFigure.IsClosed;

        private bool SensitiveClosed(Point point)
        {
            var v = _pathFigure.StartPoint - point;
            return v.Length <= _sensitiveDistance;
        }

        public override bool Add(Point point)
        {
            if (!CanAdd)
                return true;
            if (!_points.Any())
            {
                _pathFigure.StartPoint = point;
            }
            else if (_points.Count > 2 && SensitiveClosed(point))
            {
                if (_path.Bounds.Width < _sensitiveDistance 
                    || _path.Bounds.Height < _sensitiveDistance)
                    return false;

                var len = _segment.Points.Count;
                if (_segment.Points[len - 1] == _pathFigure.StartPoint)
                {
                    _segment.Points.RemoveAt(len - 1);
                }
                _pathFigure.IsClosed = true;
                _points.Clear();
                _sensitiveArea.RadiusX = 0;
                _sensitiveArea.RadiusY = 0;
                return true;
            } 

            _points.Add(point);
            return false;
        }

        public override List<Point> GetVertices()
        {
            var points = new List<Point>();
            points.Add(_pathFigure.StartPoint);
            points.AddRange(_segment.Points);
            return points;
        }

        public override void Move(double dx, double dy)
        {
            var v = new Vector(dx, dy);
            _pathFigure.StartPoint += v;
            for (int i = 0; i < _segment.Points.Count; ++i)
            {
                _segment.Points[i] += v;
            }
        }

        public override Point Move(Point point, double dx, double dy)
        {
            var v = new Vector(dx, dy);
            if (_pathFigure.StartPoint == point)
            {
                return _pathFigure.StartPoint += v;
            }
            for (int i = 0; i < _segment.Points.Count; ++i)
            {
                if (_segment.Points[i] == point)
                {
                    return _segment.Points[i] += v;
                }
            }
            return point;
        }

        public override void Next(Point point)
        {
            if (!CanAdd || !_points.Any())
                return;
            if (_points.Count > 2 && SensitiveClosed(point))
            {
                _sensitiveArea.Center = _pathFigure.StartPoint;
                _sensitiveArea.RadiusX = _sensitiveDistance;
                _sensitiveArea.RadiusY = _sensitiveDistance;
                point = _pathFigure.StartPoint;
            }
            else
            {
                _sensitiveArea.RadiusX = 0;
                _sensitiveArea.RadiusY = 0;
            }
            var len = _points.Count - 1;
            if (_segment.Points.Count <= len)
                _segment.Points.Add(point);
            else
                _segment.Points[len] = point;
        }

        public override void Reset()
        {
            _pathFigure.IsClosed = false;
            _pathFigure.Segments.Clear();
            _points.Clear();
            _sensitiveArea.RadiusX = 0;
            _sensitiveArea.RadiusY = 0;
        }

        protected override Geometry CreateGeometry()
        {
            _group = new GeometryGroup();
            _path = new PathGeometry();
            _pathFigure = new PathFigure()
            {
                IsClosed = false,
            };
            _segment = new PolyLineSegment();
            _pathFigure.Segments.Add(_segment);
            _path.Figures.Add(_pathFigure);
            _group.Children.Add(_path);
            _sensitiveArea = new EllipseGeometry();
            _group.Children.Add(_sensitiveArea);
            return _group;
        }

        public override ExpandoObject Serialize()
        {
            dynamic e = base.Serialize();
            if (CanAdd)
                return e;
            e.Points = GetVertices();
            return e;
        }

        public override void Deserialize(ExpandoObject expando)
        {
            base.Deserialize(expando);
            dynamic e = expando;
            var points = (e.Points as IEnumerable<object>).Select(p => Point.Parse((string)p)).ToList();
            _pathFigure.StartPoint = points[0];
            _pathFigure.IsClosed = true;
            _segment.Points = new PointCollection(points.Skip(1));
        }
    }
}
