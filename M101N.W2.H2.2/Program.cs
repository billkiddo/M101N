using System;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Homework_2_2
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

            BsonClassMap.RegisterClassMap<Grade>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(p => p.StudentId).SetElementName("student_id");
            });

            var db = Client.GetDatabase("students");
            //await db.DropCollectionAsync("grades");
            var collection = db.GetCollection<Grade>("grades");

            
            var builder = Builders<Grade>.Filter;
            var filter = builder.Eq(p => p.Type, "homework");

            var list = await collection.Find(filter)
                .SortBy(p => p.StudentId)
                .ThenBy(p => p.Score)
                .ToListAsync();

            var idsToRemove = list
                .GroupBy(p => p.StudentId)
                .Select(grade => grade.First().Id)
                .ToList();

            await collection.DeleteManyAsync(
                Builders<Grade>.Filter.In(p => p.Id, idsToRemove));
        }
    }


    //{ "_id" : ObjectId("50906d7fa3c412bb040eb57b"), "student_id" : 1, 
    // "type" : "exam", "score" : 74.20010837299897 }
    public class Grade
    {
        public ObjectId Id { get; set; }
        public int StudentId { get; set; }
        public string Type { get; set; }
        public double Score { get; set; }
    }
}
