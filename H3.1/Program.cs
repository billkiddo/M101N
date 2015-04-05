using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace H3._1
{
    internal class Program
    {
        private static readonly MongoClient Client =
            new MongoClient("mongodb://localhost:27017");

        private static void Main(string[] args)
        {
            if (args == null) throw new ArgumentNullException("args");
            DoHomework().Wait();

            Console.WriteLine("Press Enter");
            Console.ReadLine();
        }

        private static async Task DoHomework()
        {
            ConventionRegistry.Register("camelCase", new ConventionPack {new CamelCaseElementNameConvention()},
                t => true);
            BsonClassMap.RegisterClassMap<Student>(cm => { cm.AutoMap(); });
            BsonClassMap.RegisterClassMap<Score>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(p => p.Value).SetElementName("score");
            });

            var db = Client.GetDatabase("school");
            var collection = db.GetCollection<Student>("students");

            var studentsList = await collection.Find(new BsonDocument())
                .ToListAsync();

            var tasks = new List<Task>();
            foreach (var student in studentsList)
            {
                student.Scores.Remove(student.Scores.Where(p => p.Type == "homework").OrderBy(p => p.Value).First());
                tasks.Add(
                    collection.UpdateManyAsync(Builders<Student>.Filter.Eq(p => p.Id, student.Id),
                        Builders<Student>.Update.Set(p => p.Scores, student.Scores)));
            }

            Console.WriteLine("Update...");
            await Task.WhenAll(tasks);
            Console.WriteLine("Updtae Done.");
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Score> Scores { get; set; }
    }

    public class Score
    {
        public string Type { get; set; }
        public double Value { get; set; }
    }
}
