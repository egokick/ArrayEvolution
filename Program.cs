using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq; 
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml;
using Formatting = Newtonsoft.Json.Formatting;

#pragma warning disable 162

namespace ArrayEvolution
{


    public class Animal
    { 
        public string Id { get;set;}
        public string Name { get; set; } = "TestName";
        public int Health { get; set; } = 100;
        public int Resources { get; set; } = 35;
        public int Size { get; set; } = 1;
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public bool IsAlive
        {
            get
            {
                if (Health > 0) return true;
                else return false;
            }
        }
    }

    public class Fauna : Animal
    {
    }

    class Program
    {
        

        private static void Main(string[] args)
        {
            var result = GameIterate().Result;
            Console.Write(result);
        }

        private static async Task<bool> GameIterate()
        {
            var X = 20;
            var Y = 50;
            var animals = GetAnimals(X, Y, 9);
            var fauna = GetFauna(X, Y, 900);
            var FaunaTotalInitHealth = fauna.Sum(x => x.Health);
            int i = 1;
            do
            {                
                string[,] array;
                var faunaHealthTotal = fauna.Sum(x => x.Health);
                (array, animals, fauna) = GetArray(X, Y, animals, fauna);
                var faunaHealthDifference = faunaHealthTotal - fauna.Sum(x => x.Health);
                Print(array);
                animals = AnimalsTick(animals, X, Y);
                PrintStats(animals, fauna, faunaHealthDifference, FaunaTotalInitHealth, i);
                await Task.Delay(10);
                i++;
            }
            while (true);

            return true;
        }
                

        private static List<Fauna> GetFauna(int x, int y, int faunaCount = 35)
        {
            var fauna  = new List<Fauna>();
            var rng = new Random();

            for (var i = 0; i < faunaCount; i++)
            {                                
                var newFauna = new Fauna()
                {
                    Id = new Guid().ToString(),
                    Name = "plant" + i,
                    PositionX = rng.Next(0, x),
                    PositionY = rng.Next(0, y - 1),
                    Resources = rng.Next(1, 25),
                    Health = rng.Next(35, 100) // health of fauna is directly proportional to how many resource they provide
                };
                if (!fauna.Any(x => x.PositionY == newFauna.PositionY && x.PositionX == newFauna.PositionX))// if fauna doesn't already exist at this position, add it
                {
                    fauna.Add(newFauna);
                }
            }            
            return fauna;
        }
        private static void PrintStats(List<Animal> animals, List<Fauna> fauna, int faunaHealthDifference = 0, int faunaTotalInitHealth = 0, int tickCount = 0)
        {
            Console.WriteLine();
            var faunaHealthTotal = fauna.Sum(x => x.Health);
            var faunaHealthConsumed = faunaTotalInitHealth - fauna.Sum(x => x.Health);
            var averageHealthConsumedPerTick = faunaHealthConsumed == 0 ? 0 : faunaHealthConsumed / tickCount;
            var faunaAverageHealth = fauna.Count() > 0 ? faunaHealthTotal / fauna.Count() : 0;
            Console.WriteLine($"Tick Count: {tickCount}");
            Console.WriteLine($"Fauna Health Delta: {faunaHealthDifference}");
            Console.WriteLine($"Fauna Health Total: {faunaHealthTotal} / {faunaTotalInitHealth} =  { ( fauna.Sum(x => x.Health) / (double) faunaTotalInitHealth) * 100.00 }%" );
            Console.WriteLine($"Fauna Health Consumed: {faunaHealthConsumed}");
            Console.WriteLine($"Fauna Average Health: {faunaAverageHealth}");
            Console.WriteLine($"Average Health Consumed Per Tick: {averageHealthConsumedPerTick}");
            foreach (var animal in animals.OrderBy(x=>x.Name))
            {
                var animalsJson = JsonConvert.SerializeObject(animal);
                Console.WriteLine(animalsJson);
            }
        }
        
