# RedisNetCore20Example
A .net core 2.0 example of a Redis client.

I called this solution RedisFreeOne because it uses a free open source library (see below).

This project was thrown together very quick as a spike to create a Redis client in .NET Core 2.0.  Ultimately, this was to explore aws ElasticCache
using the Redis engine. 

The ability to store complex objects was mandatory.

I initially used the StackService.Redis library which had a super nice API that supported generics, complex objects, etc..  I then discovered 
that was not a free api  :-)

I then came across the free StackExchange.Redis and compiled the StackExchange.Redis.Extensions as that did support complex objects. 
I could not find them on Nuget so I compiled and brought in the .NET Core 2.0 dll's for that and included them as assemblies in this project.

You can find that at

https://github.com/imperugo/StackExchange.Redis.Extensions

You can import the following file into Postman which will give you a collection of the various ways you can hit the API
redisdemopostman.json  
