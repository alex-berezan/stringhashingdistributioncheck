using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StringHashingDistributionCheck
{
	class Program
	{
		private static readonly Random _random = new Random();
		private static readonly int MinLength = 0;
		private static readonly int MaxLength = 0;
		private static readonly string Prefix = "prefix";
		private static readonly int StringCount = 50000000;// 200 mb
		private static readonly int ItemsPerBucket = 100;
		private static readonly char MinChar = '1';
		private static readonly char MaxChar = '4';


		private static readonly int Iterations = 10;

		static void Main(string[] args)
		{
			List<string> columns = new List<string> { "Min", "Max", "Median", "Average", "Total" };
			PrintRow(columns);

			var tasks = new int[Iterations].Select((_, i) => DoBenchmarkIterationAsync()).ToArray();

			foreach (var task in tasks) task.RunSynchronously();
			Task.WaitAll(tasks);

			//for (int j = 0; j < Iterations; j++)
			//{
			//	DoBenchmarkIterationAsync().Start();
			//}

			Console.WriteLine("Done, hit any key");
			Console.ReadKey();
		}

		private static Task DoBenchmarkIterationAsync()
		{
			return new Task(() =>
			{
				List<string> list = new List<string>(StringCount);
				for (int i = 0; i < StringCount; i++)
				{
					list.Add(GenerateRandomString(i));
				}

				List<string>[] buckets = Distribute(list);
				AnalyzeBuckets(buckets);
			});
		}

		private static void AnalyzeBuckets(List<string>[] buckets)
		{
			List<int> counts = buckets.Select(x => x.Count).ToList();
			counts.Sort();

			int totalCount = counts.Sum();
			int minCount = counts.First();
			int maxCount = counts.Last();
			int medianCount = counts[counts.Count / 2];
			int averageCount = (int)counts.Average();

			PrintRow(new List<int> { minCount, maxCount, medianCount, averageCount, totalCount });
		}

		private static void PrintRow(IEnumerable values)
		{
			string row = string.Join("\t", values.OfType<object>().Select(x => x.ToString()));
			Console.WriteLine(row);
		}

		#region Distribute

		private static List<string>[] Distribute(List<string> list)
		{
			int bucketsBaseCount = (list.Count % ItemsPerBucket == 0 ? 0 : 1) + list.Count / ItemsPerBucket;
			List<string>[] buckets = new List<string>[bucketsBaseCount];
			for (int i = 0; i < buckets.Length; i++)
			{
				buckets[i] = new List<string>();
			}

			foreach (var pair in list.Select(s => new { value = s, bucket = GetBucketIndex(s, bucketsBaseCount) }))
			{
				buckets[pair.bucket].Add(pair.value);
			}

			return buckets;
		}

		private static int GetBucketIndex(string s, int bucketsBaseCount)
		{
			int hash = GetJenkingHashCode(s);
			return Math.Abs(hash) % bucketsBaseCount;
		}

		private static int GetJenkingHashCode(string s)
		{
			int hash, i;
			for (hash = i = 0; i < s.Length; ++i)
			{
				hash += (int)s[i];
				hash += (hash << 10);
				hash ^= (hash >> 6);
			}
			hash += (hash << 3);
			hash ^= (hash >> 11);
			hash += (hash << 15);
			return hash;
		}

		#endregion

		#region Distribute 2

		private static List<string>[] Distribute2(List<string> list)
		{
			int bucketsBaseCount = (list.Count % ItemsPerBucket == 0 ? 0 : 1) + list.Count / (ItemsPerBucket * 2);
			List<string>[] buckets = new List<string>[bucketsBaseCount * 2];
			for (int i = 0; i < buckets.Length; i++)
			{
				buckets[i] = new List<string>();
			}

			foreach (var pair in list.Select(s => new { value = s, bucket = GetBucketIndex(s, bucketsBaseCount) }))
			{
				buckets[pair.bucket].Add(pair.value);
			}

			return buckets;
		}

		private static int GetBucketIndex2(string s, int bucketsBaseCount)
		{
			int hash = s.GetHashCode();
			return (Math.Abs(hash) % bucketsBaseCount) + (hash > 0 ? 0 : bucketsBaseCount);
		}

		#endregion

		private static string GenerateRandomString(int num)
		{
			int length = _random.Next(MinLength, MaxLength + 1);
			char[] sb = new char[length];
			for (int i = 0; i < length - 1; i++)
			{
				int r = _random.Next((int)MinChar, (int)MaxChar + 1);
				sb[i] = (char)r;
			}
			var s = new string(sb);
			return Prefix + s + num;
		}
	}
}
