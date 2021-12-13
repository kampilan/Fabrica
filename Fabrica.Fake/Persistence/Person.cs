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

    private long _id;
    public override long Id
    {
        get => _id;
        protected set { }
    }

    public override string Uid { get; set; } = "";

    public string FirstName { get; set; } = "";
    public string MiddleName { get; set; } = "";
    public string LastName { get; set; } = "";

    [JsonConverter(typeof(StringEnumConverter))]
    public GenderKind Gender { get; set; } = GenderKind.Female;

    public DateTime BirthDate { get; set; } = DateTime.Now.AddYears(-25).Date;

    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";

    public decimal Salary { get; set; } = 0;

}