        private static List<Animal> AnimalsTick(List<Animal> animals, int x, int y)
        {
            var rng = new Random();
            foreach(var a in animals)
            {
                var rngX = rng.Next(-1, 2);
                var rngY = rng.Next(-1, 2);
                if (rngX != 0 || rngY != 0)
                {
                    a.Health += -3; // Decrement resources if animal moved
                }
                else
                {
                    a.Health += -1; // Decrement resources if not moved
                }
                // walk randomly;
                a.PositionX += a.PositionX + rngX > x - 1 || a.PositionX + rngX < 0 ? 0 : rngX;
                a.PositionY += a.PositionY + rngY > y - 1 || a.PositionY + rngY < 0 ? 0 : rngY;
            }
            //animals.ForEach(a =>
            //    {
            //        var rngX = rng.Next(-1, 2);
            //        var rngY = rng.Next(-1, 2);
            //        if (rngX != 0 || rngY != 0)
            //        {
            //            a.Health += -3; // Decrement resources if animal moved
            //        }
            //        else
            //        {
            //            a.Health+= -1; // Decrement resources if not moved
            //        }                    
            //        // walk randomly;
            //        a.PositionX += a.PositionX + rngX > x - 1 || a.PositionX + rngX < 0 ? 0 : rngX;
            //        a.PositionY += a.PositionY + rngY > y - 1 || a.PositionY + rngY < 0 ? 0 : rngY;
            //    }
            //);

            //if (animals.All(a => a.Resources <= 4 || !a.IsAlive))
            //{
            //    animals = GetAnimals(x, y, 7); // reset the animals because everyone is low energy or dead
            //}
            return animals.Where(animal=>animal.IsAlive).ToList();
        }
        private static List<Animal> GetAnimals(int x, int y, int animalCount = 10)
        {
            var animals = new List<Animal>();
            var rng = new Random();
            
            for (var i = 0; i < animalCount; i++)
            {
                animals.Add(new Animal()
                {
                    Name = "Ted" + i,
                    PositionX = rng.Next(0, x),
                    PositionY = rng.Next(0, y - 1)
                });
            }

            return animals;
        }

        private static (string[,], List<Animal>, List<Fauna>) GetArray(int gridX, int gridY, List<Animal> animals, List<Fauna> fauna)
        {
            //var spacing = (gridX * gridY) / animals.Count;
            var array = new string[gridX, gridY];
            var rng = new Random();

            for (var x = 0; x < array.GetLength(0); x++)
            {
                for (var y = 0; y < array.GetLength(1); y++)
                {
                    var isEndLine = y != array.GetLength(1) - 1;
                    var character = "-";
                    var animal = animals.FirstOrDefault(a => a.PositionX == x && a.PositionY == y);
                    var animalInCell = animal != null;
                    var plant = fauna.FirstOrDefault(f => f.IsAlive && f.PositionX == x && f.PositionY == y);
                    var plantInCell = plant != null;

                    if (animalInCell && plantInCell)
                    {
                        animals.Remove(animal);
                        fauna.Remove(plant);
                        var healthToConsume = (animal.Health - 100) * -1;
                        if (healthToConsume > plant.Health) healthToConsume = plant.Health;
                        var plantEatAmount = rng.Next(0, healthToConsume); // Animals can consume more if hungry i.e. if 99 health can consume max of  1 health, if 1 health can consume max of 99 health
                        plant.Health -= plantEatAmount;
                        animal.Health += plantEatAmount;
                        animals.Add(animal);
                        if(plant.Health > 0)
                        {
                            fauna.Add(plant);
                        }
                        character = "x";
                    }
                    else if (animalInCell) character = "x";
                    else if (plantInCell) character = "t";
                    
                    var cell = isEndLine ? character : Environment.NewLine;
                    array[x, y] = cell;
                }
            }
            return (array, animals, fauna);
        }

        private static void Print(string[,] array)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < array.GetLength(0); i++)
            {
                for (var j = 0; j < array.GetLength(1); j++)
                {
                    sb.Append(array[i, j]);
                }
            }
            Console.SetCursorPosition(0,0);
            var defaultColor = Console.ForegroundColor;
            foreach (var c in sb.ToString())
            {
                if (c == 'x') Console.ForegroundColor = ConsoleColor.Cyan;
                if (c == 't') Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(c);
                Console.ForegroundColor = defaultColor;
            }
            
        }
    }
}
