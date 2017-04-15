using System;
using System.Configuration;
using System.Web.Mvc;
using StackExchange.Redis;

namespace DemoRedisCache.Controllers
{
    public class TestController : Controller
    {
		/// <summary>
		/// Do not connect to redis this way !!!
		/// URL: connections
		/// </summary>
        public ActionResult Connections()
        {
			var redisConnectionString = ConfigurationManager.AppSettings["RedisConnectionString"];
	        ConnectionMultiplexer connection = ConnectionMultiplexer.Connect(redisConnectionString);

	        var db = connection.GetDatabase();
	        var result = db.StringGet("test");

			return Content(result);
        }

		/// <summary>
		/// Recommended approach: lazy connection pattern
		/// </summary>
		public ActionResult ConnectionsLazy()
		{
			var db = Connection.GetDatabase();
			var result = db.StringGet("test");

			return Content(result);
		}

		private static readonly Lazy<ConnectionMultiplexer> LazyConnection = new Lazy<ConnectionMultiplexer>(() =>
		{
			var redisConnectionString = ConfigurationManager.AppSettings["RedisConnectionString"];
			return ConnectionMultiplexer.Connect(redisConnectionString);
		});

		public static ConnectionMultiplexer Connection => LazyConnection.Value;


		/// <summary>
		/// Databases
		/// URL: databases
		/// </summary>
		public ActionResult ConnectionsDb()
		{
			var db = Connection.GetDatabase();
			var result = db.StringSet("test", "test A");

			var db1 = Connection.GetDatabase(0);
			var result1 = db1.StringSet("test", "test B");

			var db2 = Connection.GetDatabase(1);
			var result2 = db2.StringSet("test", "test C");

			return Content(result + " : " + result1 + " : " + result2);
		}
	}
}