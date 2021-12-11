using Fabrica.Models.Support;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fabrica.Persistence.Ef.Contexts
{


    public interface IModeler<TEntity> where TEntity : class, IModel
    {

        void Configure( EntityTypeBuilder<TEntity> builder );

    }


}
