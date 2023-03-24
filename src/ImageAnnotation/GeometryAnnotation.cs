using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImageAnnotation
{
    public abstract class GeometryAnnotation : IAnnotation
    {
        private Color _color;
        private double _thickness;

        protected abstract Geometry CreateGeometry();
        public virtual bool DragDraw => false;
        public abstract bool Add(Point point);
        public abstract void Next(Point point);

        public void SetParameters(Color color, double thickness)
        {
            _color = color;
            _thickness = thickness;
            _drawing.Pen = new Pen(new SolidColorBrush(_color), _thickness);
        }

        protected readonly GeometryDrawing _drawing;

        public GeometryAnnotation()
        {
            _drawing = new GeometryDrawing()
            {
                Geometry = CreateGeometry(),
            };
        }

        public Drawing Drawing => _drawing;


        public void SetBrush(Brush brush)
        {
            _drawing.Brush = brush;
        }

        public bool HitTest(Point point)
        {
            return _drawing.Geometry.FillContains(point);
        }

        public virtual double X => _drawing.Geometry.Bounds.X;
        public virtual double Y => _drawing.Geometry.Bounds.Y;
        public virtual double Width => _drawing.Geometry.Bounds.Width;
        public virtual double Height => _drawing.Geometry.Bounds.Height;

        public abstract string Name { get; }
        public abstract void Reset();
        public abstract void Move(double dx, double dy);
        public abstract Point Move(Point point, double dx, double dy);
        public abstract List<Point> GetVertices();

        public override string ToString()
        {
            return $"{Name}_{X}_{Y}_{Width}_{Height}";
        }

        public virtual ExpandoObject Serialize()
        {
            dynamic e = new ExpandoObject();
            e.Color = _color;
            e.Thickness = _thickness;
            return e;
        }

        public virtual void Deserialize(ExpandoObject expando)
        {
            dynamic e = expando;
            var color = (Color)ColorConverter.ConvertFromString(e.Color);
            var thickness = e.Thickness;
            SetParameters(color, thickness);
        }
    }
}
