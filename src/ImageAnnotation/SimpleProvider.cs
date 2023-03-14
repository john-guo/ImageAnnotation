using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageAnnotation
{
    public class SimpleProvider : IAnnotationProvider
    {
        public IAnnotation Create()
        {
            return new RectangleAnnotation();
        }
    }
}
