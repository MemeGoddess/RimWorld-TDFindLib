using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TD_Find_Lib;
using TD_Find_Lib.Compat;
using UnityEngine;
using Verse;

// ReSharper disable once CheckNamespace
namespace TDFindLib_Ideology;

//helper class for category
[IgnoreMissing]
public class ThingQueryStyleCategory : ThingQueryCategorizedDropdownHelper<ThingStyleDef, StyleCategoryDef,
    ThingQueryThingStyle, ThingQueryStyleCategory>
{
    public override bool AppliesDirectlyTo(Thing thing)
    {
        // Not ?.StyleDef because null .Category is an option
        return thing.StyleDef is ThingStyleDef styleDef && styleDef.Category == sel;
    }


    public override string NameFor(StyleCategoryDef cat)
    {
        return cat.LabelCap;
    }

    public override string NullOption()
    {
        return "TD.OtherCategory".Translate();
    }

    public override Texture2D IconTexFor(StyleCategoryDef cat)
    {
        return cat?.Icon;
    }
}

[IgnoreMissing]
public class ThingQueryThingStyle : ThingQueryCategorizedDropdown<ThingStyleDef, StyleCategoryDef, ThingQueryThingStyle,
    ThingQueryStyleCategory>
{
    public static readonly Dictionary<ThingStyleDef, string> styleNames = new();
    public static readonly Dictionary<ThingStyleDef, ThingDef> baseDefForStyle = new();

    static ThingQueryThingStyle()
    {
        foreach (var styleDef in DefDatabase<StyleCategoryDef>.AllDefsListForReading)
        foreach (var style in styleDef.thingDefStyles)
        {
            baseDefForStyle[style.StyleDef] = style.ThingDef;

            styleNames[style.StyleDef] = styleDef.LabelCap + " " + style.ThingDef.LabelCap;
        }


        foreach (var thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
            if (thingDef.randomStyle != null)
                foreach (var styleChance in thingDef.randomStyle)
                {
                    baseDefForStyle[styleChance.StyleDef] = thingDef;

                    // Probably redundant since this seems to be overriden below:
                    styleNames[styleChance.StyleDef] = thingDef.LabelCap;
                }

        // overrides
        foreach (var styleDef in DefDatabase<ThingStyleDef>.AllDefsListForReading)
            if (styleDef.overrideLabel != null)
                styleNames[styleDef] = styleDef.overrideLabel.CapitalizeFirst();
    }

    public ThingQueryThingStyle()
    {
        extraOption = 1;
    }

    public override int ExtraOptionsCount => 1;

    public override bool AppliesDirectly2(Thing thing)
    {
        if (extraOption == 1)
            return thing.StyleDef != null;

        // redundant with check below
        //if (sel == null)
        //	return thing.StyleDef == null;

        return thing.StyleDef == sel;
    }

    public override string NameFor(ThingStyleDef def)
    {
        return styleNames[def];
    }


    public override string NullOption()
    {
        return "None".Translate();
    }

    public override string NameForExtra(int ex)
    {
        return "TD.AnyOption".Translate();
    }

    public override StyleCategoryDef CategoryFor(ThingStyleDef def)
    {
        return def.Category;
    }

    public override Texture2D IconTexFor(ThingStyleDef def)
    {
        return def.UIIcon;
    }

    public override Color IconColorFor(ThingStyleDef def)
    {
        if (def.color != default)
            return def.color; // though def.color is never set without mods..

        var baseDef = baseDefForStyle[def];
        if (baseDef.MadeFromStuff)
            return baseDef.GetColorForStuff(GenStuff.DefaultStuffFor(baseDef));
        return baseDef.uiIconColor;
    }

    public override IEnumerable<ThingStyleDef> AvailableOptions()
    {
        return ContentsUtility.AvailableInGame(t => t.StyleDef);
    }
}

[IgnoreMissing]
public class ThingQueryIdeoligion : ThingQueryDropDown<Ideo>
{
    public ThingQueryIdeoligion()
    {
        extraOption = 1;
    }

    public override int ExtraOptionsCount => 2;

    protected override Ideo ResolveRef(Map map)
    {
        return Find.IdeoManager.ideos.FirstOrDefault(i => NameFor(i) == selName);
    }

    public override bool AppliesDirectlyTo(Thing thing)
    {
        var pawn = thing as Pawn;
        if (pawn == null) return false;

        var ideo = pawn.Ideo;
        if (ideo == null) return false;

        if (extraOption == 1)
            return ideo == Faction.OfPlayer.ideos.PrimaryIdeo;

        if (extraOption == 2)
            return ideo != Faction.OfPlayer.ideos.PrimaryIdeo;

        return ideo == sel;
    }

    public override IEnumerable<Ideo> AllOptions()
    {
        return Current.Game?.World?.ideoManager?.IdeosInViewOrder;
    }

    public override Color IconColorFor(Ideo ideo)
    {
        return ideo.Color;
    }

    public override Texture2D IconTexFor(Ideo ideo)
    {
        return ideo.Icon;
    }

    public override string NameForExtra(int ex)
    {
        return ex == 1 ? "TD.PlayersIdeoligion".Translate() : "TD.OtherIdeoligion".Translate();
    }
}

//Non-structure memes.
[IgnoreMissing]
public class ThingQueryMeme : ThingQueryDropDown<MemeDef>
{
    public ThingQueryMeme()
    {
        sel = MemeDefOf.Darkness;
    }

    public override bool AppliesDirectlyTo(Thing thing)
    {
        var pawn = thing as Pawn;
        if (pawn == null) return false;

        var ideo = pawn.Ideo;
        if (ideo == null) return false;


        return pawn.Ideo.memes.Contains(sel);
    }

    public override Texture2D IconTexFor(MemeDef d)
    {
        return d.Icon;
    }

    public override IEnumerable<MemeDef> AllOptions()
    {
        // Only Structore or Normal, ehhhh
        return base.AllOptions().Where(m => m.category != MemeCategory.Structure);
    }
}

[IgnoreMissing]
public class ThingQueryPrecept : ThingQueryCategorizedDropdown<PreceptDef, IssueDef>
{
    public static readonly HashSet<PreceptDef> singlePreceptIssues =
        DefDatabase<PreceptDef>.AllDefs
            .Where(p => !DefDatabase<PreceptDef>.AllDefs.Any(other => p != other && other.issue == p.issue))
            .ToHashSet();

    public ThingQueryPrecept()
    {
        sel = PreceptDefOf.MeatEating_Abhorrent;
    }

    public override bool OrderedCat => true;

    public override bool Ordered => true;

    public override string NameForCat(IssueDef cat)
    {
        return cat?.LabelCap ?? "TD.OtherCategory".Translate();
    }

    public override IssueDef CategoryFor(PreceptDef def)
    {
        return singlePreceptIssues.Contains(def) ? null : def.issue;
    }

    public override Texture2D IconTexForCat(IssueDef cat)
    {
        return cat?.Icon ?? null;
    }

    public override Color IconColorFor(PreceptDef p)
    {
        return IdeoUIUtility.GetIconAndLabelColor(p.impact);
    }
    //public override IComparable OrderByCat(IssueDef cat) => cat.order;
    //The order of precepts is inconsistent, and the precepts are ordered, not the Issues. Ugh.

    public override bool AppliesDirectlyTo(Thing thing)
    {
        var pawn = thing as Pawn;
        if (pawn == null) return false;

        var ideo = pawn.Ideo;
        if (ideo == null) return false;


        return pawn.Ideo.HasPrecept(sel);
    }

    public override Texture2D IconTexFor(PreceptDef d)
    {
        return d.Icon;
    }

    public override string NameFor(PreceptDef d)
    {
        return d.tipLabelOverride ?? d.issue.LabelCap + ": " + d.LabelCap;
        //Precept.TipLabel
    }

    public override IEnumerable<PreceptDef> AllOptions()
    {
        return base.AllOptions().Where(p => p.visible && p.preceptClass == typeof(Precept));
    }
}

// Yap I sure planned to use this abstract class more
[IgnoreMissing]
public abstract class ThingQueryPreceptOther<T> : ThingQueryDropDown<PreceptDef> where T : Precept
{
    public override bool AppliesDirectlyTo(Thing thing)
    {
        var pawn = thing as Pawn;
        if (pawn == null) return false;

        var ideo = pawn.Ideo;
        if (ideo == null) return false;

        return AppliesToIdeo(pawn, ideo);
    }

    public abstract bool AppliesToIdeo(Pawn pawn, Ideo ideo);

    public override Texture2D IconTexFor(PreceptDef d)
    {
        return d.Icon;
    }

    public override string NameFor(PreceptDef d)
    {
        return d.tipLabelOverride ?? d.LabelCap;
    }

    public override IEnumerable<PreceptDef> AllOptions()
    {
        return base.AllOptions().Where(pdef => pdef.visible && typeof(T).IsAssignableFrom(pdef.preceptClass));
    }
}

[IgnoreMissing]
public class ThingQueryRole : ThingQueryPreceptOther<Precept_Role>
{
    public bool canBe; // default checks "is role"

    public ThingQueryRole()
    {
        sel = PreceptDefOf.IdeoRole_Leader;
    }

    public override int ExtraOptionsCount => 4;

    public bool RoleFitsSel(Precept_Role role)
    {
        return extraOption switch
        {
            3 => role is Precept_RoleMulti,
            2 => role is Precept_RoleSingle,
            1 => role != null, //aka role is Precept_Role
            _ => role?.def == sel
        };
    }

    public override bool AppliesToIdeo(Pawn pawn, Ideo ideo)
    {
        var role = ideo.GetRole(pawn);

        if (extraOption == 4)
        {
            if (role == null || role.apparelRequirements.NullOrEmpty())
                return false;

            return role.apparelRequirements
                .Select(r => r.requirement)
                .Any(r => r.IsActive(pawn) && !r.IsMet(pawn));
            // For multi-roles missing apparel, The -mood doesn't actually take effect
            // But they'll still choose to qear required apparel over others it seems
            // So whatever, even if there's no mood debuff, include them in this filter.
            // Otherwise if role == single return false
        }

        if (canBe)
            return ideo.RolesListForReading.Where(RoleFitsSel).Any(r => r.RequirementsMet(pawn));

        return RoleFitsSel(role);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref canBe, "canBe");
    }

    protected override ThingQuery Clone()
    {
        var clone = (ThingQueryRole)base.Clone();
        clone.canBe = canBe;
        return clone;
    }

    protected override bool DrawMain(Rect rect, bool locked, Rect fullRect)
    {
        var changed = base.DrawMain(rect, locked, fullRect);

        if (extraOption != 4)
            changed |= row.ButtonTextToggleBool(ref canBe, "TD.CanBe".Translate(), "TD.HasRole".Translate());

        return changed;
    }

    public override string NullOption()
    {
        return "None".Translate();
    }

    public override string NameForExtra(int ex)
    {
        return ex switch
        {
            4 => "TD.WantsApparelForRole".Translate(),
            3 => "TD.AnyNonLeaderRole".Translate(),
            2 => "TD.AnyLeaderRole".Translate(),
            _ => "TD.AnyOption".Translate()
        };
    }
}

