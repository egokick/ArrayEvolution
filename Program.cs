using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text; 
using System.Threading.Tasks; 

#pragma warning disable 162

namespace ArrayEvolution
{
    public class PrintAnimal
    {
        public int Health;
        public bool IsAlive => Health > 0;
        public List<string> DNA { get; set; }
        public int DeathAtTick { get; set; }
    }

    public class Animal
    { 
        public string Name { get; set; } = "TestName";
        public int Health { get; set; } = 100;
        public int Resources { get; set; } = 35; 
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public bool IsAlive => Health > 0;
        public List<Action> DNA { get; set; }
        public int DeathAtTick { get; set; }
    }

    public enum Action
    {
        Up,
        Down,
        Left,
        Right,
        Stay, 
        Random
    }
    public class Fitness
    {
        public int Batch { get; set; }
        public int Score { get; set; }
        public List<Action> DNA { get; set; }
    }
    public class Fauna : Animal
    {
    }

    class Program
    {
        private static void Main(string[] args)
        {
            var result = GameIterate();
            Console.Write(result);
        }

        private static bool GameIterate()
        {
            var X = 20;
            var Y = 50;
            List<Animal> animals = null;
            List<Fauna> fauna = null;
            var faunaTotalInitHealth = 0;
            var tick = 1;
            var batch = GetBatchNumber();

            do
            {
                if (animals == null || !animals.Any(a => a.IsAlive) || fauna == null) // if no animals or all animals are dead or no fauna
                {
                    Console.Clear();
                    if (animals != null)
                    {
                        RecordFitness(animals, batch);
                        batch += 1;
                        tick = 1;
                    }
                    animals = GetAnimals(X, Y, batch, 20);
                    fauna = GetFauna(X, Y, 900);
                    faunaTotalInitHealth = fauna.Sum(x => x.Health);
                }

                string[,] array;
                var faunaHealthTotal = fauna.Sum(x => x.Health);
                (array, animals, fauna) = GetArray(X, Y, animals, fauna, faunaTotalInitHealth);
                var faunaHealthDifference = faunaHealthTotal - fauna.Sum(x => x.Health);
                
                if(tick%50==0) Print(array);
                animals = AnimalsTick(animals, X, Y, tick);
                if (tick % 50 == 0) PrintStats(animals, fauna, faunaHealthDifference, faunaTotalInitHealth, tick);
                //await Task.Delay(10);
                tick++;
            }
            while (true);

            return true;
        }

        private static int GetBatchNumber()
        {
            if (!File.Exists("health.json")) return 1;

            var fitnessText = File.ReadAllText("health.json");
            try
            {
                var fitnessHistory = JsonConvert.DeserializeObject<List<Fitness>>(fitnessText);
                return fitnessHistory
                    .OrderByDescending(x => x.Batch)
                    .First()
                    .Batch;
            }
            catch (Exception ex)
            {
                return 1;
            }
        }

        private static void RecordFitness(List<Animal> animals, int batch)
        {
            var fitness = new List<Fitness>();
            animals.ForEach(a =>
            {
                fitness.Add(new Fitness()
                {
                    Batch = batch,
                    DNA = a.DNA,
                    Score = a.DeathAtTick
                });
            });
            if (File.Exists("health.json"))
            {
                try
                {
                    var fitnessHistory = GetFitnessHistory();
                    fitnessHistory.AddRange(fitness);
                    File.WriteAllText("health.json", JsonConvert.SerializeObject(fitnessHistory));
                }
                catch (Exception ex)
                {
                    // if this breaks it will erase all your existing data
                    File.WriteAllText("health.json", JsonConvert.SerializeObject(fitness));
                }
            }
            else
            {
                File.WriteAllText("health.json", JsonConvert.SerializeObject(fitness));
            }
        }

        private static List<Fitness> GetFitnessHistory()
        {
            if (!File.Exists("health.json")) return new List<Fitness>();
            
            var fitnessHistoryText = File.ReadAllText("health.json");
            var fitnessHistory = JsonConvert.DeserializeObject<List<Fitness>>(fitnessHistoryText);
            return fitnessHistory;
        }

