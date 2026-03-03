using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TD_Find_Lib
{
	public class QuerySearchDef
	{
		public string name;
		public SearchListType listType;
		public SearchMapType mapType;
		public List<Map> searchMaps;
		public List<ThingQueryPreselectDef> queries;
		public bool matchAllQueries;

		public QuerySearch CreateSearch()
		{
			var search = new QuerySearch(null)
			{
				name = name,
				parameters = new SearchParameters()
				{
					listType = listType,
					mapType = mapType,
					searchMaps = searchMaps
				},
				MatchAllQueries = matchAllQueries,
				Children =
				{
					queries = queries.Select(ThingQueryMaker.MakeQuery).ToList()
				}
			};

			return search;
		}
	}
}
