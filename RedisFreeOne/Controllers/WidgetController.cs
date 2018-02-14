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
    public class Widget
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
    /*
    http://localhost:61081/api/Widget
    http://localhost:61081/api/Home/Widget/10/0
    http://localhost:61081/api/Home/Widget/88/51200000
    Had to bring in the StackExchange.Redis.Extensions project into studio, compile it and then add as reference
        so that we can enjoy complex objects
    */
    public class WidgetController : Controller
    {
        private ICacheClient getCacheClient()
        {
            var mux = ConnectionMultiplexer.Connect(new ConfigurationOptions
            {
                //DefaultVersion = new Version(3, 0, 500),
                EndPoints = { { "localhost", 6379 } },
                AllowAdmin = true
            });

            return new StackExchangeRedisCacheClient(mux, new BinarySerializer());
        }
        [Route("api/Widget/Add")]
        [HttpPost]
        public IActionResult Add([FromBody]Widget newEntry)
        {            
            ICacheClient client = getCacheClient();
            
            // this also does an upsert.
            bool added = client.Add("widget_" + newEntry.Id.ToString(), newEntry, DateTimeOffset.Now.AddMinutes(10));
            if (!added)
            {
                return BadRequest("Cache error");
            }

            return Ok();
        }

        [Route("api/Widget/Seed/{num}/{start}")]
        public IActionResult Seed(int num, int start)
        {
            ICacheClient client = getCacheClient();
                           
            for (int idx = start; idx < start + num; idx++)
            { 
                Widget newWidget = new Widget()
                {
                    Id = idx,
                    Name = string.Format("Widget {0}", idx),
                    Price = (decimal)(idx + 1.5m)
                };

                bool added = client.Add("widget_" + newWidget.Id.ToString(), newWidget, DateTimeOffset.Now.AddMinutes(10));
                if (!added)
                {
                    return BadRequest("Cache error");
                }
            }
            return Ok();
        }

        [Route("api/Widget/SeedBigData/{Id}/{numBytes}")]
        public IActionResult SeedBigData(int Id, int numBytes)
        {
            StringBuilder sb = new StringBuilder(numBytes);
            for (int idx = 0; idx < numBytes; idx++)
            {
                sb.Append('i');
            }
            ICacheClient client = getCacheClient();
            Widget bigDataWidget = new Widget()
            {
                Id = Id,
                Name = sb.ToString(),
                Price = 5m
            };
            bool added = client.Add("widget_" + bigDataWidget.Id.ToString(), bigDataWidget, DateTimeOffset.Now.AddMinutes(10));
            if (!added)
            {
                return BadRequest("Cache error");
            }
            return Ok();
        }
        [HttpDelete]
        [Route("api/Widget/{Id}")]
        public IActionResult Remove(string Id)
        {
            ICacheClient client = getCacheClient();
            bool bResult = client.Remove("widget_" + Id);
            if (!bResult)
            {
                return NotFound();
            }
            else
            {
                return NoContent();
            }
        }
        // GET: http://localhost:61081/api/Widget/77
        [Route("api/Widget/{key}")]
        public IActionResult Get(string key)
        {
            ICacheClient client = getCacheClient();

            var content = client.Get<Widget>("widget_" + key);

            if (content == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(content);
            }
        }

        [Route("api/Widget")]
        public IActionResult GetAll()
        {
            ICacheClient client = getCacheClient();
            var contentKeysGrab = client.SearchKeys("widget_*");
            var result = client.GetAll<Widget>(contentKeysGrab).OrderBy(c => c.Key);

            if (result == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(result);
            }
        }
    }
}
