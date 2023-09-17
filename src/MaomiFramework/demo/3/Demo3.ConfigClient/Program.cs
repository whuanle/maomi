using Microsoft.Extensions.Configuration;

namespace Demo3.ConfigClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(5000);
            var builder = new ConfigurationBuilder()
                .AddReomteConfig("http://127.0.0.1:5000/config", "myapp", "dev");

            var config = builder.Build();
            while (true)
            {
                Console.WriteLine(config["Name"]);
                Thread.Sleep(1000);
            }
        }
    }
}