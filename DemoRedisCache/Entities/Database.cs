using System.Collections.Generic;

namespace DemoRedisCache.Entities
{
	public static class Database
	{
		private static readonly List<Person> all = new List<Person>();

		static Database()
		{
			all.Add(new Person { Id = 1, Score = 2983, FirstName = "Gilbert", LastName = "Lewis" });
			all.Add(new Person { Id = 2345, Score = 1022, FirstName = "Alberto", LastName = "Roy" });
			all.Add(new Person { Id = 32837, Score = 1084, FirstName = "Oscar", LastName = "Reeves" });
			all.Add(new Person { Id = 984857, Score = 1923, FirstName = "Jana", LastName = "Blair" });
			all.Add(new Person { Id = 1074745, Score = 2912, FirstName = "Eleanor", LastName = "Lopez" });
			all.Add(new Person { Id = 687589, Score = 2347, FirstName = "Leo", LastName = "Stonawski" });
			all.Add(new Person { Id = 1057057, Score = 1390, FirstName = "Miroslav", LastName = "Holec" });
		}

		public static IEnumerable<Person> All => all;
	}
}