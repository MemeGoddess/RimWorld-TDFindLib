using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Verse;

namespace TD_Find_Lib
{
	public class QuerySearchGroupDef : Def
	{
		public string name;
		public XmlNode searches;

		public SearchGroup CreateGroup()
		{
			var group = new SearchGroup(name, null);
			//searches.ForEach(x => group.Add(x.CreateSearch()));
			var scribed = ScribeXmlFromString.LoadListFromString<SearchGroup>(searches.OuterXml);
			return group;
		}
	}
}
