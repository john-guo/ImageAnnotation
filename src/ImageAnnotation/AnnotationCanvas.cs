using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageAnnotation
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ImageAnnotation"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:ImageAnnotation;assembly=ImageAnnotation"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:CustomControl1/>
    ///
    /// </summary>
    public class AnnotationCanvas : Control
    {
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(BitmapSource), typeof(AnnotationCanvas), new PropertyMetadata(PropertyChangedRefresh));
        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(nameof(Scale), typeof(double), typeof(AnnotationCanvas), new PropertyMetadata(PropertyChangedRefresh));
        public static readonly DependencyProperty OffsetXProperty = DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(AnnotationCanvas), new PropertyMetadata(PropertyChangedRefresh));
        public static readonly DependencyProperty OffsetYProperty = DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(AnnotationCanvas), new PropertyMetadata(PropertyChangedRefresh));
        public static readonly DependencyProperty RemainProperty = DependencyProperty.Register(nameof(Remain), typeof(Thickness), typeof(AnnotationCanvas), new PropertyMetadata(PropertyChangedRefresh));
        public static readonly DependencyProperty ProviderProperty = DependencyProperty.Register(nameof(Provider), typeof(IAnnotationProvider), typeof(AnnotationCanvas), new PropertyMetadata(DrawModeChanged));
        public static readonly DependencyProperty ShapeColorProperty = DependencyProperty.Register(nameof(ShapeColor), typeof(Color), typeof(AnnotationCanvas), new PropertyMetadata(Colors.Black));
        public static readonly DependencyProperty ShapeThicknessProperty = DependencyProperty.Register(nameof(ShapeThickness), typeof(double), typeof(AnnotationCanvas), new PropertyMetadata(3.0));
        public static readonly DependencyProperty DrawModeProperty = DependencyProperty.Register(nameof(DrawMode), typeof(bool), typeof(AnnotationCanvas), new PropertyMetadata(false, DrawModeChanged, DrawModeCoerceValue));
        public static readonly DependencyProperty PivotRadiusProperty = DependencyProperty.Register(nameof(PivotRadius), typeof(double), typeof(AnnotationCanvas), new PropertyMetadata(7.0));
        public static readonly DependencyProperty PivotBrushProperty = DependencyProperty.Register(nameof(PivotBrush), typeof(Brush), typeof(AnnotationCanvas), new PropertyMetadata(new SolidColorBrush(Colors.Blue)));
        public static readonly DependencyProperty SelectedBrushProperty = DependencyProperty.Register(nameof(SelectedBrush), typeof(Brush), typeof(AnnotationCanvas), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(128, 0, 255, 0))));
        public static readonly DependencyProperty HoverBrushProperty = DependencyProperty.Register(nameof(HoverBrush), typeof(Brush), typeof(AnnotationCanvas), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(64, 0, 255, 0))));
        public static readonly DependencyProperty AnnotationsProperty = DependencyProperty.Register(nameof(Annotations), typeof(ObservableCollection<IAnnotation>), typeof(AnnotationCanvas), new PropertyMetadata(new ObservableCollection<IAnnotation>(), AnnotationsPropertyChanged));
        public static readonly DependencyProperty SelectedAnnotationProperty = DependencyProperty.Register(nameof(SelectedAnnotation), typeof(IAnnotation), typeof(AnnotationCanvas), new PropertyMetadata(null));

        private static void PropertyChangedRefresh(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (AnnotationCanvas)d;
            _this.InvalidateVisual();
        }

        private static void AnnotationsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (AnnotationCanvas)d;
            if (_this.Annotations != null)
            {
                _this.Annotations.CollectionChanged += (sender, cce) =>
                {
                    _this.InvalidateVisual();
                };
            }
            _this.InvalidateVisual();
        }

        private static object DrawModeCoerceValue(DependencyObject d, object baseValue)
        {
            var _this = (AnnotationCanvas)d;
            var drawMode = (bool)baseValue;
            if (drawMode)
            {
                return _this._tsm.Try(STATE.DRAWMODE);
            }
            else
            {
                return !_this._tsm.Try(STATE.INIT);
            }
        }

        private static void DrawModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = (AnnotationCanvas)d;
            if (_this.DrawMode)
            {
                _this.StartDrawAnnotation();
            }
            else
            {
                _this.EndDrawAnnotation();
            }
        }

        #region Properties
        public BitmapSource Image
        {
            get
            {
                return (BitmapSource)GetValue(ImageProperty);
            }
            set
            {
                SetValue(ImageProperty, value);
            }
        }

        public double Scale
        {
            get
            {
                return (double)GetValue(ScaleProperty); ;
            }
            set
            {
                SetValue(ScaleProperty, value);
            }
        }

        public double OffsetX
        {
            get
            {
                return (double)GetValue(OffsetXProperty); ;
            }
            set
            {
                SetValue(OffsetXProperty, value);
            }
        }

        public double OffsetY
        {
            get
            {
                return (double)GetValue(OffsetYProperty); ;
            }
            set
            {
                SetValue(OffsetYProperty, value);
            }
        }

        public Thickness Remain
        {
            get
            {
                return (Thickness)GetValue(RemainProperty); ;
            }
            set
            {
                SetValue(RemainProperty, value);
            }
        }

        public IAnnotationProvider Provider
        {
            get
            {
                return (IAnnotationProvider)GetValue(ProviderProperty); ;
            }
            set
            {
                SetValue(ProviderProperty, value);
            }
        }

        public Color ShapeColor
        {
            get
            {
                return (Color)GetValue(ShapeColorProperty); ;
            }
            set
            {
                SetValue(ShapeColorProperty, value);
            }
        }

        public double ShapeThickness
        {
            get
            {
                return (double)GetValue(ShapeThicknessProperty);
            }
            set
            {
                SetValue(ShapeThicknessProperty, value);
            }
        }

        public bool DrawMode
        {
            get
            {
                return (bool)GetValue(DrawModeProperty);
            }
            set
            {
                SetValue(DrawModeProperty, value);
            }
        }

        public double PivotRadius
        {
            get
            {
                return (double)GetValue(PivotRadiusProperty);
            }
            set
            {
                SetValue(PivotRadiusProperty, value);
            }
        }

        public Brush PivotBrush
        {
            get
            {
                return (Brush)GetValue(PivotBrushProperty);
            }
            set
            {
                SetValue(PivotBrushProperty, value);
            }
        }

        public Brush SelectedBrush
        {
            get
            {
                return (Brush)GetValue(SelectedBrushProperty);
            }
            set
            {
                SetValue(SelectedBrushProperty, value);
            }
        }

        public Brush HoverBrush
        {
            get
            {
                return (Brush)GetValue(HoverBrushProperty);
            }
            set
            {
                SetValue(HoverBrushProperty, value);
            }
        }

        public ObservableCollection<IAnnotation> Annotations
        {
            get
            {
                return (ObservableCollection<IAnnotation>)GetValue(AnnotationsProperty);
            }
            set
            {
                SetValue(AnnotationsProperty, value);
            }
        }

        public IAnnotation SelectedAnnotation
        {
            get
            {
                return (IAnnotation)GetValue(SelectedAnnotationProperty);
            }
            set
            {
                SetValue(SelectedAnnotationProperty, value);
            }
        }
        #endregion

        class Pivot
        {
            public Point Point { get; private set; }
            public Geometry Circle { get; private set; }
            public Pivot(Point point, double radius)
            {
                Point = point;
                Circle = new EllipseGeometry(point, radius, radius);
            }
        }

        private IAnnotation _currentAnnotation = null;
        private Point _lastPos;
        private readonly TinyStateMachine _tsm;
        private Pivot _selectedPivot;
        private List<Pivot> Pivots = new List<Pivot>();

        private static class STATE
        {
            public const int INIT = 0;
            public const int DRAG = 1;
            public const int DRAWMODE = 2;
            public const int DRAWING = 3;
            public const int EDITMODE = 4;
            public const int MOVING = 5;
            public const int EDITING = 6;
        }

        private void NewDrawAnnotation()
        {
            var annotation = Provider.Create();
            annotation.SetParameters(ShapeColor, ShapeThickness);
            _currentAnnotation = annotation;
        }

        private void DrawAnnotationPoint(Point point, bool isRelease = false)
        {
            if (isRelease && !_currentAnnotation.DragDraw)
                return;
            var result = _currentAnnotation.Add(point);
            if (result)
            {
                Annotations.Add(_currentAnnotation);
                NewDrawAnnotation();
            }
            else if (isRelease)
            {
                _currentAnnotation.Reset();
            }
        }

        internal void StartDrawAnnotation()
        {
            if (Provider == null)
                return;
            if (!_tsm.Next(STATE.DRAWMODE))
                return;
            NewDrawAnnotation();
        }

        internal void EndDrawAnnotation()
        {
            if (!_tsm.Next(STATE.INIT))
                return;
            _currentAnnotation = null;
            InvalidateVisual();
        }

        private void CancelSelect()
        {
            if (_tsm.State == STATE.EDITMODE)
                _tsm.Next(STATE.INIT);

            Pivots.Clear();
            SelectedAnnotation?.SetBrush(null);
            SelectedAnnotation = null;
            InvalidateVisual();
        }

        private IAnnotation FindAnnotation(Point point, bool resetBrush = true)
        {
            IAnnotation find = null;
            foreach (var annontation in Annotations)
            {
                if (resetBrush && annontation != SelectedAnnotation)
                    annontation.SetBrush(null);
                if (find != null)
                    continue;
                if (annontation.HitTest(point))
                {
                    find = annontation;
                }
            }
            return find;
        }

        private void GeneratePivots(Point? expect = null)
        {
            if (SelectedAnnotation == null)
                return;
            var points = SelectedAnnotation.GetVertices();
            Pivots.Clear();
            foreach (var point in points)
            {
                var pivot = new Pivot(point, PivotRadius);
                if (expect != null && expect == point)
                {
                    _selectedPivot = pivot;
                }
                Pivots.Add(pivot);
            }
            InvalidateVisual();
        }

        static AnnotationCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AnnotationCanvas), new FrameworkPropertyMetadata(typeof(AnnotationCanvas)));
        }

        public AnnotationCanvas()
        {
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);

            _tsm = new TinyStateMachine(new List<(int, int)>()
            {
                (STATE.INIT, STATE.INIT),
                (STATE.INIT, STATE.DRAG),
                (STATE.INIT, STATE.DRAWMODE),
                (STATE.INIT, STATE.EDITMODE),
                (STATE.DRAWMODE, STATE.DRAWMODE),
                (STATE.DRAWMODE, STATE.DRAWING),
                (STATE.DRAWMODE, STATE.INIT),
                (STATE.DRAG, STATE.INIT),
                (STATE.DRAWING, STATE.DRAWMODE),
                (STATE.DRAWING, STATE.DRAWING),
                (STATE.DRAWING, STATE.INIT),
                (STATE.EDITMODE, STATE.MOVING),
                (STATE.EDITMODE, STATE.EDITING),
                (STATE.EDITMODE, STATE.EDITMODE),
                (STATE.EDITMODE, STATE.DRAWMODE),
                (STATE.EDITMODE, STATE.INIT),
                (STATE.MOVING, STATE.EDITMODE),
                (STATE.EDITING, STATE.EDITMODE),
            });
            _tsm.Start(STATE.INIT);
        }

        private void DrawAnnotation(DrawingContext dc, IAnnotation annotation)
        {
            if (annotation == null)
                return;
            dc.DrawDrawing(annotation.Drawing);
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (Image == null)
                return;

            if (Scale == 0)
                Scale = 1;

            var trans = new TranslateTransform(OffsetX, OffsetY);
            var scale = new ScaleTransform(Scale, Scale);

            dc.PushTransform(scale);
            dc.PushTransform(trans);

            dc.DrawImage(Image, new Rect(0, 0, Image.PixelWidth, Image.PixelHeight));
            foreach (var annotation in Annotations)
            {
                DrawAnnotation(dc, annotation);
            }
            if (_currentAnnotation != null)
                DrawAnnotation(dc, _currentAnnotation);
            if (Pivots.Any())
            {
                var group = new GeometryGroup();
                Pivots.ForEach(p => group.Children.Add(p.Circle));
                dc.DrawGeometry(PivotBrush, null, group);
            }
            dc.Pop();
            dc.Pop();
        }

        private Point ImageToScreen(Point p)
        {
            var trans = new TranslateTransform(OffsetX, OffsetY);
            var scale = new ScaleTransform(Scale, Scale);

            return scale.Transform(trans.Transform(p));
        }

        private Point ScreenToImage(Point p)
        {
            var trans = new TranslateTransform(OffsetX, OffsetY);
            var scale = new ScaleTransform(Scale, Scale);
            if (!scale.Value.HasInverse || !trans.Value.HasInverse)
                return p;
            return trans.Inverse.Transform(scale.Inverse.Transform(p));
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var curr = e.GetPosition(this);
            var p1 = ScreenToImage(curr);

            var scale = Scale;
            if (e.Delta > 0)
                scale *= 1.25;
            else
                scale /= 1.25;

            Scale = Math.Min(30.0, Math.Max(ActualHeight / Image.PixelHeight / 1.25, scale));

            var p2 = ScreenToImage(curr);
            var v = p2 - p1;
            OffsetX += v.X;
            OffsetY += v.Y;

            InvalidateVisual();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                switch (_tsm.State)
                {
                    case STATE.DRAWING:
                        _tsm.Next(STATE.DRAWMODE);
                        _currentAnnotation?.Reset();
                        break;
                    case STATE.DRAG:
                        _tsm.Next(STATE.INIT);
                        break;
                    default:
                        break;
                }
                return;
            }

            var curr = e.GetPosition(this);
            var imgCurr = ScreenToImage(curr);
            _lastPos = curr;

            if (_tsm.State == STATE.EDITMODE)
            {
                _selectedPivot = Pivots.FirstOrDefault(p => p.Circle.FillContains(imgCurr));
                if (_selectedPivot != null && _tsm.Next(STATE.EDITING))
                {
                    return;
                }
                if (SelectedAnnotation != null && SelectedAnnotation.HitTest(imgCurr) && _tsm.Next(STATE.MOVING))
                {
                    return;
                }
            }

            var find = FindAnnotation(imgCurr, false);
            if (find != null && _tsm.Next(STATE.EDITMODE))
            {
                SelectedAnnotation?.SetBrush(null);
                SelectedAnnotation = find;
                SelectedAnnotation.SetBrush(SelectedBrush);
                GeneratePivots();

                if (SelectedAnnotation.HitTest(imgCurr) && _tsm.Next(STATE.MOVING))
                {
                    return;
                }
                return;
            }

            CancelSelect();

            if (_tsm.Next(STATE.DRAWING))
            {
                DrawAnnotationPoint(imgCurr);
                return;
            }

            _tsm.Next(STATE.DRAG);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var curr = e.GetPosition(this);
            var imgCurr = ScreenToImage(curr);
            var diff = imgCurr - ScreenToImage(_lastPos);

            switch (_tsm.State)
            {
                case STATE.DRAWING:
                    _currentAnnotation.Next(imgCurr);
                    break;
                case STATE.DRAG:
                    OffsetX += diff.X;
                    OffsetY += diff.Y;
                    break;
                case STATE.INIT:
                case STATE.EDITMODE:
                    IAnnotation find = FindAnnotation(imgCurr);
                    if (find != SelectedAnnotation)
                        find?.SetBrush(HoverBrush);
                    break;
                case STATE.EDITING:
                    if (_selectedPivot != null)
                    {
                        var point = SelectedAnnotation?.Move(_selectedPivot.Point, diff.X, diff.Y);
                        GeneratePivots(point);
                    }
                    break;
                case STATE.MOVING:
                    SelectedAnnotation?.Move(diff.X, diff.Y);
                    GeneratePivots();
                    break;
                default:
                    break;
            }

            _lastPos = curr;
            InvalidateVisual();
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
                return;

            var p = e.GetPosition(this);
            switch (_tsm.State)
            {
                case STATE.DRAWING:
                    DrawAnnotationPoint(ScreenToImage(p), true);
                    break;
                case STATE.DRAWMODE:
                    break;
                case STATE.EDITING:
                    _selectedPivot = null;
                    _tsm.Next(STATE.EDITMODE);
                    break;
                case STATE.MOVING:
                    _tsm.Next(STATE.EDITMODE);
                    break;
                case STATE.EDITMODE:
                    if (SelectedAnnotation == null)
                    {
                        _tsm.Next(STATE.INIT);
                    }
                    break;
                case STATE.DRAG:
                default:
                    _tsm.Next(STATE.INIT);
                    break;
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            _lastPos = e.GetPosition(this);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            _lastPos = e.GetPosition(this);
        }

        public void RemoveSelected()
        {
            if (SelectedAnnotation == null)
                return;
            Annotations.Remove(SelectedAnnotation);
            CancelSelect();
        }
    }
}
