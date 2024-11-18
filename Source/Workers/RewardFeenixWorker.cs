using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace PEMC.Workers
{
    public class RewardFeenixWorker : RitualAttachableOutcomeEffectWorker
    {
        public override void Apply(Dictionary<Pawn, int> totalPresence, LordJob_Ritual jobRitual, RitualOutcomePossibility outcome, out string extraOutcomeDesc, ref LookTargets letterLookTargets)
        {
            if (jobRitual == null)
            {
                throw new ArgumentNullException("jobRitual");
            }
            
            if (outcome == null)
            {
                throw new ArgumentNullException("outcome");
            }
            
            IncidentParms incidentParams = new IncidentParms
            {
                target = jobRitual.Map
            };
            
            IncidentDef incidentDef = IncidentDef.Named("FeenixPasses");
            if (!incidentDef.Worker.CanFireNow(incidentParams))
            {
                extraOutcomeDesc = "FeenixSummonFail".Translate();
                return;
            }
            
            incidentDef.Worker.TryExecute(incidentParams);
            extraOutcomeDesc = this.def.letterInfoText;
        }
    }
}