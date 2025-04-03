using System.Threading.Channels;
using WTFCalculator.Models;
using WTFCalculator.Services;

namespace WTFCalculator
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                List<Item> items = new List<Item>();

                Console.WriteLine("Enter number of nomenclature: ");
                int numberNomenclature = Convert.ToInt32(Console.ReadLine());

                Console.WriteLine("Enter VAT coefficient (1 by default): ");
                decimal vatCoefficient = decimal.Parse(Console.ReadLine()!);

                Console.Clear();

                for (int i = 0; i < numberNomenclature; i++)
                {
                    Item item = new Item();

                    Console.WriteLine("Enter item name: ");
                    item.Name = Console.ReadLine();

                    Console.WriteLine("Enter the price of the item: ");
                    item.Cost = decimal.Parse(Console.ReadLine()!) * vatCoefficient;

                    Console.WriteLine("Enter the maximum quantity of this item: ");
                    item.Count = Convert.ToInt32(Console.ReadLine());

                    items.Add(item);

                    Console.Clear();
                }

                Console.WriteLine("Enter the prepayment amount: ");
                decimal prepayment = decimal.Parse(Console.ReadLine()!);

                Console.WriteLine("Enter the maximum difference for incorrect results: ");
                decimal difference = decimal.Parse(Console.ReadLine()!);

                Console.Clear();

                Channel<Dictionary<string, int>> channel = Channel.CreateUnbounded<Dictionary<string, int>>
                (
                    new UnboundedChannelOptions { SingleReader = true }
                );

                Channel<Dictionary<string, int>> shitChannel = Channel.CreateUnbounded<Dictionary<string, int>>
                (
                    new UnboundedChannelOptions { SingleReader = true }
                );

                OptimizedCombinationFinderService finder = new OptimizedCombinationFinderService(items, prepayment, channel.Writer, shitChannel, difference);


                using (StreamWriter writer = new StreamWriter("results.txt"))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("GOOD RESULTS: \n\n\n");
                    writer.WriteLine("GOOD RESULTS: \n\n\n");

                    var processingTask = Task.Run(async () =>
                    {
                        async Task ProcessChannel(ChannelReader<Dictionary<string, int>> reader,
                                                ConsoleColor color,
                                                string prefix)
                        {
                            await foreach (var combo in reader.ReadAllAsync())
                            {
                                lock (Console.Out)
                                {
                                    Console.ForegroundColor = color;
                                    Console.WriteLine($"{prefix}: {string.Join(", ", combo)}");
                                    writer.WriteLine($"{prefix}: {string.Join(", ", combo)}");
                                    Console.ResetColor();
                                }
                            }
                        }

                        var mainTask = ProcessChannel(channel.Reader, ConsoleColor.DarkGreen, "GOOD");
                        var shitTask = ProcessChannel(shitChannel.Reader, ConsoleColor.DarkYellow, "SHIT");

                        await finder.FindCombinationsAsync();
                        await Task.WhenAll(mainTask, shitTask);
                    });

                    await processingTask;
                }

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\n\nThe operation is completed, press any button to close the program...");
                Console.ReadKey();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"FATAL ERROR: {exception.Message}");
            }
        }

        /*
        public static void Main(string[] args)
        {
            try
            {
                List<Item> items = new List<Item>();

                Console.WriteLine("Number of nomenclature: ");
                int numberNomenclature = Convert.ToInt32(Console.ReadLine());

                Console.Clear();

                for (int i = 0; i < numberNomenclature; i++)
                {
                    Item item = new Item();

                    Console.WriteLine("Enter item name: ");
                    item.Name = Console.ReadLine();

                    Console.WriteLine("Enter the price of the item: ");
                    item.Cost = decimal.Parse(Console.ReadLine()!);

                    Console.WriteLine("Enter the maximum quantity of this item: ");
                    item.Count = Convert.ToInt32(Console.ReadLine());

                    items.Add(item);

                    Console.Clear();
                }

                Console.WriteLine("Enter the prepayment amount: ");
                decimal prepayment = decimal.Parse(Console.ReadLine()!);

                Console.WriteLine("Enter the maximum difference for incorrect results: ");
                decimal difference = decimal.Parse(Console.ReadLine()!);

                Console.Clear();

                // Console.WriteLine("Enter the payment amount: ");
                // float payment = float.Parse(Console.ReadLine()!);

                CombinationFinderService combinationFinderService = new CombinationFinderService();
                //List<Dictionary<string, int>>? combinations = combinationFinderService.FindCombinations(items, prepayment);

                (List<Dictionary<string, int>> combinations, List<Dictionary<string, int>> shitCombinations) = combinationFinderService.FindCombinations(items, prepayment, difference);



                using (StreamWriter writer = new StreamWriter("results.txt"))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("GOOD RESULTS: \n\n\n");
                    writer.WriteLine("GOOD RESULTS: \n\n\n");

                    foreach (Dictionary<string, int>? combination in combinations)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine(string.Join(", ", combination.Select(keyValuePair => $"{keyValuePair.Key}: {keyValuePair.Value}")));
                        writer.WriteLine(string.Join(", ", combination.Select(keyValuePair => $"{keyValuePair.Key}: {keyValuePair.Value}")));
                    }

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n\n\nSHIT RESULTS WITH DIFFERENCE {difference}: \n\n\n");
                    writer.WriteLine("\n\n\nSHIT RESULTS WITH DIFFERENCE {difference}: \n\n\n");

                    foreach (Dictionary<string, int>? shitCombination in shitCombinations)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                        Console.WriteLine(string.Join(", ", shitCombination.Select(keyValuePair => $"{keyValuePair.Key}: {keyValuePair.Value}")));
                        writer.WriteLine(string.Join(", ", shitCombination.Select(keyValuePair => $"{keyValuePair.Key}: {keyValuePair.Value}")));
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"FATAL ERROR: {exception.Message}");
            }
        }
        */
    }
}
