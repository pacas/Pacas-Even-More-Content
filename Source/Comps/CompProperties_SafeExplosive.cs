using RimWorld;

namespace PEMC
{
    public class CompProperties_SafeExplosive : CompProperties_Explosive
    {
        public new bool explodeOnKilled = false;
        public new bool explodeOnDestroyed = false;
        
        public CompProperties_SafeExplosive() => compClass = typeof (CompSafeExplosive);
    }
}