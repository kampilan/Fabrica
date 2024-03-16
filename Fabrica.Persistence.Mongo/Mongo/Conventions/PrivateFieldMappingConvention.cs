using System.Reflection;
using Fabrica.Models.Support;
using Humanizer;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.IdGenerators;

namespace Fabrica.Persistence.Mongo.Conventions;


public class PrivateFieldMappingConvention() : ConventionBase("Fabrica.ClassMapping"), IClassMapConvention
{

    public void Apply( BsonClassMap classMap )
    {

        var type = classMap.ClassType;
        if( !type.IsAssignableTo(typeof(IModel)) || type.IsAbstract )
            return;

        classMap.SetIgnoreExtraElements(true);

        var flags  = BindingFlags.NonPublic | BindingFlags.Instance;
        var fields = type.GetFields(flags);


        foreach( var field in fields.Where(f=>!f.IsPublic && f.Name.StartsWith("_") && !f.FieldType.IsAssignableTo(typeof(IAggregateCollection)) && !f.FieldType.IsAssignableTo(typeof(IReferenceModel)) ) )
        {

            var propName = field.Name.Substring(1).Pascalize();

            if( field.Name == "_id" && field.FieldType == typeof(ObjectId) )
                classMap.MapIdMember(field).SetElementName("_id").SetIdGenerator( new ObjectIdGenerator());
            else
                classMap.MapMember(field).SetElementName(propName);

        }


    }
}