/*
 * Aeh rituals got awfully complex with ... obligations and whatnot.
 * And it's awkard to have to target buildings that have rituals attached anyway
// Not PreceptOther, it doesn't target pawns ideo and it has invisible precepts
public enum RitualFilterType { Has, Available, InProgress}
public class ThingQueryRitual : ThingQueryDropDown<PreceptDef>
{
    public RitualFilterType filterType;

    public ThingQueryRitual() => sel = PreceptDefOf.RoleChange;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref filterType, "filterType");
    }

    protected override ThingQuery Clone()
    {
        ThingQueryRitual clone = (ThingQueryRitual)base.Clone();
        clone.filterType = filterType;
        return clone;
    }

    public static IEnumerable<Precept_Ritual> RitualsAt(Building building)
    {

        foreach (Ideo ideo in Faction.OfPlayer.ideos.AllIdeos)
        {
            foreach (Precept precept in ideo.PreceptsListForReading)
            {
                if (precept is not Precept_Ritual ritual) continue;

                if (!ritual.activeObligations.NullOrEmpty())
                    foreach (RitualObligation obl in ritual.activeObligations)
                        if (ritual.CanUseTarget(building, obl).canUse)
                            yield return ritual;
            }
        }
    }

    public override bool AppliesDirectlyTo(Thing thing)
    {
        Building building = thing as Building;
        if (building == null) return false;

        if(extraOption == 1)
        {
            return filterType switch
            {
                RitualFilterType.Has => RitualsAt(building).Any(),
                RitualFilterType.Available => RitualsAt(building).Any(r => r.CanUseTarget(building, ,
                RitualFilterType.InProgress=> building.TargetOfRitual() is LordJob_Ritual,
                _ => false // I guess
            }
            return ;
        }

        return building.TargetOfRitual() is LordJob_Ritual lordjob && lordjob.Ritual.def == sel;
    }

    public override Texture2D IconTexFor(PreceptDef d) => d.Icon;
    public override string NameFor(PreceptDef d) => d.tipLabelOverride ?? d.LabelCap;

    public override IEnumerable<PreceptDef> AllOptions() =>
        base.AllOptions().Where(pdef => typeof(Precept_Ritual).IsAssignableFrom(pdef.preceptClass));

    public override int ExtraOptionsCount => 1;
    public override string NameForExtra(int ex) => "TD.AnyOption".Translate();
}
*/

