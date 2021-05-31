using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Fabrica.Rules.Builder;
using Fabrica.Rules.Factory;

namespace Fabrica.Rules
{

    public static class AutofacExtensions
    {

        public static ContainerBuilder UseRules(this ContainerBuilder builder)
        {


            // *********************************************************
            builder.Register(c =>
                {

                    var sources = c.Resolve<IEnumerable<IRuleBuilderSource>>();

                    var comp = new RuleSetFactory();
                    comp.AddAllSources(sources);

                    return comp;

                })
                .AsSelf()
                .As<IStartable>()
                .SingleInstance()
                .AutoActivate();



            // *********************************************************
            builder.Register(c => c.Resolve<RuleSetFactory>().GetRuleSet())
                .As<IRuleSet>();



            // *********************************************************
            return builder;

        }



        public static ContainerBuilder AddRules(this ContainerBuilder builder, IRuleBuilderSource source)
        {


            // *********************************************************
            builder.RegisterInstance(source)
                .As<IRuleBuilderSource>()
                .SingleInstance()
                .AutoActivate();



            // *********************************************************
            return builder;


        }


        public static ContainerBuilder AddRules(this ContainerBuilder builder, params Assembly[] assemblies)
        {


            // *********************************************************
            builder.Register(c =>
                {

                    var comp = new RuleBuilderSource();
                    comp.AddTypes(assemblies);

                    return comp;

                })
                .As<IRuleBuilderSource>()
                .SingleInstance()
                .AutoActivate();



            // *********************************************************
            return builder;


        }


        public static ContainerBuilder AddRules(this ContainerBuilder builder, params Type[] types)
        {


            // *********************************************************
            builder.Register(c =>
            {

                var comp = new RuleBuilderSource();
                comp.AddTypes(types);

                return comp;

            })
                .As<IRuleBuilderSource>()
                .SingleInstance()
                .AutoActivate();


            // *********************************************************
            return builder;


        }


        public static ContainerBuilder AddRules(this ContainerBuilder builder, IEnumerable<Type> candidates)
        {


            // *********************************************************
            builder.Register(c =>
            {

                var comp = new RuleBuilderSource();
                comp.AddTypes(candidates);

                return comp;

            })
                .As<IRuleBuilderSource>()
                .SingleInstance()
                .AutoActivate();


            // *********************************************************
            return builder;


        }


    }


}