        private static List<Fauna> GetFauna(int x, int y, int faunaCount = 35)
        {
            var fauna  = new List<Fauna>();
            var rng = new Random();

            for (var i = 0; i < faunaCount; i++)
            {                                
                var newFauna = new Fauna()
                { 
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
            var faunaAverageHealth = fauna.Any() ? faunaHealthTotal / fauna.Count() : 0;
            Console.WriteLine($"Tick Count: {tickCount}");
            Console.WriteLine($"Fauna Health Delta: {faunaHealthDifference}");
            Console.WriteLine($"Fauna Health Total: {faunaHealthTotal} / {faunaTotalInitHealth} =  { ( fauna.Sum(x => x.Health) / (double) faunaTotalInitHealth) * 100.00 }%" );
            Console.WriteLine($"Fauna Health Consumed: {faunaHealthConsumed}");
            Console.WriteLine($"Fauna Average Health: {faunaAverageHealth}");
            Console.WriteLine($"Average Health Consumed Per Tick: {averageHealthConsumedPerTick}");
            Console.WriteLine($"Maximum Carrying Capacity: TODO");
            foreach (var animal in animals.OrderBy(x=>x.Name).Take(20))
            {
                var animalsJson = JsonConvert.SerializeObject(animal);
                var printAnimal = JsonConvert.DeserializeObject<PrintAnimal>(animalsJson);
                printAnimal.DNA = new List<string>();
                foreach (var action in animal.DNA)
                {
                    printAnimal.DNA.Add(action.ToString());
                }

                var printAnimalJson = JsonConvert.SerializeObject(printAnimal);
                Console.ForegroundColor = !animal.IsAlive ? ConsoleColor.DarkRed : ConsoleColor.Gray;
                Console.WriteLine(printAnimalJson);
            }
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        
        private static List<Animal> AnimalsTick(List<Animal> animals, int x, int y, int tickCount)
        {
            var rng = new Random();            
            animals.Where(x=>x.IsAlive)
            .ToList()
            .ForEach(a =>
            {
                var action = a.DNA.First();
                a.DNA.RemoveAt(0);

                switch (action)
                {
                    case Action.Stay:
                        break;
                    case Action.Left:
                        a.PositionY += -1;
                        break;
                    case Action.Down:
                        a.PositionX += 1;
                        break;
                    case Action.Up:
                        a.PositionX += -1;
                        break;
                    case Action.Right:
                        a.PositionY += 1;
                        break;
                    case Action.Random:
                        var rngX = rng.Next(-1, 2);
                        var rngY = rng.Next(-1, 2);
                        a.PositionX += a.PositionX + rngX > x - 1 || a.PositionX + rngX < 0 ? 0 : rngX;
                        a.PositionY += a.PositionY + rngY > y - 1 || a.PositionY + rngY < 0 ? 0 : rngY;
                        break;
                }
                if (Action.Stay == action)
                {
                    a.Health -= 1; // Decrement resources if not moved
                }

                if (action == Action.Random)
                {
                    a.Health -= 3;
                }
                else
                {
                    a.Health -= 3; // Decrement resources if moved
                }
                if (!a.IsAlive)
                {
                    a.DeathAtTick = tickCount;
                }
                a.DNA.Add(action); // add this action to the end of the list
                
            }
        );

            return animals;
        }

        private static List<Action> GetNewDNA()
        {
            // fitness is determined by the sum of ticks acquired by the group, i.e. higher group tick count is better
            var fitnessHistory = GetFitnessHistory();
            // get Max fitness
            int mostFit = 0;
            List<Action> dna = null;
            foreach (var fitness in fitnessHistory.GroupBy(x => x.Batch))
            {
                var fitnessOfBatch = fitness.Sum(x => x.Score);
                if (fitnessOfBatch > mostFit)
                {
                    mostFit = fitnessOfBatch;
                    dna = fitness.First().DNA;
                }
            }

            dna ??= new List<Action>() {Action.Stay};
            dna = MutateDNA(dna);
            return dna;
        }

        private static List<Action> MutateDNA(List<Action> dna)
        {
            var rng = new Random();
            if (rng.Next(2) == 1) // add to dna sequence
            {
                var indexAddAction = rng.Next(dna.Count());
                dna.Insert(indexAddAction, GetAnyAction());
            }
            if (rng.Next(2) == 1) // add to dna sequence
            {
                var indexAddAction = rng.Next(dna.Count());
                dna.Insert(indexAddAction, GetAnyAction());
            }
            if (rng.Next(2) == 1) // add to dna sequence
            {
                var indexAddAction = rng.Next(dna.Count());
                dna.Insert(indexAddAction, GetAnyAction());
            }
            if (rng.Next(2) == 1) // add to dna sequence
            {
                var indexAddAction = rng.Next(dna.Count());
                dna.Insert(indexAddAction, GetAnyAction());
            }
            if (rng.Next(5) == 1 && dna.Count() > 1) // remove from dna sequence
            {
                var indexRemoveAction = rng.Next(dna.Count());
                dna.RemoveAt(indexRemoveAction);
            }
            //if (rng.Next(2) == 0) 
            //{ 
            // Get an action and replace it with any action
            var indexAction = rng.Next(dna.Count());
            dna[indexAction] = GetAnyAction();
            //}

            return dna;
        }

        private static Action GetAnyAction()
        {
            var rng = new Random();
            var actions = Enum.GetValues(typeof(Action));
            var action = (Action)actions.GetValue(rng.Next(actions.Length));
            return action;
        }

        private static List<Animal> GetAnimals(int x, int y, int batch, int animalCount = 10)
        {
            var animals = new List<Animal>();
            var rng = new Random();
            var dna = GetNewDNA();
            for (var i = 0; i < animalCount; i++)
            {
                animals.Add(new Animal()
                {
                    Name = "Ted" + i,
                    PositionX = rng.Next(0, x),
                    PositionY = rng.Next(0, y - 1),
                    Health = 100,
                    DNA = dna
                });
            }

            return animals;
        }

        private static (string[,], List<Animal>, List<Fauna>) GetArray(int gridX, int gridY, List<Animal> animals, List<Fauna> fauna, int faunaInitHealth)
        {
            //var spacing = (gridX * gridY) / animals.Count;
            var array = new string[gridX, gridY];
            var rng = new Random();
            if (fauna.Sum(x => x.Health ) < faunaInitHealth && rng.Next(0,5) == 4)
            {
                fauna.OrderByDescending(x => x.Health)
                    .Take(fauna.Count() / 2)
                    .ToList()
                    .ForEach(x => x.Health += 1); // give all fauna +1 health
            }

            for (var x = 0; x < array.GetLength(0); x++)
            {
                for (var y = 0; y < array.GetLength(1); y++)
                {
                    var isEndLine = y != array.GetLength(1) - 1;
                    var character = "-";
                    var animal = animals.FirstOrDefault(a => a.IsAlive && a.PositionX == x && a.PositionY == y);
                    var animalInCell = animal != null;
                    var plant = fauna.FirstOrDefault(f => f.IsAlive && f.PositionX == x && f.PositionY == y);
                    var plantInCell = plant != null;

                    if (animalInCell && plantInCell)
                    {
                        animals.Remove(animal);
                        fauna.Remove(plant);
                        var healthToConsume = (animal.Health > 100 ? 0 : animal.Health - 100) * -1;
                        if (healthToConsume > plant.Health) healthToConsume = plant.Health;
                        var plantEatAmount = rng.Next(0, healthToConsume); // Animals can consume more if hungry i.e. if 99 health can consume max of  1 health, if 1 health can consume max of 99 health
                        plant.Health -= plantEatAmount;
                        animal.Health += plantEatAmount;
                        animals.Add(animal);                        
                        if (plant.Health > 35 || rng.Next(8) == 5)
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
            Console.SetCursorPosition(0, 21);
            for(var i = 0; i < 10; i++)
            {
                Console.WriteLine("                                                                                                                                       ");
            }
            Console.SetCursorPosition(0, 21);
        }
    }
}
