using Fabrica.Exceptions;

namespace Fabrica.Rules.Validators
{
    public interface ICollectionValidator<out TFact, out TType>
    {

        ICollectionValidator<TFact, TType> Is(Func<TFact, IEnumerable<TType>, bool> condition);

        ICollectionValidator<TFact, TType> IsNot(Func<TFact, IEnumerable<TType>, bool> condition);

        IValidationRule<TFact> Otherwise(string template, params Func<TFact, object>[] parameters);

        IValidationRule<TFact> Otherwise(string group, string template, params Func<TFact, object>[] parameters);

        IValidationRule<TFact> Otherwise( EventDetail.EventCategory category, string group, string template, params Func<TFact, object>[] parameters);

    }



    public class CollectionValidator<TFact, TType> : BaseValidator<TFact>, ICollectionValidator<TFact, TType>
    {
        public CollectionValidator(ValidationRule<TFact> rule, string group, Func<TFact, IEnumerable<TType>> extractor) : base(rule, group)
        {
            Extractor = extractor;
        }


        protected Func<TFact, IEnumerable<TType>> Extractor { get; }

        
        public ICollectionValidator<TFact, TType> Is(Func<TFact, IEnumerable<TType>, bool> condition)
        {
            bool Cond(TFact f) => condition(f, Extractor(f) );
            Conditions.Add(Cond);
            return this;
        }

        
        public ICollectionValidator<TFact, TType> IsNot(Func<TFact, IEnumerable<TType>, bool> condition)
        {
            bool Cond(TFact f) => !(condition(f, Extractor(f)));
            Conditions.Add(Cond);
            return this;
        }

    }


    public static class CollectionValidatorEx
    {

        public static ICollectionValidator<TFact, TType> Required<TFact, TType>( this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
        {
            return validator.Is((f, v) => v.Any());
        }

        public static ICollectionValidator<TFact, TType> IsEmpty<TFact, TType>( this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
        {
            return validator.IsNot((f, v) => v.Any());
        }

        public static ICollectionValidator<TFact, TType> IsNotEmpty<TFact, TType>( this ICollectionValidator<TFact, TType> validator) where TFact : class where TType : class
        {
            return validator.Is((f, v) => v.Any());
        }


        
        public static ICollectionValidator<TFact, TType> Has<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class
            where TType : class
        {
            validator.Is((f, v) => v.Any(predicate));
            return validator;
        }


        
        public static ICollectionValidator<TFact, TType> HasNone<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
        {
            validator.IsNot((f, v) => v.Any(predicate));
            return validator;
        }


        
        public static ICollectionValidator<TFact, TType> HasExactly<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
        {
            validator.Is((f, v) => v.Where(predicate).Count() == count);
            return validator;
        }


        
        public static ICollectionValidator<TFact, TType> HasOnlyOne<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
        {
            validator.Is((f, v) => v.Where(predicate).Count() == 1);
            return validator;
        }


        
        public static ICollectionValidator<TFact, TType> HasAtMostOne<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate) where TFact : class where TType : class
        {
            validator.Is((f, v) => v.Where(predicate).Count() <= 1);
            return validator;
        }


        
        public static ICollectionValidator<TFact, TType> HasAtLeast<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
        {
            validator.Is((f, v) => v.Where(predicate).Count() <= count);
            return validator;
        }


        
        public static ICollectionValidator<TFact, TType> HasAtMost<TFact, TType>(this ICollectionValidator<TFact, TType> validator, Func<TType, bool> predicate, int count) where TFact : class where TType : class
        {
            validator.Is((f, v) => v.Where(predicate).Count() <= count);
            return validator;
        }

    }

}
