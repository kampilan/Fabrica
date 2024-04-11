using System.Collections.ObjectModel;
using System.ComponentModel;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;


#pragma warning disable CS8618
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable ConvertToAutoProperty


namespace Fabrica.Test.Models.Patch;

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

    private string _name = "";
    [ModelMeta]
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    private string _address1 = "";
    [ModelMeta]
    public string Address1
    {
        get { return _address1; }
        set { _address1 = value; }
    }

    private string _address2 = "";
    [ModelMeta]
    public string Address2
    {
        get { return _address2; }
        set { _address2 = value; }
    }

    private string _city = "";
    public string City
    {
        get { return _city; }
        set { _city = value; }
    }

    private string _state = "";
    [ModelMeta]
    public string State
    {
        get { return _state; }
        set { _state = value; }
    }

    private string _zip = "";
    [ModelMeta]
    public string Zip
    {
        get { return _zip; }
        set { _zip = value; }
    }

    private string _mainPhone = "";
    [ModelMeta]
    public string MainPhone
    {
        get { return _mainPhone; }
        set { _mainPhone = value; }
    }

    private string _fax = "";
    [ModelMeta]
    public string Fax
    {
        get { return _fax; }
        set { _fax = value; }
    }

    private string _website = "";
    [ModelMeta]
    public string Website
    {
        get { return _website; }
        set { _website = value; }
    }

    private int _employeeCount = 0;
    [ModelMeta]
    public int EmployeeCount
    {
        get { return _employeeCount; }
        set { _employeeCount = value; }
    }


    private AggregateObservable<Person> _employees;
    [ModelMeta]
    public ICollection<Person> Employees
    {
        get => _employees;
        set => _employees = new AggregateObservable<Person>(this, "Employees", value);
    }


}