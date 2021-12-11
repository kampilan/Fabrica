using System.Collections.Generic;
using Fabrica.Mediator;
using Fabrica.Models;
using Fabrica.Models.Support;
using MediatR;

namespace Fabrica.Persistence.Mediator;

public class AuditJournalQueryRequest<TEntity> : IRequest<Response<List<AuditJournalModel>>> where TEntity: class, IMutableModel
{

    public string Uid { get; set; } = "";


}