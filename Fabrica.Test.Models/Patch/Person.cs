using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using System.ComponentModel;
using Fabrica.Utilities.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

#pragma warning disable CS8618
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable ConvertToAutoProperty

namespace Fabrica.Test.Models.Patch
{

    [JsonObject(MemberSerialization.OptIn)]
    [Model]
    public class Person : BaseMutableModel<Person>, IAggregateModel, INotifyPropertyChanged
    {

        public enum GenderKind { Female, Male }


        private long _id;
        public long Id
        {
            get => _id;
            protected set => _id = value;
        }

        [JsonProperty("Uid")]
        private string _uid = Base62Converter.NewGuid();
        [ModelMeta(Scope = PropertyScope.Immutable)]
        public override string Uid
        {
            get { return _uid; }
            set { _uid = value; }
        }


        private Company _parent;
        public Company Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        public void SetParent(object parent)
        {
        }

        [JsonProperty("FirstName")]
        private string _firstName = "";
        public string FirstName
        {
            get { return _firstName; }
            set { _firstName = value; }
        }

        [JsonProperty("MiddleName")]
        private string _middleName = "";
        public string MiddleName
        {
            get { return _middleName; }
            set { _middleName = value; }
        }

        [JsonProperty("LastName")]
        private string _lastName = "";
        public string LastName
        {
            get { return _lastName; }
            set { _lastName = value; }
        }

        [JsonProperty("Gender")]
        [JsonConverter(typeof(StringEnumConverter))]
        private GenderKind _gender = GenderKind.Female;
        public GenderKind Gender
        {
            get { return _gender;}
            set { _gender = value; }
        }

        [JsonProperty("BirthDate")]
        private DateTime _birthDate = DateTime.Now.AddYears(-25).Date;
        public DateTime BirthDate
        {
            get { return _birthDate; }
            set { _birthDate = value; }
        }

        [JsonProperty("PhoneNumber")]
        private string _phoneNumber = "";
        public string PhoneNumber
        {
            get { return _phoneNumber; }
            set { _phoneNumber = value; }
        }

        [JsonProperty("Email")]
        private string _email = "";
        public string Email
        {
            get { return _email; }
            set { _email = value; }
        }

        [JsonProperty("Salary")]
        private decimal _salary = 0;
        public decimal Salary
        {
            get { return _salary;}
            set { _salary = value; }
        }


    }

}
