using TD_Find_Lib;
using TD_Find_Lib.Compat;
using Verse;

// ReSharper disable once CheckNamespace - Namespace needs to stay the same
namespace TDFindLib_RealFOW;

[IgnoreMissing]
public class ThingQueryRealFOW : ThingQuery
{
    public override bool AppliesDirectlyTo(Thing thing) => 
        true;
}