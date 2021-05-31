
namespace Fabrica.Models.Patch.Builder
{

    public class DeltaProperty
    {

        public bool IsReference { get; set; }

        public bool IsCollection { get; set; }

        public string Parent { get; set; } = "";
        public string ParentUid  { get; set; } = "";

        public string Name { get; set; } = "";

        public object Original { get; set; }
        public object Current  { get; set; }

    }


}
