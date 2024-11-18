using System.Collections.Generic;
using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace PEMC
{
    public class CompSafeExplosive : CompExplosive
    {
        private Thing instigator = null;
        private OverlayHandle? overlayBurningWick;
        
        public new CompProperties_SafeExplosive Props => (CompProperties_SafeExplosive) props;
        
        private void UpdateOverlays()
        {
            if (!parent.Spawned || !Props.drawWick)
                return;
            parent.Map.overlayDrawer.Disable(parent, ref overlayBurningWick);
            if (!wickStarted)
                return;
            overlayBurningWick =
                parent.Map.overlayDrawer.Enable(parent, OverlayTypes.BurningWick);
        }

        private bool CanExplodeFromDamageType(DamageDef damage)
        {
            return Props.requiredDamageTypeToExplode == null || Props.requiredDamageTypeToExplode == damage;
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
                radius += parent.GetComp<CompRefuelable>().Fuel;
                parent.GetComp<CompRefuelable>().ConsumeFuel(parent.GetComp<CompRefuelable>().Fuel);
            }

            EndWickSustainer();
            wickStarted = false;
            if (map == null) {
                Log.Warning("Tried to detonate CompExplosive in a null map.");
            } else {
                if (props.explosionEffect != null) {
                    Effecter effecter = props.explosionEffect.Spawn();
                    effecter.Trigger(new TargetInfo(this.parent.PositionHeld, map),
                        new TargetInfo(this.parent.PositionHeld, map));
                    effecter.Cleanup();
                }

                IntVec3 positionHeld = parent.PositionHeld;
                Map map1 = map;
                DamageDef explosiveDamageType = props.explosiveDamageType;
                Thing instigator = thing;
                int damageAmountBase = props.damageAmountBase;
                double armorPenetrationBase = props.armorPenetrationBase;
                SoundDef explosionSound = props.explosionSound;
                ThingDef explosionSpawnThingDef1 = props.postExplosionSpawnThingDef;
                double explosionSpawnChance1 = props.postExplosionSpawnChance;
                int explosionSpawnThingCount1 = props.postExplosionSpawnThingCount;
                GasType? explosionGasType = Props.postExplosionGasType;
                int num2 = props.applyDamageToExplosionCellsNeighbors ? 1 : 0;
                ThingDef explosionSpawnThingDef2 = props.preExplosionSpawnThingDef;
                double explosionSpawnChance2 = props.preExplosionSpawnChance;
                int explosionSpawnThingCount2 = props.preExplosionSpawnThingCount;
                double chanceToStartFire = props.chanceToStartFire;
                int num3 = props.damageFalloff ? 1 : 0;
                float? direction = new float?();
                List<Thing> ignoredByExplosion = new List<Thing>{parent};
                FloatRange? affectedAngle = new FloatRange?();
                int num4 = props.doVisualEffects ? 1 : 0;
                bool doSoundEffects = props.doSoundEffects;
                double propagationSpeed = props.propagationSpeed;
                int num5 = doSoundEffects ? 1 : 0;
                GenExplosion.DoExplosion(positionHeld, map1, radius, explosiveDamageType, instigator,
                    damageAmountBase, (float)armorPenetrationBase, explosionSound,
                    postExplosionSpawnThingDef: explosionSpawnThingDef1,
                    postExplosionSpawnChance: (float)explosionSpawnChance1,
                    postExplosionSpawnThingCount: explosionSpawnThingCount1, postExplosionGasType: explosionGasType,
                    applyDamageToExplosionCellsNeighbors: num2 != 0, preExplosionSpawnThingDef: explosionSpawnThingDef2,
                    preExplosionSpawnChance: (float)explosionSpawnChance2,
                    preExplosionSpawnThingCount: explosionSpawnThingCount2, chanceToStartFire: (float)chanceToStartFire,
                    damageFalloff: num3 != 0, excludeRadius: 1, direction: direction, ignoredThings: ignoredByExplosion,
                    affectedAngle: affectedAngle, doVisualEffects: num4 != 0, propagationSpeed: (float)propagationSpeed,
                    doSoundEffects: num5 != 0);
            }

            UpdateOverlays();
        }
        
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if ((mode != DestroyMode.KillFinalize || !Props.explodeOnKilled) && !Props.explodeOnDestroyed)
                return;
            Detonate(previousMap, true);
        }
    }
}