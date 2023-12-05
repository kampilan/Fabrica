using System.ComponentModel;
using Fabrica.Models.Support;
using Fabrica.Persistence.Mongo;
using Fabrica.Utilities.Text;
using MongoDB.Bson;
using PropertyChanged.SourceGenerator;

namespace Fabrica.Test.Models.Patch;

[Collection("companies")]
[Model]
public partial class MongoCompany : BaseMutableModel<MongoCompany>, IRootModel, INotifyPropertyChanged
{


    public MongoCompany() : this(false)
    {

    }

    public MongoCompany(bool added)
    {

        SuspendTracking(m =>
        {

        });

        if (added)
            Added();

    }

    private ObjectId _id;

    public override string Uid { get; set; } = Base62Converter.NewGuid();


    [Notify]
    private string _name = "";
    [Notify]
    private string _address1 = "";
    [Notify]
    private string _address2 = "";
    [Notify]
    private string _city = "";
    [Notify]
    private string _state = "";
    [Notify]
    private string _zip = "";
    [Notify]
    private string _mainPhone = "";
    [Notify]
    private string _fax = "";
    [Notify]
    private string _website = "";
    [Notify]
    private int _employeeCount = 0;



}