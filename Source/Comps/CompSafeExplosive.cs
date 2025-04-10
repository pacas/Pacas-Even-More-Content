using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace PEMC
{
    public class CompSafeExplosive : CompExplosive
    {
        public Thing instigator;
        public int countdownTicksLeft = -1;
        public OverlayHandle? overlayBurningWick;
        public Sustainer wickSoundSustainer;
        public bool wickStarted;
        public new CompProperties_SafeExplosive Props => (CompProperties_SafeExplosive) props;
        
        public override void CompTick()
        {
            if (countdownTicksLeft > 0)
            {
                --countdownTicksLeft;
                if (countdownTicksLeft == 0)
                {
                    StartWick();
                    countdownTicksLeft = -1;
                }
            }
            if (!wickStarted)
                return;
            if (wickSoundSustainer == null)
                StartWickSustainer();
            else
                wickSoundSustainer.Maintain();
            if (Props.wickMessages != null)
            {
                foreach (WickMessage wickMessage in Props.wickMessages)
                {
                    if (wickMessage.ticksLeft == wickTicksLeft && wickMessage.wickMessagekey != null)
                        Messages.Message(wickMessage.wickMessagekey.Translate((NamedArgument) parent.GetCustomLabelNoCount(false), (NamedArgument) wickTicksLeft.ToStringSecondsFromTicks()), (Thing) this.parent, wickMessage.messageType ?? MessageTypeDefOf.NeutralEvent, false);
                }
            }
            --wickTicksLeft;
            if (wickTicksLeft > 0)
                return;
            Detonate(parent.MapHeld);
        }
        
        private void StartWickSustainer()
        {
            SoundDefOf.MetalHitImportant.PlayOneShot(new TargetInfo(parent.PositionHeld, parent.MapHeld));
            SoundInfo info = SoundInfo.InMap((TargetInfo) (Thing) parent, MaintenanceType.PerTick);
            wickSoundSustainer = SoundDefOf.HissSmall.TrySpawnSustainer(info);
        }
        
        private void UpdateOverlays()
        {
            if (!parent.Spawned || !Props.drawWick)
                return;
            parent.Map.overlayDrawer.Disable(parent, ref overlayBurningWick);
            if (!wickStarted)
                return;
            overlayBurningWick = parent.Map.overlayDrawer.Enable(parent, OverlayTypes.BurningWick);
        }

        private bool CanExplodeFromDamageType(DamageDef damage)
        {
            return Props.requiredDamageTypeToExplode == null || Props.requiredDamageTypeToExplode == damage;
        }
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            if (Props.countdownTicks.HasValue)
                countdownTicksLeft = Props.countdownTicks.Value.RandomInRange;
            UpdateOverlays();
        }
        
        public override void PostDeSpawn(Map map)
        {
            base.PostDeSpawn(map);
            StopWick();
        }
        
        public new void StopWick()
        {
            wickStarted = false;
            instigator = null;
            parent.Map.overlayDrawer.Disable(parent, ref overlayBurningWick);
            EndWickSustainer();
        }

        private void EndWickSustainer()
        {
            if (wickSoundSustainer == null)
                return;
            wickSoundSustainer.End();
            wickSoundSustainer = (Sustainer)null;
        }
        
        public new float ExplosiveRadius()
        {
            CompProperties_SafeExplosive props = Props;
            float num = (float) (customExplosiveRadius ?? (double) Props.explosiveRadius);
            if (parent.stackCount > 1 && props.explosiveExpandPerStackcount > 0.0)
                num += Mathf.Sqrt((parent.stackCount - 1) * props.explosiveExpandPerStackcount);
            if (props.explosiveExpandPerFuel > 0.0 && parent.GetComp<CompRefuelable>() != null)
                num += Mathf.Sqrt(parent.GetComp<CompRefuelable>().Fuel * props.explosiveExpandPerFuel);
            return num;
        }

        protected new void Detonate(Map map, bool ignoreUnspawned = false)
        {
            if (!ignoreUnspawned && !parent.SpawnedOrAnyParentSpawned)
                return;
            CompProperties_SafeExplosive props = Props;
            float radius = ExplosiveRadius();
            if (radius <= 0.0)
                return;
            Thing thing =
                this.instigator == null || this.instigator.HostileTo(parent.Faction) &&
                parent.Faction != Faction.OfPlayer ? parent : this.instigator;
            if (props.explosiveExpandPerFuel > 0.0 && parent.GetComp<CompRefuelable>() != null) {
                radius += parent.GetComp<CompRefuelable>().Fuel * 1.3f;
                parent.GetComp<CompRefuelable>().ConsumeFuel(parent.GetComp<CompRefuelable>().Fuel);
            }

            EndWickSustainer();
            wickStarted = false;
            parent.Map.overlayDrawer.Disable(parent, ref overlayBurningWick);
            if (map == null) {
                Log.Warning("Tried to detonate CompExplosive in a null map.");
            } else {
                if (props.explosionEffect != null) {
                    Effecter effecter = props.explosionEffect.Spawn();
                    effecter.Trigger(new TargetInfo(this.parent.PositionHeld, map),
                        new TargetInfo(this.parent.PositionHeld, map));
                    effecter.Cleanup();
                }

                List<Thing> ignoredByExplosion = new List<Thing>{parent};
                GenExplosion.DoExplosion(
                    parent.PositionHeld, 
                    map, 
                    radius, 
                    props.explosiveDamageType, 
                    thing,
                    props.damageAmountBase, 
                    props.armorPenetrationBase, 
                    props.explosionSound,
                    postExplosionSpawnThingDef: props.postExplosionSpawnThingDef,
                    postExplosionSpawnChance: props.postExplosionSpawnChance,
                    postExplosionSpawnThingCount: props.postExplosionSpawnThingCount, 
                    postExplosionGasType: props.postExplosionGasType,
                    applyDamageToExplosionCellsNeighbors: props.applyDamageToExplosionCellsNeighbors, 
                    preExplosionSpawnThingDef: props.preExplosionSpawnThingDef,
                    preExplosionSpawnChance: props.preExplosionSpawnChance,
                    preExplosionSpawnThingCount: props.preExplosionSpawnThingCount, 
                    chanceToStartFire: props.chanceToStartFire,
                    damageFalloff: props.damageFalloff, 
                    excludeRadius: 1, 
                    direction: null, 
                    ignoredThings: ignoredByExplosion,
                    affectedAngle: null, 
                    doVisualEffects: props.doVisualEffects, 
                    propagationSpeed: props.propagationSpeed,
                    doSoundEffects: props.doSoundEffects
                );
            }
        }
        
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if ((mode != DestroyMode.KillFinalize || !Props.explodeOnKilled) && !Props.explodeOnDestroyed)
                return;
            Detonate(previousMap, true);
        }
    }
}