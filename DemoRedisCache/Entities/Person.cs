using System;

namespace DemoRedisCache.Entities
{
	public class Person
	{
		public Person()
		{
		}

		public Person(string firstName, string lastName)
		{
			FirstName = firstName;
			LastName = lastName;
			Score = new Random(Guid.NewGuid().GetHashCode()).Next(1111,9999);
		}

		public Person(string firstName, string lastName, int score)
		{
			FirstName = firstName;
			LastName = lastName;
			Score = score;
		}

		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public int Score { get; set; }
	}
}