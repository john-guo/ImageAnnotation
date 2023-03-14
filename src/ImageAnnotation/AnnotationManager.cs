using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Annotations;
using System.Windows.Input;

namespace ImageAnnotation
{
    public static class AnnotationResource
    {
        public const string DESC_POINTER = "鼠标指针";
        public const string DESC_RECTANGLE = "矩形工具";
        public const string DESC_POLYGON = "多边形工具";
        public const string ICON_POINTER = "/Assets/pointer.png";
        public const string ICON_RECTANGLE = "/Assets/rectangle.png";
        public const string ICON_POLYGON = "/Assets/polygon.png";
        private readonly static string _AsmShortName;

        static AnnotationResource()
        {
            _AsmShortName = typeof(AnnotationResource).Assembly.ToString().Split(',')[0];
        }

        public static string GetUri(string filename)
        {
            return $"pack://application:,,,/{_AsmShortName};component/{filename}";
        }
    }

    public class AnnotationTool : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Cursor Cursor { get; set; }
        public IAnnotationProvider Provider { get; set; }
        public bool Selected { get; set; }
        public string Icon { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AnnotationToolAttribute : Attribute
    {
        public string Description { get; set; }
        public string Icon { get; set; }
        public string IconUri => AnnotationResource.GetUri(Icon);
    }

    public class DelegateAnnotationProvider : IAnnotationProvider
    {
        private readonly Func<IAnnotation> _creator;
        public DelegateAnnotationProvider(Func<IAnnotation> creator)
        {
            _creator = creator;
        }

        public IAnnotation Create()
        {
            return _creator();
        }
    }

    public static class AnnotationManager
    {
        public readonly static ObservableCollection<AnnotationTool> AnnotationTools = new ObservableCollection<AnnotationTool>();
        private readonly static Dictionary<string, Type> _annotationTypes;
        
        static AnnotationManager()
        {
            var iat = typeof(IAnnotation);
            _annotationTypes = typeof(AnnotationManager).Assembly.GetExportedTypes().Where(type =>
            {
                return !type.IsAbstract && iat.IsAssignableFrom(type);
            }).ToDictionary(type =>
            {
                var annotation = (IAnnotation)Activator.CreateInstance(type);
                return annotation.Name;
            });
        }

        public static void RegisterDefault()
        {
            RegisterTool("Pointer", 
                toolDescription: AnnotationResource.DESC_POINTER, 
                toolIcon: AnnotationResource.GetUri(AnnotationResource.ICON_POINTER));

            foreach (var pair in _annotationTypes)
            {
                var ata = pair.Value.GetCustomAttribute<AnnotationToolAttribute>();
                RegisterTool(pair.Key,
                    () => (IAnnotation)Activator.CreateInstance(pair.Value),
                    ata?.Description,
                    Cursors.Pen,
                    ata?.IconUri);
            }
        }

        public static ExpandoObject Serialize(IAnnotation annotation)
        {
            var e = annotation.Serialize();
            dynamic jsonObj = new ExpandoObject();
            jsonObj.Name = annotation.Name;
            jsonObj.Data = annotation.Serialize();
            return jsonObj;
        }

        public static IAnnotation Deserialize(ExpandoObject jsonObj)
        {
            dynamic e = jsonObj;
            if (!_annotationTypes.TryGetValue(e.Name, out Type annotationType))
                return null;
            var annotation = (IAnnotation)Activator.CreateInstance(annotationType);
            annotation.Deserialize(e.Data);
            return annotation;
        }

        public static void RegisterTool(string toolName, 
            Func<IAnnotation> toolCreator = null, 
            string toolDescription = null, 
            Cursor toolCursor = null,
            string toolIcon = null)
        {
            AnnotationTools.Add(new AnnotationTool()
            {
                Name = toolName,
                Description = toolDescription ?? string.Empty,
                Provider = toolCreator != null ? new DelegateAnnotationProvider(toolCreator) : null,
                Cursor = toolCursor ?? Cursors.Arrow,
                Icon = toolIcon ?? string.Empty,
            });
        }


    }
}
