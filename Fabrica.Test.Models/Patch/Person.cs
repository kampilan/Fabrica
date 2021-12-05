using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Types;
using System.ComponentModel;

namespace Fabrica.Test.Models.Patch
{

    [Model(Alias = nameof(Person))]
    public class Person : BaseMutableModel<Person>, IAggregateModel, INotifyPropertyChanged
    {


        private long _id = 0;
        public override long Id
        {
            get => _id;
            protected set => _id = value;
        }


        private string _uid = ShortGuid.NewGuid();
        [ModelMeta(Scope = PropertyScope.Immutable)]
        public override string Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }


        private Company _parent = default;
        public Company Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public void SetParent(object parent)
        {
        }

        private string _firstName = "";
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        private string _lastName = "";
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }



    }

}
