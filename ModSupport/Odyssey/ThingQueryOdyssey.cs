using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using TD_Find_Lib;
using Verse;

namespace TDFindLib_Odyssey
{
    public class ThingQueryIsUnique : ThingQuery
    {
	    public override bool AppliesDirectlyTo(Thing thing) => 
		    thing.TryGetComp<CompUniqueWeapon>() != null;
    }

    public class ThingQueryPercentFuel : ThingQueryFloatRange
    {
	    public override bool AppliesDirectlyTo(Thing thing)
	    {
		    if (thing.TryGetComp<CompPilotConsole>() is CompPilotConsole console)
			    return sel.Includes(console.engine.TotalFuel / console.engine.MaxFuel);

		    return false;
	    }
    }

	public class ThingQueryPercentCapacity : ThingQueryFloatRange
	{
		public override bool AppliesDirectlyTo(Thing thing)
		{
			return thing.TryGetComp<CompPilotConsole>() is CompPilotConsole console &&
			       sel.Includes(console.engine.AllConnectedSubstructure.Count /
			                    console.engine.GetStatValue(StatDefOf.SubstructureSupport));

		}
	}

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
				foreach (ThingQuerySelectableDef def in DefDatabase<ThingQuerySelectableDef>.AllDefsListForReading)
					if (def.mod == ModContentPack.OdysseyModPackageId)
						def.devOnly = true;
		}
	}
}
