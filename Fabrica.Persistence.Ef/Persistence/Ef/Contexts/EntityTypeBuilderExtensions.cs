// ReSharper disable UnusedMember.Global

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Fabrica.Models.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabrica.Persistence.Ef.Contexts;

#nullable enable

public static class EntityTypeBuilderExtensions
{

    public static EntityTypeBuilder<TModel> HasRequiredReference<TModel, TReference>(this EntityTypeBuilder<TModel> builder, Expression<Func<TModel, TReference?>> navigation) where TModel : class, IModel where TReference : class, IReferenceModel
    {
        builder.HasOne(navigation).WithMany().OnDelete(DeleteBehavior.Restrict).IsRequired();
        return builder;
    }

    public static EntityTypeBuilder<TModel> HasOptionalReference<TModel, TReference>(this EntityTypeBuilder<TModel> builder, Expression<Func<TModel, TReference?>> navigation) where TModel : class, IModel where TReference : class, IReferenceModel
    {
        builder.HasOne(navigation).WithMany().OnDelete(DeleteBehavior.ClientSetNull).IsRequired(false);
        return builder;
    }

    public static EntityTypeBuilder<TModel> HasAggregate<TModel, TAggregate>(this EntityTypeBuilder<TModel> builder, Expression<Func<TModel, IEnumerable<TAggregate>?>>? navigation, Expression<Func<TAggregate, TModel?>>? parent) where TModel : class, IModel where TAggregate : class, IAggregateModel
    {
        builder.HasMany(navigation).WithOne(parent).OnDelete(DeleteBehavior.ClientCascade);
        return builder;
    }

    public static EntityTypeBuilder<TAggregate> HasParent<TAggregate, TModel>(this EntityTypeBuilder<TAggregate> builder, Expression<Func<TAggregate, TModel?>>? parent) where TModel : class, IModel where TAggregate : class, IAggregateModel
    {
        builder.HasOne(parent);
        return builder;
    }

}

