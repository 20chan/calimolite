using System;
using System.Linq;

namespace calimolite {
    class Program {
        static void Main(string[] args) {
            if (args.Length <= 0 || !int.TryParse(args[1], out var powerLength)) {
                Console.WriteLine("usage: wfc-sample {wordlistfile} {powerlen}");
                return;
            }

            var words = System.IO.File.ReadAllLines(args[0]);
            var generator = new WordGenerator(words, powerLength);
            generator.powers = new float[powerLength];
            Console.WriteLine("ready");

            while (true) {
                var read = Console.ReadLine();
                if (0 < read.Length && read[0] == '/') {
                    generator.powers = read.TrimStart('/').Split().Select(s => float.Parse(s)).ToArray();
                    Console.WriteLine("powers set");
                } else {
                    var generated = generator.Generate(Environment.TickCount);
                    Console.WriteLine(generated);
                }
            }
        }
    }
}