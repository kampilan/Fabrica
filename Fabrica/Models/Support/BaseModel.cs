using Fabrica.Utilities.Text;

namespace Fabrica.Models.Support;

public abstract class BaseModel<TImp> : IModel where TImp : BaseModel<TImp>
{

    //public abstract long Id { get; protected set; }

    private string _uid = Base62Converter.NewGuid();
    public virtual string Uid
    {
        get => _uid;
        set => _uid = value;
    }


    #region Identity members


    private Type GetUnproxiedType()
    {
        return GetType();
    }


    public virtual bool Equals(BaseModel<TImp>? other)
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



    public override bool Equals( object? other )
    {
        if (other is BaseModel<TImp> a)
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

    #endregion


}