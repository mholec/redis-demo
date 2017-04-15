using System;
using System.Linq;
using System.Web.Mvc;
using DemoRedisCache.Entities;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace DemoRedisCache.Controllers
{
    public class DemoController : Controller
    {
	    private readonly ConnectionMultiplexer _connection;

		public DemoController(ConnectionMultiplexer connection)
		{
			_connection = connection;
		}

        public ActionResult Prehled()
        {
            return View();
        }

		/// <summary>
		/// Uložení libovolné hodnoty do cache
		/// </summary>
        public ActionResult CachovaniPrimitivnichTypu()
        {
	        IDatabase redis = _connection.GetDatabase();

	        RedisValue date = redis.StringGet("datum");

	        if (!date.IsNullOrEmpty)
	        {
		        DateTime dt = DateTime.Parse(date);
		        return Content(dt.ToString());
	        }
	        else
	        {
		        DateTime dt = DateTime.Now;
		        bool result = redis.StringSet("datum", dt.ToString(), TimeSpan.FromSeconds(10), When.Always, CommandFlags.FireAndForget);
				return Content(dt.ToString());
			}
		}

		/// <summary>
		/// Jednoduchý čítač návštěv pomocí Increment
		/// </summary>
		public ActionResult Inkrementace()
		{
			IDatabase redis = _connection.GetDatabase();

			string ip = Request.UserHostAddress;
			string key = "user:" + ip + ":visits";

			long counter = redis.StringIncrement(key, 1);

			return Content(counter.ToString());
		}

		/// <summary>
		/// Cachování objektu serializací do JSON
		/// </summary>
		public ActionResult CachovaniObjektu()
        {
			IDatabase redis = _connection.GetDatabase();

			RedisValue jsonString = redis.StringGet("objekt");
			if (!jsonString.IsNullOrEmpty)
			{
				Person person = JsonConvert.DeserializeObject<Person>(jsonString);
				return Json(person, JsonRequestBehavior.AllowGet);
			}
			else
			{
				Person person = new Person("Miroslav", "Holec");
				redis.StringSet("objekt", JsonConvert.SerializeObject(person), TimeSpan.FromSeconds(10));
				return Json(person, JsonRequestBehavior.AllowGet);
			}
		}

		/// <summary>
		/// Cachování vlastností objektu pomocí hash
		/// </summary>
		public ActionResult CachovaniVlastnostiObjektu()
		{
			IDatabase redis = _connection.GetDatabase();

			var mujhash = redis.HashGet("mujhash", new RedisValue[] {"FirstName", "LastName", "Score"});
			if (mujhash.Any(x => x.HasValue))
			{
				Person person = new Person(mujhash[0].ToString(), mujhash[1].ToString());
				if (mujhash[2].IsInteger)
				{
					person.Score = int.Parse(mujhash[2]);
				}

				return Json(person, JsonRequestBehavior.AllowGet);
			}
			else
			{
				Person person = new Person("Miroslav", "Holec");
				redis.HashSet("mujhash", new[]
				{
					new HashEntry("FirstName", person.FirstName),
					new HashEntry("LastName", person.LastName),
					new HashEntry("Score", person.Score),
				});

				return Json(person, JsonRequestBehavior.AllowGet);
			}
		}

		/// <summary>
		/// Indexování množiny dat
		/// </summary>
		public ActionResult Indexing(string firstName, string lastName)
		{
			string key = "homepage:filtered:" + firstName + "-" + lastName;

			IDatabase redis = _connection.GetDatabase();

			var set = redis.SetMembers(key);
			if (set.Length > 0)
			{
				var ids = set.Select(x => x.ToString());
				var result = Database.All.Where(x => ids.Contains(x.Id.ToString()));

				return View(result.Take(5).ToList());
			}
			else
			{
				var result = Database.All;
				if (!string.IsNullOrEmpty(firstName))
				{
					result = result.Where(x => x.FirstName.Contains(firstName));

				}
				if (!string.IsNullOrEmpty(lastName))
				{
					result = result.Where(x => x.LastName.Contains(lastName));
				}

				RedisValue[] redisValues = result.Take(5).Select(x => (RedisValue)x.Id.ToString()).ToArray();
				if (redisValues.Length > 0)
				{
					redis.SetAdd(key, redisValues);
				}

				return View(result.Take(5).ToList());
			}
        }

		/// <summary>
		/// Transakce
		/// </summary>
		public ActionResult Transakce()
		{
			IDatabase redis = _connection.GetDatabase();

			ITransaction transaction = redis.CreateTransaction();
			transaction.StringSetAsync("klic1", "hodnota1");
			transaction.AddCondition(Condition.KeyExists("klic1")); // proběhne transakce nebo ne?
			transaction.StringSetAsync("klic2", "hodnota2");
			bool result = transaction.Execute();

			return Content(result.ToString());
		}

		/// <summary>
		/// Nalezení TOP 3 prvků dle nějakých pravidel
		/// </summary>
		public ActionResult Leaderboard()
		{
			IDatabase redis = _connection.GetDatabase();

			// Příprava dat
			foreach (var person in Database.All.OrderByDescending(x=> x.Score).Take(3))
			{
				redis.SortedSetAdd("leaderboard", person.Id, person.Score);
			}

			SortedSetEntry[] sort = redis.SortedSetRangeByRankWithScores("leaderboard", 0, 9, Order.Descending);

			var data = (from p in Database.All
				from s in sort
				where p.Id == s.Element
				orderby s.Score descending 
				select new Person()
				{
					Id = p.Id,
					FirstName = p.FirstName,
					LastName = p.LastName,
					Score = (int) s.Score
				}).Take(3);

			return Json(data, JsonRequestBehavior.AllowGet);
		}

		/// <summary>
		/// OutputCache, viz. web.config
		/// </summary>
		[OutputCache(Duration = 10)]
        public ActionResult OutputCaching()
        {
            return View();
        }

		/// <summary>
		/// Uložení do Session
		/// </summary>
        public ActionResult SessionStateCaching()
        {
			Session.Add("testSessionKey", "Hello World!");

            return Content(Session["testSessionKey"].ToString());
        }

		public ActionResult Pipelining()
		{
			IDatabase redis = _connection.GetDatabase();

			var task1 = redis.StringGetAsync("customer:1");
			var task2 = redis.StringGetAsync("customer:2");

			// libovolný C# kód nezávislý na výsledku

			var customer1 = redis.Wait(task1);
			var customer2 = redis.Wait(task2);

			return Content(customer1 + " " + customer2);
		}
	}
}