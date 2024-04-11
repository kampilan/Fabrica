using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Fabrica.Utilities.Text;

#pragma warning disable CS8618
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable ConvertToAutoProperty

namespace Fabrica.Test.Models.Patch;


[Model]
public class Person : BaseMutableModel<Person>, IAggregateModel, INotifyPropertyChanged
{

    public enum GenderKind { Female, Male }


    private long _id;
    [ModelMeta(Scope = PropertyScope.Exclude)]
    public long Id
    {
        get => _id;
        protected set => _id = value;
    }

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

    private string _firstName = "";
    [ModelMeta]
    public string FirstName
    {
        get { return _firstName; }
        set { _firstName = value; }
    }

    private string _middleName = "";
    [ModelMeta]
    public string MiddleName
    {
        get { return _middleName; }
        set { _middleName = value; }
    }

    private string _lastName = "";
    [ModelMeta]
    public string LastName
    {
        get { return _lastName; }
        set { _lastName = value; }
    }

    [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringEnumConverter))]
    private GenderKind _gender = GenderKind.Female;
    public GenderKind Gender
    {
        get { return _gender;}
        set { _gender = value; }
    }

    private DateTime _birthDate = DateTime.Now.AddYears(-25).Date;
    [ModelMeta]
    public DateTime BirthDate
    {
        get { return _birthDate; }
        set { _birthDate = value; }
    }

    private string _phoneNumber = "";
    [ModelMeta]
    public string PhoneNumber
    {
        get { return _phoneNumber; }
        set { _phoneNumber = value; }
    }

    private string _email = "";
    [ModelMeta]
    public string Email
    {
        get { return _email; }
        set { _email = value; }
    }

    private decimal _salary = 0;
    [ModelMeta]
    public decimal Salary
    {
        get { return _salary;}
        set { _salary = value; }
    }


}