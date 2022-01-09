using System.Collections.Generic;
using Fabrica.Models.Support;
using Fabrica.Utilities.Text;

namespace Fabrica.One.Models
{

    public class RepositoryModel: BaseReferenceModel
    {

        public override long Id { get; protected set; }

        public override string Uid { get; set; } = Base62Converter.NewGuid();

        public string Description { get; set; } = "";

        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();


        public override string ToString()
        {
            return $"{Description} [{string.Join(",", Properties.Values)}]";
        }

    }

}
