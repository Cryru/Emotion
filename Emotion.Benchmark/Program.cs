#region Using

using BenchmarkDotNet.Running;

#endregion

namespace Emotion.Benchmark;

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
//#if DEBUG
//                var benchmark = new ImageLoadingBenchmark();
//                benchmark.GlobalSetup();
//                benchmark.LoadPNG();
//                benchmark.LoadImgBin();
//                Console.WriteLine("Done!");
//#else
//                BenchmarkRunner.Run<ImageLoadingBenchmark>();
//#endif
        }

        {
//#if DEBUG
//                var benchmark = new AudioBenchmark();
//                benchmark.GlobalSetup();
//                benchmark.ContinousResampleEmotion();
//                benchmark.ContinousResampleEmotionFast();
//                Console.WriteLine("Done!");
//#else
//                BenchmarkRunner.Run<AudioBenchmark>();
//#endif
        }

        {
//#if DEBUG
//                var benchmark = new ReflectorBenchmark();
//                benchmark.GlobalSetup();
//                benchmark.ReflectorXML_ComplexWithPrimitiveMember();
//                benchmark.ReflectorXML_ComplexWithPrimitiveMember_StackUTF16();
//                benchmark.ReflectorXML_ComplexWithPrimitiveMember_StackUTF8();
//                benchmark.StockEmotionXML_ComplexWithPrimitiveMember();
//                Console.WriteLine("Done!");
//#else
//                BenchmarkRunner.Run<ReflectorBenchmark>();
//#endif
        }

//        {
//#if DEBUG
//            var benchmark = new MiscBenchmark();
//            benchmark.GlobalSetup();
//            Console.WriteLine("Done!");
//#else
//            BenchmarkRunner.Run<MiscBenchmark>();
//#endif
//        }

        BenchmarkRunner.Run<ParticleBenchmark>();
        //BenchmarkRunner.Run<MathsBenchmark>();
    }
}