using Fabrica.Models.Support;

namespace Fabrica.Persistence.Ef.Entities;

public abstract class BaseReferenceEntity : IReferenceModel
{


    public abstract string Uid { get; set; }


    private Type GetUnproxiedType()
    {
        return GetType();
    }


    // ReSharper disable once UnusedMember.Global
    public virtual bool Equals(BaseReferenceEntity? other)
    {

        if (other == null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Equals(Uid, other.Uid))
        {

            var typeOther = other.GetUnproxiedType();
            var typeThis = GetUnproxiedType();

            return (typeThis.IsAssignableFrom(typeOther)) || (typeOther.IsAssignableFrom(typeThis));

        }

        return false;

    }



    public override bool Equals(object? other)
    {
        if (other is BaseReferenceModel a)
            return Equals(a);

        return false;

    }

    public override int GetHashCode()
    {
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Uid.GetHashCode();
    }


    public override string ToString()
    {
        var s = $"{GetType().FullName} - Id: {Uid}";
        return s;
    }


}