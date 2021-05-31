using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Fabrica.Utilities.Types
{


    public class PropertySorterConverter : TypeConverter
    {

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {

            var list = new List<PropertyDescriptor>();
            foreach( PropertyDescriptor pd in TypeDescriptor.GetProperties(value, attributes) )
                list.Add(pd);

            var allProperties = list
                .Select(x => new
                {
                    PropertyDescriptor  = x,
                    Attribute = (PropertySortOrder)x.Attributes[typeof(PropertySortOrder)]
                })
                .OrderBy(x => x.Attribute?.Order??9999)
                .Select(a=>a.PropertyDescriptor)
                .ToArray();

            var result = new PropertyDescriptorCollection( allProperties );

            return result;

        }


    }


}
