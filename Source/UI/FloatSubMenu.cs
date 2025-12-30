using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace TD_Find_Lib
{
  public static class FloatSubMenu
  {
	  private static Type FloatSubMenuType;
	  private static Type FloatSubMenuAndRefresh;
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

		  if (options == null || !options.Any())
		  {
			  return new FloatMenuOption(label + " (empty)", null);
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

	  public static FloatMenuOption NewRefreshMenu(string label, List<FloatMenuOption> options, ThingQuery query,
		  Color? color = null)
	  {
		  if (options == null || !options.Any())
		  {
			  return new FloatMenuOption(label + " (empty)", null);
		  }

			FloatSubMenuAndRefresh ??= AccessTools.TypeByName("TDFloatSubMenu.FloatSubMenuAndRefresh");
		  return FindConstructorAndCreate<FloatMenuOption>(FloatSubMenuAndRefresh, label, options, query, color);
	  }

	  public static FloatMenuOption NewRefreshMenu(string label, List<FloatMenuOption> options, ThingQuery query,
		  Texture2D itemIcon, Color? color = null)
	  {
		  if (options == null || !options.Any())
		  {
			  return new FloatMenuOption(label + " (empty)", null);
		  }

			FloatSubMenuAndRefresh ??= AccessTools.TypeByName("TDFloatSubMenu.FloatSubMenuAndRefresh");
		  return FindConstructorAndCreate<FloatMenuOption>(FloatSubMenuAndRefresh, label, options, query, itemIcon, color);
	  }

	  public static FloatMenuOption NewRefreshMenu(string label, List<FloatMenuOption> options, ThingQuery query,
		  ThingDef defForItemIcon, Color? color = null)
	  {
		  if (options == null || !options.Any())
		  {
			  return new FloatMenuOption(label + " (empty)", null);
		  }

			FloatSubMenuAndRefresh ??= AccessTools.TypeByName("TDFloatSubMenu.FloatSubMenuAndRefresh");
		  return FindConstructorAndCreate<FloatMenuOption>(FloatSubMenuAndRefresh, label, options, query, defForItemIcon, color);
	  }

		public static void AddSearchIfInstalled(this List<FloatMenuOption> options)
	  {
		  if (Settings.FloatSubMenuInstalled)
			  options.Add((FloatMenuOption)Activator.CreateInstance(AccessTools.TypeByName("FloatSubMenus.FloatMenuSearch"),
				  args: true));
	  }

	  private static T FindConstructorAndCreate<T>(Type type, params object[] parameters)
	  {
		  var constructors = type.GetConstructors();
		  var selected = constructors.FirstOrDefault(x =>
		  {
			  var param = x.GetParameters();

			  for (var index = 0; index < param.Length; index++)
			  {
				  var parameterInfo = param[index];
				  if (index < parameters.Length)
				  {
					  if (parameterInfo.ParameterType != parameters[index].GetType() && !parameterInfo.ParameterType.IsInstanceOfType(parameters[index]))
						  return false;
						continue;
				  }

					if(!parameterInfo.HasDefaultValue)
						return false;
			  }
			  return true;
		  });

		  if (selected == null)
			  throw new MissingMethodException(type.Name, "Constructor");

		  var args = selected.GetParameters().Select((x, index) =>
		  {
			  if (index < parameters.Length)
				  return parameters[index];
			  if (x.HasDefaultValue)
				  return x.DefaultValue;
			  throw new ArgumentException($"Missing parameter {x.GetType().Name}");
		  }).ToArray();

		  return (T)selected.Invoke(args);
	  }
  }
}
