using System;
using System.Threading.Tasks;

namespace Fabrica.One.Plan
{


    public abstract class AbstractPlanSource
    {


        public TimeSpan CheckInterval { get; set; } = TimeSpan.FromSeconds(30);
        protected DateTime LastCheck { get; set; } = DateTime.Now.AddHours(-24);

        protected abstract Task<bool> CheckForUpdate();

        public async Task<bool> HasUpdatedPlan()
        {

            if( DateTime.Now < LastCheck + CheckInterval )
                return false;

            var updated = await CheckForUpdate();

            LastCheck = DateTime.Now;

            return updated;

        }


    }


}
