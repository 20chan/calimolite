using System;
using System.Collections.Generic;
using System.Linq;

namespace calimolite {
    public class WordGenerator {
        private static readonly char[] characters = "abcdefghijklmnopqrstuvxyz\0".ToCharArray();

        private readonly Array[] weightsList;
        private readonly int weightLength;
        public float[] powers;

        public WordGenerator(string[] words, int powerLength) {
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
                var wordPadded = word + '\0';
                for (var i = 0; i < wordPadded.Length; i++) {
                    for (var j = 1; j < weightLength; j++) {
                        if (0 < i - j) {
                            Add(wordPadded.ToLower().AsSpan().Slice(i - j, j));
                        }
                        if (i + j < word.Length) {
                            Add(wordPadded.ToLower().AsSpan().Slice(i, j));
                        }
                    }
                }
            }

            void Add(ReadOnlySpan<char> word) {
                var weights = weightsList[word.Length - 1];
                var indices = new int[word.Length];
                for (var i = 0; i < word.Length; i++) {
                    if (!TryGetCharIndex(word[i], out var index)) {
                        return;
                    }
                    indices[i] = index;
                }
                var weight = (int)weights.GetValue(indices);
                weights.SetValue(weight + 1, indices);
            }

            bool TryGetCharIndex(char c, out int index) {
                if (c == '\0') {
                    index = characters.Length - 1;
                    return true;
                }
                index = c - 'a';
                return index >= 0 && 'z' - 'a' >= index;
            }
        }

        public string Generate(int seed) {
            var result = new List<int>();
            var r = new Random(seed);
            result.Add(r.Next(characters.Length));

            for (var i = 1;; i++) {
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
                        if (j == weights.Length - 1) {
                            return new string(result.Select(c => characters[c]).ToArray());
                        }
                        result.Add(j);
                        break;
                    }
                }
            }
        }
    }
}