[DefOf]
[IgnoreMissing]
public class ThingQueryWeaponClass : ThingQueryDropDown<WeaponClassDef>
{
    public static WeaponClassDef Ultratech;
    //public bool despised; // false means check noble

    public ThingQueryWeaponClass()
    {
        sel = Ultratech;
    }

    public override bool AppliesDirectlyTo(Thing thing)
    {
        var weapon =
            thing is Pawn pawn
                ? pawn.equipment?.Primary
                : thing as ThingWithComps; //if it's not a weapon it won't pass check anyway

        if (weapon == null)
            return false;

        return weapon.def.weaponClasses?.Contains(sel) ?? false;
    }
}

// Check if item is noble to player ideo ; or held pawn's ideo. ; or pawn's held weapon
// Precept_Weapon
[IgnoreMissing]
public class ThingQueryWeaponDisposition : ThingQueryDropDown<IdeoWeaponDisposition>
{
    public ThingQueryWeaponDisposition()
    {
        sel = IdeoWeaponDisposition.Noble;
    }

    public override bool AppliesDirectlyTo(Thing thing)
    {
        // The weapon itself, or the equipped weapon of a pawn.
        var weapon =
            thing is Pawn pawn
                ? pawn.equipment?.Primary
                : thing as ThingWithComps; //if it's not a weapon it won't pass check anyway

        if (weapon == null || !weapon.def.IsWeapon)
            return false;

        // The ideo of the person holding this item, or... I guess just the Player's ideo. Items don't have factions.
        var ideo =
            weapon.ParentHolder is Pawn_EquipmentTracker tracker
                ? tracker.pawn.Ideo
                : Faction.OfPlayer.ideos.PrimaryIdeo;

        if (ideo == null)
            return false;

        return ideo.GetDispositionForWeapon(weapon.def) == sel;
    }

