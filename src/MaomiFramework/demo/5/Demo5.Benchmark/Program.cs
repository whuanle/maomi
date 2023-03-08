using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo5.Benchmark
{
	public class Program
	{
		static void Main()
		{
			var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
			Console.Read();
		}
	}
}
