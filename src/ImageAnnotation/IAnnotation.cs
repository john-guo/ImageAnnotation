using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ImageAnnotation
{
    public interface IAnnotation
    {
        Drawing Drawing { get; }
        string Name { get; }
        double X { get; }
        double Y { get; }
        double Width { get; }
        double Height { get; }
        bool DragDraw { get; }

        void SetParameters(Color color, double thickness);
        bool Add(Point point);
        void Next(Point point);
        void SetBrush(Brush brush);
        bool HitTest(Point point);
        List<Point> GetVertices();
        void Move(double dx, double dy);
        Point Move(Point point, double dx, double dy);
        void Reset();
        ExpandoObject Serialize();
        void Deserialize(ExpandoObject expando);
    }
}
