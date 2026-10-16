using RimWorld;
using TD_Find_Lib;
using TD_Find_Lib.Compat;
using Verse;

// ReSharper disable once CheckNamespace
namespace TDFindLib_Odyssey;

[IgnoreMissing]
public class ThingQueryIsUnique : ThingQuery
{
    public override bool AppliesDirectlyTo(Thing thing)
    {
        return thing.TryGetComp<CompUniqueWeapon>() != null;
    }
}

[IgnoreMissing]
public class ThingQueryPercentFuel : ThingQueryFloatRange
{
    public override bool AppliesDirectlyTo(Thing thing)
    {
        if (thing.TryGetComp<CompPilotConsole>() is CompPilotConsole console)
            return sel.Includes(console.engine.TotalFuel / console.engine.MaxFuel);

        return false;
    }
}

[IgnoreMissing]
public class ThingQueryPercentCapacity : ThingQueryFloatRange
{
    public override bool AppliesDirectlyTo(Thing thing)
    {
        return thing.TryGetComp<CompPilotConsole>() is CompPilotConsole console &&
               sel.Includes(console.engine.AllConnectedSubstructure.Count /
                            console.engine.GetStatValue(StatDefOf.SubstructureSupport));
    }
}

[IgnoreMissing]
public class ThingQueryIsOnCooldown : ThingQuery
{
    public override bool AppliesDirectlyTo(Thing thing)
    {
        return thing.TryGetComp<CompPilotConsole>() is CompPilotConsole console &&
               console.engine.cooldownCompleteTick > Find.TickManager.TicksGame;
    }
}

[StaticConstructorOnStartup]
public static class ExpansionHider
{
    static ExpansionHider()
    {
        ThingQueryMaker.EnsureStaticInit();

        if (!ModsConfig.OdysseyActive)
            foreach (var def in DefDatabase<ThingQuerySelectableDef>.AllDefsListForReading)
                if (def.mod == ModContentPack.OdysseyModPackageId)
                    def.devOnly = true;
    }
}