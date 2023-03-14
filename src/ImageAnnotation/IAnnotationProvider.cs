using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnnotation
{
    public interface IAnnotationProvider
    {
        IAnnotation Create();
    }
}
