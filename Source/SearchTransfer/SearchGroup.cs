using System;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using CloneArgs = TD_Find_Lib.QuerySearch.CloneArgs;
using System.Collections;

namespace TD_Find_Lib
{
	public interface ISearchStorageParent
	{
		public void NotifyChanged();
		public List<SearchGroup> Children { get; }
		public void Add(SearchGroup group, bool refresh = true);
		public void ReorderGroup(int from, int to);
	}

	// Trying to save a List<List<Deep>> doesn't work.
	// Need List to be "exposable" on its own.
	public class SearchGroup : SearchGroupBase<QuerySearch>
	{
		public string name;
		public ISearchStorageParent parent;

		public SearchGroup(string name, ISearchStorageParent parent)
		{
			this.name = name;
			this.parent = parent;
		}

		public SearchGroup()
		{
			
		}

		public SearchGroup Clone(CloneArgs cloneArgs, string newName = null, ISearchStorageParent newParent = null)
		{
			SearchGroup clone = new(newName ?? name, newParent);
			foreach (QuerySearch search in this)
			{
				//obviously don't set newName in cloneArgs
				clone.Add(search.Clone(cloneArgs));
			}
			return clone;
		}

		public override void Replace(QuerySearch newSearch, int i)
		{
			base.Replace(newSearch, i);
			parent.NotifyChanged();
		}

		public override void Copy(QuerySearch newSearch, int i)
		{
			base.Copy(newSearch, i);
			parent.NotifyChanged();
		}

		public override void DoAdd(QuerySearch newSearch)
		{
			base.DoAdd(newSearch);
			parent.NotifyChanged();
		}


		public override void ExposeData()
		{
			Scribe_Values.Look(ref name, "name", Settings.defaultGroupName);

			base.ExposeData();
		}
	}

	public abstract class SearchGroupBase<T> : IList<T>, IExposable where T : IQuerySearch
	{
		private List<T> searches = new List<T>();

		// Abstractions
		public IEnumerator<T> GetEnumerator() =>
			searches.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() =>
			GetEnumerator();

		public void Add(T item) =>
			searches.Add(item);

		public void Clear() =>
			searches.Clear();

		public bool Contains(T item) =>
			searches.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) =>
			searches.CopyTo(array, arrayIndex);

		public bool Remove(T item) =>
			searches.Remove(item);

		public int Count { get; }
		public bool IsReadOnly { get; }

		public int IndexOf(T item) =>
			searches.IndexOf(item);

		public void Insert(int index, T item) =>
			searches.Insert(index, item);

		public void RemoveAt(int index) =>
			searches.RemoveAt(index);

		public int RemoveAll(Predicate<T> remove) => 
			searches.RemoveAll(remove);

		public void AddRange(IList<T> newSearches) => 
			searches.AddRange(newSearches);

		public int FindLastIndex(Predicate<T> match) =>
			searches.FindLastIndex(match);

		public T this[int index]
		{
			get => searches[index];
			set => searches[index] = value;
		}
		public virtual void Replace(T newSearch, int i)
		{
			this[i] = newSearch;
		}
		public virtual void Copy(T newSearch, int i)
		{
			newSearch.Search.name += "TD.CopyNameSuffix".Translate();
			Insert(i + 1, newSearch);
		}
		public virtual void DoAdd(T newSearch)
		{
			Add(newSearch);
		}

		public void ConfirmPaste(T newSearch, int i)
		{
			// TODO the weird case where you changed the name in the editor, to a name that already exists.
			// Right now it'll have two with same name instead of overwriting that one.
			Verse.Find.WindowStack.Add(new Dialog_MessageBox(
				"TD.SaveChangesTo0".Translate(newSearch.Search.name),
				"Confirm".Translate(), () => Replace(newSearch, i),
				"No".Translate(), null,
				"TD.OverwriteSearch".Translate(),
				true, () => Replace(newSearch, i),
				delegate () { }// I dunno who wrote this class but this empty method is required so the window can close with esc because its logic is very different from its base class
			)
			{
				buttonCText = "TD.SaveAsCopy".Translate(),
				buttonCAction = () => Copy(newSearch, i),
			});
		}

		public void TryAdd(T search)
		{
			if (searches.FindIndex(d => d.Search.name == search.Search.name) is int index && index != -1)
				ConfirmPaste(search, index);
			else
				DoAdd(search);
		}

		public virtual void ExposeData()
		{
			string label = "searches";//notranslate

			//Watered down Scribe_Collections, doing LookMode.Deep on List<QuerySearch>
			if (Scribe.EnterNode(label))
			{
				try
				{
					if (Scribe.mode == LoadSaveMode.Saving)
					{
						foreach (T search in this)
						{
							T target = search;	//It's what vanilla code does /shrug
							Scribe_Deep.Look(ref target, "li");
						}
					}
					else if (Scribe.mode == LoadSaveMode.LoadingVars)
					{
						XmlNode curXmlParent = Scribe.loader.curXmlParent;
						Clear();

						foreach (XmlNode node in curXmlParent.ChildNodes)
							Add(ScribeExtractor.SaveableFromNode<T>(node, new object[] { }));

						// Somehow this happened perhaps with modded subclass of ThingQuery
						searches.RemoveAll(i => i == null);
					}
				}
				finally
				{
					Scribe.ExitNode();
				}
			}
		}
	}
}