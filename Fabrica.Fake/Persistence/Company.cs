using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fabrica.Models.Support;
using Microsoft.EntityFrameworkCore;

namespace Fabrica.Fake.Persistence;

[Index(nameof(Uid))]
[Index(nameof(Name))]
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

    [StringLength(25)]
    public override string Uid { get; set; } = "";

    [StringLength(100)]
    public string Name { get; set; } = "";

    [StringLength(100)]
    public string Address1 { get; set; } = "";
    [StringLength(100)]
    public string Address2 { get; set; } = "";

    [StringLength(50)]
    public string City { get; set; } = "";
    [StringLength(2)]
    public string State { get; set; } = "";
    [StringLength(15)]
    public string Zip { get; set; } = "";

    [StringLength(25)]
    public string MainPhone { get; set; } = "";
    [StringLength(25)]
    public string Fax { get; set; } = "";

    [StringLength(255)]
    public string Website { get; set; } = "";

    public int EmployeeCount { get; set; } = 0;


}