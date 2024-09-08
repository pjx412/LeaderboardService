using CustomerRankService.Services;
using System.Collections.Concurrent;
using System.Net.Http;

namespace TestProject
{
    public class UnitTest1
    {
        LeaderboardService leaderboardService = new LeaderboardService();
        [Fact]
        public async Task Test1Async()
        {
            await RunConcurrentTest();
            //for(var i=1000; i<=1020; i++)
            //{
            //    leaderboardService.UpdateScore(i, i + 100);
            //}
            //var ranklist1 = leaderboardService.GetCustomersByRank(1, 20);
           
            //var rankList5 = leaderboardService.GetCustomersByCustomerId(1003,8,20);
        }

        private async Task RunConcurrentTest()
        {
            const int numberOfThreads = 10;
            const int requestsPerThread = 10;
            var tasks = new ConcurrentBag<Task>();

            for (int i = 0; i < numberOfThreads; i++)
            {
                var threadNumber = i;
                var task = Task.Run(() => {
                    var random = new Random(threadNumber);


                    for (int j = 0; j < requestsPerThread; j++)
                    {

                        try
                        {
                            var customerId = random.Next(1, 100000);
                            var score = random.Next(-1000, 1000);
                            leaderboardService.UpdateScore(customerId, score);
                            var rankList = leaderboardService.GetCustomersByRank(10, 20);
                            var rankList2= leaderboardService.GetCustomersByCustomerId(customerId, 5,2);
                        
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error in thread {threadNumber}: {ex.Message}");
                        }
                        var response = new HttpResponseMessage();

                        if (!response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Error in thread {threadNumber}: {response.StatusCode}");
                        }
                    }

                    return Task.CompletedTask;
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
    }
}