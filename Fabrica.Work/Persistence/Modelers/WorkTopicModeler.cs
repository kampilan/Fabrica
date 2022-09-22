using Fabrica.Persistence.Ef.Contexts;
using Fabrica.Utilities.Text;
using Fabrica.Work.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabrica.Work.Persistence.Modelers;

public class WorkTopicModeler: IModeler<WorkTopic>
{

    public void Configure(EntityTypeBuilder<WorkTopic> builder)
    {
        
        builder.ToTable(nameof(WorkTopic).Pluralize());

    }

}