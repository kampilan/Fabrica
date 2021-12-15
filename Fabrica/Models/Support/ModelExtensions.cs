using System;
using System.Reflection;
using Fabrica.Utilities.Text;
using JetBrains.Annotations;

namespace Fabrica.Models.Support;

public static class ModelExtensions
{


    public static string ToAlias([NotNull] this Type type)
    {

        var attr = (ModelAttribute)type.GetCustomAttribute(typeof(ModelAttribute));
        if (attr != null && !string.IsNullOrWhiteSpace(attr.Alias))
            return attr.Alias;

        return type.Name;

    }

    public static string ToAlias([NotNull] this IModel model)
    {


        var attr = (ModelAttribute)model.GetType().GetCustomAttribute(typeof(ModelAttribute));
        if (attr != null && !string.IsNullOrWhiteSpace(attr.Alias))
            return attr.Alias;

        return model.GetType().Name;

    }


    public static string ToResource( [NotNull] this Type type )
    {

        static string FromType( ModelAttribute ea, Type src )
        {

            string resource;
            if( !string.IsNullOrWhiteSpace(ea.Resource))
                resource = ea.Resource.Pluralize().ToLowerInvariant();
            else if(!string.IsNullOrWhiteSpace(ea.Alias))
                resource = ea.Alias.Pluralize().ToLowerInvariant();
            else
                resource = src.Name.Pluralize().ToLowerInvariant();

            return resource;

        }

        var attr = (ModelAttribute)type.GetCustomAttribute(typeof(ModelAttribute));

        if( attr != null && !string.IsNullOrWhiteSpace(attr.Resource) )
            return attr.Resource.Pluralize().ToLowerInvariant();

        if( attr != null && attr.RefersTo != null )    
            return FromType(attr, attr.RefersTo );

        if( attr != null )
            return FromType( attr, type );

        return type.Name.Pluralize().ToLowerInvariant();

    }


}