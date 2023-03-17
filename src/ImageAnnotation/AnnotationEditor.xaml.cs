using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
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
    /// Interaction logic for AnnotationEditor.xaml
    /// </summary>
    public partial class AnnotationEditor : UserControl
    {
        public static readonly DependencyProperty ImageProperty = 
            DependencyProperty.Register(nameof(Image), typeof(BitmapSource), typeof(AnnotationEditor), new PropertyMetadata(default));
        public static readonly DependencyProperty AnnotationsProperty = 
            DependencyProperty.Register(nameof(Annotations), typeof(ObservableCollection<IAnnotation>), typeof(AnnotationEditor), new PropertyMetadata(new ObservableCollection<IAnnotation>()));
        public static readonly DependencyProperty SelectedAnnotationProperty = 
            DependencyProperty.Register(nameof(SelectedAnnotation), typeof(IAnnotation), typeof(AnnotationEditor), new PropertyMetadata(OnSelectedAnnotationChanged));
        public static readonly DependencyProperty ChooseToolProperty =
            DependencyProperty.Register(nameof(ChooseTool), typeof(ICommand), typeof(AnnotationEditor), new PropertyMetadata(default));
        public static readonly DependencyProperty ShapeColorProperty = 
            DependencyProperty.Register(nameof(ShapeColor), typeof(Color), typeof(AnnotationEditor), new PropertyMetadata(Colors.Black));
        public static readonly DependencyProperty ShapeThicknessProperty = 
            DependencyProperty.Register(nameof(ShapeThickness), typeof(double), typeof(AnnotationEditor), new PropertyMetadata(3.0));
        public static readonly DependencyProperty RemoveProperty =
            DependencyProperty.Register(nameof(Remove), typeof(ICommand), typeof(AnnotationEditor), new PropertyMetadata(default));

        private static void OnSelectedAnnotationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = (AnnotationEditor)d;
            (editor.Remove as RemoveCommand).RaiseCanExecuteChanged();
        }

        public BitmapSource Image
        {
            get { return (BitmapSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
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
            get { return (IAnnotation)GetValue(SelectedAnnotationProperty); }
            set { SetValue(SelectedAnnotationProperty, value); }
        }

        public Color ShapeColor
        {
            get
            {
                return (Color)GetValue(ShapeColorProperty);
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

        public ICommand ChooseTool
        {
            get { return (ICommand)GetValue(ChooseToolProperty); }
            set { SetValue(ChooseToolProperty, value); }
        }

        public ICommand Remove
        {
            get { return (ICommand)GetValue(RemoveProperty); }
            set { SetValue(RemoveProperty, value); }
        }

        public class ToolCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;
            private readonly AnnotationEditor _editor;
            public ToolCommand(AnnotationEditor editor)
            {
                _editor = editor;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                var tool = parameter as AnnotationTool;
                _editor.drawArea.Provider = tool.Provider;
                _editor.drawArea.DrawMode = tool.Provider != null;
                _editor.drawArea.Cursor = tool.Cursor;
            }
        }

        public class RemoveCommand : ICommand
        {
            public event EventHandler CanExecuteChanged;
            private readonly AnnotationEditor _editor;
            public RemoveCommand(AnnotationEditor editor)
            {
                _editor = editor;
            }

            public bool CanExecute(object parameter)
            {
                return _editor.SelectedAnnotation != null;
            }

            public void Execute(object parameter)
            {
                _editor.drawArea.RemoveSelected();
            }

            public void RaiseCanExecuteChanged()
            {
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public AnnotationEditor()
        {
            InitializeComponent();
            ChooseTool = new ToolCommand(this);
            Remove = new RemoveCommand(this);
            drawTools.ItemsSource = AnnotationManager.AnnotationTools;
        }
    }
}
