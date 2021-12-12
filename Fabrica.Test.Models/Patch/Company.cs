using System.Collections.ObjectModel;
using System.ComponentModel;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;
using Newtonsoft.Json;


#pragma warning disable CS8618
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable ConvertToAutoProperty


namespace Fabrica.Test.Models.Patch;

[JsonObject(MemberSerialization.OptIn)]
[Model]
public class Company: BaseMutableModel<Company>, IRootModel, INotifyPropertyChanged
{


    public Company() : this(false)
    {

    }

    public Company(bool added)
    {

        SuspendTracking(m =>
        {
            Employees = new Collection<Person>();

        });

        if (added)
            Added();

    }


    private long _id;
    public override long Id
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

    [JsonProperty("Name")]
    private string _name = "";
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    [JsonProperty("Address1")]
    private string _address1 = "";
    public string Address1
    {
        get { return _address1; }
        set { _address1 = value; }
    }

    [JsonProperty("Address2")]
    private string _address2 = "";
    public string Address2
    {
        get { return _address2; }
        set { _address2 = value; }
    }

    [JsonProperty("City")]
    private string _city = "";
    public string City
    {
        get { return _city; }
        set { _city = value; }
    }

    [JsonProperty("State")]
    private string _state = "";
    public string State
    {
        get { return _state; }
        set { _state = value; }
    }

    [JsonProperty("Zip")]
    private string _zip = "";
    public string Zip
    {
        get { return _zip; }
        set { _zip = value; }
    }

    [JsonProperty("MainPhone")]
    private string _mainPhone = "";
    public string MainPhone
    {
        get { return _mainPhone; }
        set { _mainPhone = value; }
    }

    [JsonProperty("Fax")]
    private string _fax = "";
    public string Fax
    {
        get { return _fax; }
        set { _fax = value; }
    }

    [JsonProperty("Website")]
    private string _website = "";
    public string Website
    {
        get { return _website; }
        set { _website = value; }
    }

    [JsonProperty("EmployeeCount")]
    private int _employeeCount = 0;
    public int EmployeeCount
    {
        get { return _employeeCount; }
        set { _employeeCount = value; }
    }


    private AggregateObservable<Person> _employees;
    public ICollection<Person> Employees
    {
        get => _employees;
        set => _employees = new AggregateObservable<Person>(this, "Employees", value);
    }


}