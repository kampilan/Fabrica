using System.ComponentModel.DataAnnotations.Schema;
using Fabrica.Models.Support;

namespace Fabrica.Fake.Persistence;

public class Company : BaseMutableModel<Company>, IRootModel, IExplorableModel
{


    [NotMapped]
    public long IdSetter
    {
        get { return _id; }
        set { _id = value; }
    }

    private long _id;
    public override long Id
    {
        get => _id;
        protected set { }
    }

    public override string Uid { get; set; } = "";

    public string Name { get; set; } = "";

    public string Address1 { get; set; } = "";
    public string Address2 { get; set; } = "";

    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string Zip { get; set; } = "";

    public string MainPhone { get; set; } = "";
    public string Fax { get; set; } = "";

    public string Website { get; set; } = "";

    public int EmployeeCount { get; set; } = 0;


}