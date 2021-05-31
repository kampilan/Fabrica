using System;
using JetBrains.Annotations;

namespace Fabrica.Models.Support
{


    public abstract class BaseReferenceModel: IReferenceModel
    {


        public abstract long Id { get; protected set; }
        public abstract string Uid { get; set; }


        private Type GetUnproxiedType()
        {
            return GetType();
        }


        public virtual bool Equals([CanBeNull] BaseReferenceModel other)
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



        public override bool Equals(object other)
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


        [NotNull]
        public override string ToString()
        {
            var s = $"{GetType().FullName} - Id: {Uid}";
            return s;
        }


    }


}
