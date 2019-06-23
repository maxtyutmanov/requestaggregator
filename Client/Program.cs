using Client.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using (var http = new HttpClient())
            {
                http.BaseAddress = new Uri("http://localhost:5000");
                var dispatcher = new AggregatedRequestDispatcher<string, bool>(1000, 20);

                Console.WriteLine("Press enter to dispatch a new request. Type 'exit' to exit.");
                var i = 0;
                while (true)
                {
                    ++i;
                    var l = Console.ReadLine();
                    if (l == "exit")
                        return;

                    var id = i.ToString();
                    dispatcher.Dispatch(ExecuteRequest, id).ContinueWith(t =>
                    {
                        Console.WriteLine("Check result for {0}: {1}", id, t.Result);
                    });
                }
            }
        }

        private static async Task<IReadOnlyDictionary<string, bool>> ExecuteRequest(IEnumerable<string> ids)
        {
            using (var http = CreateHttpClient())
            {
                var idsStr = string.Join(',', ids);
                var url = new Uri($"/api/values?ids={idsStr}", UriKind.Relative);

                var response = await http.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var respStr = await response.Content.ReadAsStringAsync();
                var resp = JsonConvert.DeserializeObject<Dictionary<string, bool>>(respStr);
                return resp;
            }
        }

        private static HttpClient CreateHttpClient()
        {
            Console.Error.WriteLine("Creating http client...");
            return new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:5000")
            };
        }
    }
}
