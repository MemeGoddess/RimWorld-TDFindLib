using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;

namespace TD_Find_Lib
{
  public static class FloatSubMenu
  {
	  private static Type FloatSubMenuType;
	  public static FloatMenuOption NewMenu(string label, List<FloatMenuOption> options)
	  {
		  FloatSubMenuType ??= AccessTools.TypeByName("FloatSubMenus.FloatSubMenu");

		  var constructors = FloatSubMenuType.GetConstructors();
		  var constructor =
			  constructors.FirstOrDefault(x => x.GetParameters()[2].ParameterType == typeof(MenuOptionPriority));
		  if (constructor == null)
		  {
			  var errorText = "Unable to find valid constructor for Float Sub-Menu mod, even though it's installed.";
			  Verse.Log.ErrorOnce(errorText, errorText.GetHashCode());
			  return new FloatMenuOption("Failed to init Float Sub-Menu", () => { });
		  }

		  object[] suppliedArgs = [label, options];
		  var parameters = constructor.GetParameters();
		  var args = new object[parameters.Length];

		  for (var i = 0; i < parameters.Length; i++)
		  {
			  if (i < suppliedArgs.Length)
				  args[i] = suppliedArgs[i];
			  else if (parameters[i].HasDefaultValue)
				  args[i] = parameters[i].DefaultValue;
			  else
				  throw new ArgumentException($"Missing parameter {parameters[i].Name}");
		  }

		  var value = constructor.Invoke(args);
		  return (FloatMenuOption)value;
	  }
  }
}
