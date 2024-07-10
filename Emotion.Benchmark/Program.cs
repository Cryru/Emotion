#region Using

using BenchmarkDotNet.Running;

#endregion

namespace Emotion.Benchmark
{
    public class Program
    {
        public static void Main()
        {
            {
                //BenchmarkRunner.Run<Benchmarks>();

                //var benchmark = new Benchmarks();
                //benchmark.GlobalSetup();
                //benchmark.Atlas();
            }

            {
//#if DEBUG
//                var benchmark = new ObjectQueryBenchmark();
//                benchmark.GlobalSetup();
//                benchmark.NewQueryObjectsAllLiving();
//                benchmark.NewQueryObjectsAllLivingAsEnum();
//                benchmark.OldQueryObjectsAllLivingAsEnum();
//                benchmark.NewQueryObjectsAllOfType();
//                benchmark.OldQueryObjectsAllOfType();
//                benchmark.NewQueryObjectsInShape();
//                benchmark.OldQueryObjectsInShape();
//                benchmark.NewQueryObjectsAllOfTypeInShapeAsEnum();
//                Console.WriteLine("Done!");
//#else
//                BenchmarkRunner.Run<ObjectQueryBenchmark>();
//#endif
            }

            {
#if DEBUG
                var benchmark = new ImageLoadingBenchmark();
                benchmark.GlobalSetup();
                benchmark.LoadPNG();
                benchmark.LoadImgBin();
                Console.WriteLine("Done!");
#else
                BenchmarkRunner.Run<ImageLoadingBenchmark>();
#endif
            }
        }
    }
}