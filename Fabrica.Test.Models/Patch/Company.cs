using System.ComponentModel;
using Fabrica.Models.Support;
using Fabrica.Utilities.Types;

namespace Fabrica.Test.Models.Patch;


public class Company: BaseMutableModel<Company>, INotifyPropertyChanged
{

    public override long Id { get; protected set; }
    public override string Uid { get; set; } = ShortGuid.NewGuid();

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