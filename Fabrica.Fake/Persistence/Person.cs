using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fabrica.Fake.Persistence;


[Index(nameof(Uid))]
[Index(nameof(LastName),nameof(FirstName))]
[JsonObject(MemberSerialization.OptIn)]
[Model]
public class Person : BaseMutableModel<Person>, IRootModel, IExplorableModel
{

    public enum GenderKind { Female, Male }

    [NotMapped]
    public long IdSetter
    {
        get { return _id;}
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
    [StringLength(50)]
    public string FirstName { get; set; } = "";

    [JsonProperty]
    [StringLength(50)]
    public string MiddleName { get; set; } = "";

    [JsonProperty]
    [StringLength(50)]
    public string LastName { get; set; } = "";

    [JsonConverter(typeof(StringEnumConverter))]
    [JsonProperty]
    [StringLength(20)]
    public GenderKind Gender { get; set; } = GenderKind.Female;

    [JsonProperty]
    public DateTime BirthDate { get; set; } = DateTime.Now.AddYears(-25).Date;

    [JsonProperty]
    [StringLength(25)]
    public string PhoneNumber { get; set; } = "";

    [JsonProperty]
    [StringLength(100)]
    public string Email { get; set; } = "";

    [JsonProperty]
    public decimal Salary { get; set; }

}