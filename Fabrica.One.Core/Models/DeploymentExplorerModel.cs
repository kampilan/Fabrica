using System;

namespace Fabrica.One.Models
{

    public class DeploymentExplorerModel
    {

        public string Mission { get; set; } = "";

        public string BuildName { get; set; } = "";

        public string BuildNum { get; set; } = "";

        public DateTime BuildDate { get; set; } = DateTime.MinValue;

        public bool IsAbsoluteBuild { get; set; }

    }

}
