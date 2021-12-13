using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fabrica.Models.Support;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Fabrica.Fake.Persistence;

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

    [StringLength(25)]
    public override string Uid { get; set; } = "";

    [StringLength(50)]
    public string FirstName { get; set; } = "";
    [StringLength(50)]
    public string MiddleName { get; set; } = "";
    [StringLength(50)]
    public string LastName { get; set; } = "";

    [JsonConverter(typeof(StringEnumConverter))]
    [StringLength(20)]
    public GenderKind Gender { get; set; } = GenderKind.Female;

    public DateTime BirthDate { get; set; } = DateTime.Now.AddYears(-25).Date;

    [StringLength(25)]
    public string PhoneNumber { get; set; } = "";
    [StringLength(100)]
    public string Email { get; set; } = "";

    public decimal Salary { get; set; } = 0;

}