using System.Collections.ObjectModel;
using System.ComponentModel;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;
using Fabrica.Utilities.Types;


#pragma warning disable CS8618
// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable ConvertToAutoProperty


namespace Fabrica.Test.Models.Patch;


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


    private string _uid = Base62Converter.NewGuid();
    [ModelMeta(Scope = PropertyScope.Immutable)]
    public override string Uid
    {
        get { return _uid; }
        set { _uid = value; }
    }

    private string _name = "";
    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }

    private string _city = "";
    public string City
    {
        get { return _city; }
        set { _city = value; }
    }

    private AggregateObservable<Person> _employees;
    public ICollection<Person> Employees
    {
        get => _employees;
        set => _employees = new AggregateObservable<Person>(this, "Employees", value);
    }


}