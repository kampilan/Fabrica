namespace Fabrica.Models.Patch.Builder;

public class PropertyPath
{


    public string Model { get; set; } = "";
    public string Uid { get; set; } = "";
    public string Property { get; set; } = "";


    #region Identity members

    private Type GetUnproxiedType()
    {
        return GetType();
    }


    public virtual bool Equals(PropertyPath? other)
    {

        if (other == null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (Equals(Uid, other.Uid))
        {

            var typeOther = other.GetUnproxiedType();
            var typeThis  = GetUnproxiedType();

            return (typeThis.IsAssignableFrom(typeOther)) || (typeOther.IsAssignableFrom(typeThis));

        }

        return false;

    }


    public override bool Equals(object? other)
    {
        if (other is PropertyPath a)
            return Equals(a);

        return false;

    }


    public override int GetHashCode()
    {
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Uid.GetHashCode();
    }

    #endregion        


}