using ImageAnnotation;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Windows.Input;

namespace Demo
{
    internal class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<IAnnotation> Annotations { get; set; }
        public ICommand Export { get; set; }
        public ICommand Import { get; set; }
        public IAnnotation SelectedAnnotation { get; set; }

        private void OnSelectedAnnotationChanged()
        {
            var selected = SelectedAnnotation;
        }

        public MainWindowViewModel()
        {
            Annotations = new ObservableCollection<IAnnotation>();
            Export = new DelegateCommand(args =>
            {
                var array = new List<ExpandoObject>();
                foreach (var annotation in Annotations)
                {
                    var jsonObj = AnnotationManager.Serialize(annotation);
                    array.Add(jsonObj);
                }
                
                var json = JsonConvert.SerializeObject(array);
                File.WriteAllText("test.json", json);
                Debug.WriteLine(json);
            });

            Import = new DelegateCommand(args =>
            {
                Annotations.Clear();
                var json = File.ReadAllText("test.json");
                var array = JsonConvert.DeserializeObject<List<ExpandoObject>>(json);
                foreach (var jsonObj in array)
                {
                    var annotation = AnnotationManager.Deserialize(jsonObj);
                    Annotations.Add(annotation);
                }
            });
        }
    }
}
