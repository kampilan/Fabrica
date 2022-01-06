using System.Collections.Generic;
using Fabrica.Exceptions;

namespace Fabrica.One
{


    public class Result
    {
        public bool Successful { get; set; }

        public List<EventDetail> Details { get; } = new List<EventDetail>();

    }


}