    public override string NameFor(IdeoWeaponDisposition o)
    {
        return base.NameFor(o).CapitalizeFirst();
    }
}

// Precept_Animal
[IgnoreMissing]
public class ThingQueryVeneratedAnimal : ThingQueryIdeoligion // conveniently same options but different filter
{
    public override bool AppliesDirectlyTo(Thing thing)
    {
        var pawn = thing as Pawn;
        if (pawn == null) return false;

        // Which ideo this animal is venerated for
        var ideo = sel;

        if (extraOption == 1)
            ideo = pawn.Faction?.ideos?.PrimaryIdeo; // An animal's owner's faction.

        if (extraOption == 2)
            ideo = Faction.OfPlayer.ideos.PrimaryIdeo;

        if (ideo == null) return false;

        return ideo.VeneratedAnimals.Contains(thing.def);
    }

    public override string NameForExtra(int ex)
    {
        return ex == 1 ? "TD.OwnersIdeoligion".Translate() : "TD.PlayersIdeoligion".Translate();
    }
}

// Precept_Xenotype
public enum IdeoXenotypeFilterType
{
    Preferred,
    Disliked,
    NoOpinion
}

[IgnoreMissing]
public class ThingQueryIdeoXenotype : ThingQueryIdeoligion // conveniently same options but different filter
{
    public IdeoXenotypeFilterType filterType;

    public override bool AppliesDirectlyTo(Thing thing)
    {
        var pawn = thing as Pawn;
        if (pawn == null) return false;

        if (pawn.genes == null) return false;

        // Which ideo this checks for
        var ideo = sel;

        if (extraOption == 1)
            ideo = pawn.Ideo;

        if (extraOption == 2)
            ideo = Faction.OfPlayer.ideos.PrimaryIdeo;

        if (ideo == null) return false;

        // The logic here is messing with me hnnng
        if (ideo.PreferredXenotypes.Count == 0
            && ideo.PreferredCustomXenotypes.Count == 0)
            return filterType == IdeoXenotypeFilterType.NoOpinion;

        if (filterType == IdeoXenotypeFilterType.NoOpinion)
            return false;

        return filterType == IdeoXenotypeFilterType.Preferred == ideo.IsPreferredXenotype(pawn);
    }

    public override string NameForExtra(int ex)
    {
        return ex == 1 ? "TD.PersonsIdeoligion".Translate() : "TD.PlayersIdeoligion".Translate();
    }

    protected override bool DrawMain(Rect rect, bool locked, Rect fullRect)
    {
        var changed = base.DrawMain(rect, locked, fullRect);

        RowButtonFloatMenuEnum(filterType, newValue => filterType = newValue);

        return changed;
    }
}

public enum GauranlenFilterType
{
    Connection,
    Mode,
    Strength,
    StrengthDesired,
    MaxDryads,
    CurDryads,
    TimeToDryad
}

