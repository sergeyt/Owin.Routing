#if NUNIT
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

namespace Owin.Routing.Tests
{
	[TestFixture]
	public class DynamicMethodsTests
	{
		[Test]
		public void CompileGetter()
		{
			var item = new Item {Name = "a"};
			var type = item.GetType();
			var property = type.GetProperty("Name");
			var getter = DynamicMethods.CompileGetter(type, property);
			var value = getter(item);
			Assert.AreEqual(item.Name, value);
		}

		[Test]
		public void CompileSetter()
		{
			var item = new Item { Name = "a" };
			var type = item.GetType();
			var property = type.GetProperty("Name");
			var setter = DynamicMethods.CompileSetter(type, property);
			setter(item, "b");
			Assert.AreEqual("b", item.Name);
		}

		[Test]
		public void CompileIndexer()
		{
			var items = new ItemCollection {"a"};
			var func = DynamicMethods.CompileIndexer(items.GetType());
			var item = (Item)func(items, 0);
			Assert.IsNotNull(item);
			Assert.AreSame(items[0], item);
		}

		[Test]
		public void CompileRemoveAt()
		{
			var items = new ItemCollection { "a" };
			var func = DynamicMethods.CompileRemoveAt(items.GetType());
			var result = (Item)func(items, 0);
			Assert.IsNull(result);
			Assert.AreEqual(0, items.Count);
		}

		[Test]
		public void CompileAdd()
		{
			var items = new ItemCollection();
			var func = DynamicMethods.CompileAdd(items.GetType());
			var item = (Item) func(items, new object[] {"a"});
			Assert.AreEqual(1, items.Count);
			Assert.IsNotNull(item);
			Assert.AreEqual("a", item.Name);
		}

		class Item
		{
			public string Name { get; set; }
		}

		class ItemCollection : IEnumerable<Item>
		{
			private readonly List<Item> _list = new List<Item>();

			public int Count
			{
				get { return _list.Count; }
			}

			public Item Add(string name)
			{
				var item = new Item {Name = name};
				_list.Add(item);
				return item;
			}

			public Item this[int index]
			{
				get { return _list[index]; }
			}

			public void RemoveAt(int index)
			{
				_list.RemoveAt(index);
			}

			public IEnumerator<Item> GetEnumerator()
			{
				return _list.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
#endif
