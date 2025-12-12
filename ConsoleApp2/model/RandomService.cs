using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class RandomService
    {
        private Random random;

        public RandomService()
        {
            random = new Random();
        }

        public int Next(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }

        public int Next(int maxValue)
        {
            return random.Next(maxValue);
        }

        public int Next()
        {
            return random.Next();
        }

        public double NextDouble()
        {
            return random.NextDouble();
        }

        public T Choose<T>(IList<T> list)
        {
            return list[random.Next(list.Count)];
        }

        public bool CheckProbability(double probability)
        {
            return random.NextDouble() < probability;
        }

        public double GetBlockCoefficient()
        {
            return 0.7 + random.NextDouble() * 0.3;
        }
    }
}
