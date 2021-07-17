using System;
using System.Collections.Generic;
using System.Linq;
using Fabrica.Utilities.Text;
using Fabrica.Utilities.Types;

namespace Fabrica.Press.Generation.DataSources
{

    
    public class ModelDataSource<TModel>: IMergeDataSource
    {


        public ModelDataSource( IEnumerable<TModel> input)
        {
            Region =typeof(TModel).Name.Pluralize();
            List   = new List<ExpandoWrapper<TModel>>( input.Select(m=>new ExpandoWrapper<TModel>(m)));
        }

        public ModelDataSource( params TModel[] input)
        {

            Region = typeof(TModel).Name.Pluralize();
            List   = new List<ExpandoWrapper<TModel>>(input.Select(m => new ExpandoWrapper<TModel>(m)));

        }


        public ModelDataSource(string region, IEnumerable<TModel> input)
        {

            Region = region;
            List   = new List<ExpandoWrapper<TModel>>(input.Select(m => new ExpandoWrapper<TModel>(m)));

        }

        public ModelDataSource(string region, params TModel[] input)
        {

            Region = region;
            List   = new List<ExpandoWrapper<TModel>>(input.Select(m => new ExpandoWrapper<TModel>(m)));

        }



        public string Region { get; }



        public void AddDerivedProperty( string name, Func<TModel, object> getter )
        {
            List.ForEach( w=>w.AddDerived(name, getter));
        }

        private List<ExpandoWrapper<TModel>> List { get; }

        public dynamic Current => List[Index];


        private int Index { get; set; } = -1;
        public void Rewind()
        {
            Index = -1;
        }

        public bool MoveNext()
        {

            Index++;

            if( Index >= List.Count )
                return false;

            return true;

        }

        public bool TryGetValue(string spec, out object value)
        {

            var ms = MergeField.Parse(spec);

            value = null;
            if( Current.HasValue(ms.Name) )
            {
                value = Current.Get(ms.Name);
                return true;
            }

            return value != null;

        }


    }

}
