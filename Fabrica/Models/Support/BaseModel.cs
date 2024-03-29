﻿using Fabrica.Utilities.Text;

namespace Fabrica.Models.Support;

public abstract class BaseModel<TImp> : IModel where TImp : BaseModel<TImp>
{

    public abstract string Uid
    {
        get;
        set;
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