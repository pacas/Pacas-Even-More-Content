using System.Collections.Generic;
using Verse;

namespace PEMC
{
    public class CompProperties_SafeProximityFuse : CompProperties
    {
        public ThingDef target;
        
        public float radius;

        public CompProperties_SafeProximityFuse() => compClass = typeof (CompProximitySafeFuse);

        public override IEnumerable<string> ConfigErrors(ThingDef parentDef)
        {
            foreach (string configError in base.ConfigErrors(parentDef))
                yield return configError;
            if (parentDef.tickerType != TickerType.Normal)
                yield return "CompProximityFuse needs tickerType " + TickerType.Rare + " or faster, has " + parentDef.tickerType;
            if (parentDef.CompDefFor<CompSafeExplosive>() == null)
                yield return "CompProximityFuse requires a CompSafeExplosive";
        }
    }
}