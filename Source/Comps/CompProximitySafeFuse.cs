using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace PEMC
{
    public class CompProximitySafeFuse : ThingComp
    {
        public CompProperties_SafeProximityFuse Props => (CompProperties_SafeProximityFuse) props;

        public override void CompTick()
        {
            if (Find.TickManager.TicksGame % 250 != 0)
                return;
            this.CompTickRare();
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!DebugSettings.ShowDevGizmos) {
                yield break;
            }
            
            yield return new Command_Action {
                defaultLabel = "DEV: Trigger",
                action = delegate {
                    parent.GetComp<CompSafeExplosive>().StartWick();
                }
            };
        }

        public override void CompTickRare()
        {
            if (!parent.Spawned || GenClosest.ClosestThingReachable(
                    parent.Position, parent.Map, ThingRequest.ForDef(Props.target), PathEndMode.OnCell, 
                    TraverseParms.For(TraverseMode.NoPassClosedDoors), Props.radius) == null
                )
                return;
            
            parent.GetComp<CompSafeExplosive>().StartWick();
        }
    }
}