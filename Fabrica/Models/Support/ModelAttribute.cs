using System;

namespace Fabrica.Models.Support
{

    [AttributeUsage(AttributeTargets.Class)]
    public class ModelAttribute: Attribute
    {

        public bool Ignore { get; set; } = false;

        public string Alias { get; set; } = "";

        public string Resource { get; set; } = "";
        
        public Type RefersTo { get; set; }

        public Type Reference { get; set; }

        public Type Explorer { get; set; }

        public Type Rto { get; set; }

    }


}
