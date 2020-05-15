using System;
using System.Linq;

namespace calimolite {
    public class WordGenerator {
        private readonly Array[] weightsList;
        private readonly int weightLength;
        private readonly char[] characters;
        public float[] powers;

        public WordGenerator(string[] words, char[] characters, int powerLength) {
            this.characters = characters;
            weightLength = powerLength;
            var weightLengths = new int[weightLength];
            for (var i = 0; i < weightLengths.Length; i++) {
                weightLengths[i] = characters.Length;
            }

            weightsList = new Array[weightLength];

            for (var i = 0; i < weightLength; i++) {
                weightsList[i] = Array.CreateInstance(typeof(int), Enumerable.Repeat(characters.Length, i + 1).ToArray());
            }

            foreach (var word in words) {
                for (var i = 0; i < word.Length; i++) {
                    for (var j = 1; j < weightLength; j++) {
                        if (0 < i - j) {
                            Add(word.ToLower().AsSpan().Slice(i - j, j));
                        }
                        if (i + j < word.Length) {
                            Add(word.ToLower().AsSpan().Slice(i, j));
                        }
                    }
                }
            }

            void Add(ReadOnlySpan<char> word) {
                var weights = weightsList[word.Length - 1];
                var indices = new int[word.Length];
                for (var i = 0; i < word.Length; i++) {
                    var index = word[i] - 'a';
                    if (index < 0 || 'z' - 'a' < index) {
                        return;
                    }
                    indices[i] = index;
                }
                var weight = (int)weights.GetValue(indices);
                weights.SetValue(weight + 1, indices);
            }
        }

        public string Generate(int seed, int length) {
            var result = new int[length];
            var r = new Random(seed);
            result[0] = r.Next(characters.Length);

            for (var i = 1; i < length; i++) {
                var weights = new float[characters.Length];
                var min = Math.Max(i - weightLength + 1, 0);

                for (var j = min; j < i; j++) {
                    var distance = i - j;
                    for (var c = 0; c < characters.Length; c++) {
                        var indices = new int[distance + 1];
                        for (var k = 0; k < distance; k++) {
                            indices[k] = result[j + k];
                        }
                        indices[distance] = c;
                        weights[c] += powers[distance - 1] * (int)weightsList[distance].GetValue(indices);
                    }
                }

                var sum = weights.Sum();
                var random = r.NextDouble() * sum;
                for (var j = 0; j < weights.Length; j++) {
                    random -= weights[j];
                    if (random <= 0) {
                        result[i] = j;
                        break;
                    }
                }
            }

            return new string(result.Select(i => characters[i]).ToArray());
        }
    }
}