[IgnoreMissing]
public class ThingQueryGauranlen : ThingQueryDropDown<GauranlenFilterType>
{
    public GauranlenTreeModeDef modeDef;
    public IntRangeUB numDryads;
    public FloatRangeUB strength;
    public IntRangeUB timeToDryad; // ticks, shown as days

    public ThingQueryGauranlen()
    {
        modeDef = GauranlenTreeModeDefOf.Gaumaker;
        strength = new FloatRangeUB(0, 1, .5f, 1); // todo find consts
        numDryads = new IntRangeUB(0, 4, 1, 2);
        timeToDryad = new IntRangeUB(0, 8 * GenDate.TicksPerDay, 0, GenDate.TicksPerDay);
    }

    public override void ExposeData()
    {
        base.ExposeData();

        switch (sel)
        {
            //				case GauranlenFilterType.Connection:
            //					break;
            case GauranlenFilterType.Mode:
                Scribe_Defs.Look(ref modeDef, "modeDef");
                break;
            case GauranlenFilterType.Strength:
            case GauranlenFilterType.StrengthDesired:
                Scribe_Values.Look(ref strength.range, "strength");
                break;
            case GauranlenFilterType.MaxDryads:
            case GauranlenFilterType.CurDryads:
                Scribe_Values.Look(ref numDryads.range, "numDryads");
                break;
            case GauranlenFilterType.TimeToDryad:
                Scribe_Values.Look(ref timeToDryad.range, "timeToDryad");
                break;
        }
    }

    protected override ThingQuery Clone()
    {
        var clone = (ThingQueryGauranlen)base.Clone();
        clone.modeDef = modeDef;
        clone.strength = strength;
        clone.numDryads = numDryads;
        clone.timeToDryad = timeToDryad;
        return clone;
    }


    public override bool AppliesDirectlyTo(Thing thing)
    {
        if (thing is not Plant tree ||
            tree.GetComp<CompTreeConnection>() is not CompTreeConnection connection)
            return false;

        switch (sel)
        {
            case GauranlenFilterType.Connection:
                return connection.Connected;
            case GauranlenFilterType.Mode:
                return connection.Mode == modeDef;
            case GauranlenFilterType.Strength:
                return strength.Includes(connection.ConnectionStrength);
            case GauranlenFilterType.StrengthDesired:
                return strength.Includes(connection.DesiredConnectionStrength);
            case GauranlenFilterType.MaxDryads:
                return numDryads.Includes(connection.MaxDryads);
            case GauranlenFilterType.CurDryads:
                return numDryads.Includes(connection.dryads.Count);
            case GauranlenFilterType.TimeToDryad:
                return timeToDryad.Includes(connection.spawnTick - Find.TickManager.TicksGame);
        }

        return false;
    }

    public override bool DrawCustom(Rect fullRect)
    {
        switch (sel)
        {
            case GauranlenFilterType.Connection:
                return false;

            case GauranlenFilterType.Mode:
                RowButtonFloatMenu(
                    modeDef,
                    DefDatabase<GauranlenTreeModeDef>.AllDefs.Append(null),
                    def => def == null ? "None".Translate() : def.GetLabel(),
                    newValue => modeDef = newValue);
                return false;

            case GauranlenFilterType.Strength:
            case GauranlenFilterType.StrengthDesired:
                return TDWidgets.FloatRangeUB(fullRect.RightHalfClamped(row.FinalX), id, ref strength,
                    ToStringStyle.PercentZero);

            case GauranlenFilterType.MaxDryads:
            case GauranlenFilterType.CurDryads:
                return TDWidgets.IntRangeUB(fullRect.RightHalfClamped(row.FinalX), id, ref numDryads);

            case GauranlenFilterType.TimeToDryad:
                return TDWidgets.IntRangeUB(fullRect.RightHalfClamped(row.FinalX), id, ref timeToDryad,
                    ticks => $"{ticks * 1f / GenDate.TicksPerDay:0.0}");
        }

        return false;
    }
}

[StaticConstructorOnStartup]
public static class ExpansionHider
{
    static ExpansionHider()
    {
        ThingQueryMaker.EnsureStaticInit();

        if (!ModsConfig.IdeologyActive)
            foreach (var def in DefDatabase<ThingQuerySelectableDef>.AllDefsListForReading)
                if (def.mod == ModContentPack.IdeologyModPackageId)
                    def.devOnly = true;

        if (!ModsConfig.BiotechActive)
            DefDatabase<ThingQuerySelectableDef>.GetNamed("Query_IdeoXenotype").devOnly = true;
    }
}