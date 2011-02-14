using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace Lemon
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportOverlayWindowAttribute : ExportAttribute
    {
        public ExportOverlayWindowAttribute(string title)
            : base(title, typeof(IOverlay))
        {}
    }
}
