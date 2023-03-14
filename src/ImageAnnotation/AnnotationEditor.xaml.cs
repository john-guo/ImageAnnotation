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
            DependencyProperty.Register(nameof(SelectedAnnotation), typeof(IAnnotation), typeof(AnnotationEditor), new PropertyMetadata(default));
        public static readonly DependencyProperty ChooseToolProperty =
            DependencyProperty.Register(nameof(ChooseTool), typeof(ICommand), typeof(AnnotationEditor), new PropertyMetadata(default));

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

        public ICommand ChooseTool
        {
            get { return (ICommand)GetValue(ChooseToolProperty); }
            set { SetValue(ChooseToolProperty, value); }
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

        public AnnotationEditor()
        {
            InitializeComponent();
            ChooseTool = new ToolCommand(this);
            drawTools.ItemsSource = AnnotationManager.AnnotationTools;
        }
    }
}
