using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

namespace Q7
{
    class Program
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
            BsonClassMap.RegisterClassMap<Album>(cm => { cm.AutoMap(); });
            BsonClassMap.RegisterClassMap<Image>(cm => { cm.AutoMap(); });

            var db = Client.GetDatabase("photosharing");

            var albums = db.GetCollection<Album>("albums");
            var albumsList = await albums.Find(new BsonDocument())
                .ToListAsync();

            var images = db.GetCollection<Image>("images");
            var imagesList = await images.Find(new BsonDocument())
                .ToListAsync();

            Console.WriteLine(
                "As as a sanity check, there are 49,887 images that are tagged 'sunrises' before you remove the images");
            var cnt = imagesList.Count(p => p.Tags.Contains("sunrises"));
            Console.WriteLine(cnt);

            var imagesToDelete = (from image in imagesList
                let found = albumsList.Any(album => album.Images.Contains(image.Id))
                where !found
                select image.Id).ToList();
            imagesList.RemoveAll(p => imagesToDelete.Contains(p.Id));

            Console.WriteLine("After cleaning op");
            cnt = imagesList.Count(p => p.Tags.Contains("sunrises"));
            Console.WriteLine(cnt);

        }
    }

    public class Album
    {
        public int Id { get; set; }
        public List<int> Images { get; set; }
    }

    public class Image
    {
        public int Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public List<string> Tags { get; set; }
    }
}
