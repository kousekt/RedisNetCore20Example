using Microsoft.AspNetCore.Mvc;
using RedisFreeOne.Models;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Binary;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace RedisFreeOne.Controllers
{
    [Serializable]
    public class Content
    {
        public int Id { get; set; }
        public string Text { get; set; }
    }
    /*
     * 
     * had to compile the two dll's.
     * .net standard 2.  Not 1.1
        http://localhost:55261/api/Home
        http://localhost:55261/api/Home/5
        http://localhost:55261/api/Home/Seed/10/0
        http://localhost:55261/api/Home/SeedBigData/88/51200000

        https://github.com/StackExchange/StackExchange.Redis
        https://github.com/imperugo/StackExchange.Redis.Extensions

        Had to bring in the StackExchange.Redis.Extensions project into studio, compile it and then add as reference
        so that we can enjoy complex objects
    */
    public class HomeController : Controller
    { 
        private ICacheClient getCacheClient()
        {
            var mux = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                DefaultVersion = new Version(3, 0, 500),
                EndPoints = { { "localhost", 6379 } },
                AllowAdmin = true
            });

            return new StackExchangeRedisCacheClient(mux, new BinarySerializer());
        }
        public IActionResult Index()
        {
            ICacheClient client = getCacheClient();

            Content content = new Content()
            {
                Id = 5,
                Text = "this is a bunch of text that we want to store"
            };

            bool added = client.Add("content_" + content.Id.ToString(), content, DateTimeOffset.Now.AddMinutes(1));

            return View();
        }
        
            
        [Route("api/Home/Flush")]
        public IActionResult Flush()
        {
            ICacheClient client = getCacheClient();
            client.FlushDb();
            return Ok();
        }

        [HttpPost]
        [Route("api/Home/Add")]
        public IActionResult Add([FromBody]Content newEntry)
        {
            ICacheClient client = getCacheClient();
            bool added = client.Add("content_" + newEntry.Id.ToString(), newEntry, DateTimeOffset.Now.AddMinutes(10));
            if (!added)
            {
                return BadRequest("Cache error");
            }
            return Ok();
        }

        [Route("api/Home/Seed/{num}/{start}")]
        public IActionResult Seed(int num, int start)
        {
            ICacheClient client = getCacheClient();

            for (int idx = start; idx < start + num; idx++)
            {
                Content newEntry = new Content()
                {
                    Id = idx,
                    Text = string.Format("this is content for ID {0}", idx),
                };
                bool added = client.Add("content_" + newEntry.Id.ToString(), newEntry, DateTimeOffset.Now.AddMinutes(10));
                if (!added)
                {
                    return BadRequest("Cache error");
                }
            }
            
            return Ok();
        }

        [Route("api/Home/SeedBigData/{Id}/{numBytes}")]
        public IActionResult SeedBigData(int Id, int numBytes)
        {
            StringBuilder sb = new StringBuilder(numBytes);
            for (int idx = 0; idx < numBytes; idx++)
            {
                sb.Append('i');
            }
            ICacheClient client = getCacheClient();
            Content newEntry = new Content()
            {
                Id = Id,
                Text = sb.ToString(),
            };
            bool added = client.Add("content_" + newEntry.Id.ToString(), newEntry, DateTimeOffset.Now.AddMinutes(10));
            if (!added)
            {
                return BadRequest("Cache error");
            }
            return Ok();
        }

        [HttpDelete]
        [Route("api/Home/{Id}")]
        public IActionResult Remove(string Id)
        {           
            ICacheClient client = getCacheClient();           
            bool bResult = client.Remove("content_" + Id);
            if (!bResult)
            {                
                return NotFound();
            }
            else
            {
                return NoContent();
            }            
        }

        // GET: http://localhost:61081/api/Home/77
        [Route("api/Home/{key}")]
        public IActionResult Get(string key)
        {
            ICacheClient client = getCacheClient();

            var content = client.Get<Content>("content_" + key);
            
            if (content == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(content);
            }            
        }

        [Route("api/Home")]
        public IActionResult GetAll()
        {
            ICacheClient client = getCacheClient();
            var contentKeysGrab = client.SearchKeys("content_*");           
            var result = client.GetAll<Content>(contentKeysGrab).OrderBy(c => c.Key);

            if (result == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
