using Polly;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace APICaller.Runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var apiUrls = new string[] { "https://jsonplaceholder.typicode.com/posts", "https://jsonplaceholder.typicode.com/comments", "https://jsonplaceholder.typicode.com/albums", "https://jsonplaceholder.typicode.com/todos", "https://jsonplaceholder.typicode.com/users" };

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Basic Call");
            var basicResults = await BasicCaller(apiUrls);
            Console.WriteLine("Basic Results");
            PrintResult(basicResults);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Resilient Call");
            var resilientResults = await ResilientCaller(apiUrls);
            Console.WriteLine("Resilient Results");
            PrintResult(resilientResults);

            Console.ResetColor();
            Console.ReadLine();
        }

        private static async Task<ResultData[]> BasicCaller(string[] apiUrls)
        {
            var tasks = new List<Task<ResultData>>();
            for (int i = 0; i < apiUrls.Length; i++)
            {
                tasks.Add(GetData(i, apiUrls[i], "BasicCaller"));
            }
            var result = await Task.WhenAll(tasks);
            return result;
        }

        private static async Task<ResultData[]> ResilientCaller(string[] apiUrls)
        {
            var bulkhead = Policy.BulkheadAsync(100, int.MaxValue);
            var tasks = new List<Task<ResultData>>();
            for (int i = 0; i < apiUrls.Length; i++)
            {
                var task = bulkhead.ExecuteAsync(async () =>
                {
                    return await GetData(i, apiUrls[i], "BasicCaller");
                });
                tasks.Add(task);
            }
            var result = await Task.WhenAll(tasks);
            return result;
        }

        private static void PrintResult(ResultData[] results)
        {
            foreach (var result in results)
            {
                Console.WriteLine($"Counter: {result.Counter}");
                Console.WriteLine($"Data: {result.Data.Substring(0, 200)}...");
            }
        }

        private static async Task<ResultData> GetData(int counter, string apiUrl, string callType)
        {
            using var client = new HttpClient();
            Console.WriteLine($"Call type: {callType}");
            var result = await client.GetAsync(apiUrl);
            var data = await result.Content.ReadAsStringAsync();
            return new ResultData { Counter = counter, Data = data };
        }
    }

    class ResultData
    {
        public int Counter { get; set; }
        public string Data { get; set; }
    }
}
