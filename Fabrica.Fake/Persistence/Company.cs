using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Fabrica.Fake.Persistence;

[Index(nameof(Uid))]
[Index(nameof(Name))]
[JsonObject(MemberSerialization.OptIn)]
[Model]
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

    [JsonProperty]
    [StringLength(25)]
    public override string Uid { get; set; } = Base62Converter.NewGuid();

    [JsonProperty]
    [StringLength(100)]
    public string Name { get; set; } = "";

    [JsonProperty]
    [StringLength(100)]
    public string Address1 { get; set; } = "";

    [JsonProperty]
    [StringLength(100)]
    public string Address2 { get; set; } = "";

    [JsonProperty]
    [StringLength(50)]
    public string City { get; set; } = "";

    [JsonProperty]
    [StringLength(2)]
    public string State { get; set; } = "";

    [JsonProperty]
    [StringLength(15)]
    public string Zip { get; set; } = "";

    [JsonProperty]
    [StringLength(25)]
    public string MainPhone { get; set; } = "";

    [JsonProperty]
    [StringLength(25)]
    public string Fax { get; set; } = "";

    [StringLength(255)]
    public string Website { get; set; } = "";

    [JsonProperty]
    public int EmployeeCount { get; set; } = 0;


}