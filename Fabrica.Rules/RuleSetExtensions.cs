using Fabrica.Exceptions;
using Fabrica.Watch;

// ReSharper disable UnusedMember.Global

namespace Fabrica.Rules;


public static class RuleSetExtensions
{

    private static readonly List<EventDetail> EmptyDetails = [];


    public static EvaluationResults Evaluate( this IRuleSet rules, params object[] facts )
    {

        using var logger = rules.GetLogger();

        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(facts);

        var er = rules.Evaluate(ec);

        logger.LogObject(nameof(er), er);

        return er;

    }


    public static EvaluationResults Evaluate( this IRuleSet rules, IEnumerable<object> fact )
    {

        using var logger = rules.GetLogger();

        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(fact);

        var er = rules.Evaluate(ec);

        logger.LogObject(nameof(er), er);

        return er;

    }


    public static bool TryValidate(this IRuleSet rules, object subject, out List<EventDetail> violations)
    {

        using var logger = rules.GetLogger();


        violations = EmptyDetails;


        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddFacts(subject);

        var vr = rules.Evaluate(ec);

        logger.LogObject(nameof(vr), vr);


        if( !vr.HasViolations )
            return true;

        violations = [..vr.Events];

        return false;

    }


    public static bool TryValidate(this IRuleSet rules, IEnumerable<object> subjects, out List<EventDetail> violations)
    {

        using var logger = rules.GetLogger();


        violations = EmptyDetails;


        var ec = rules.GetEvaluationContext();
        ec.ThrowNoRulesException = false;
        ec.ThrowValidationException = false;

        ec.AddAllFacts(subjects);

        var vr = rules.Evaluate(ec);

        logger.LogObject(nameof(vr), vr);


        if (!vr.HasViolations)
            return true;

        violations = [.. vr.Events];

        return false;

    }


}