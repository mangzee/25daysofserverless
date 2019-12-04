using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Day4
{
    public class User
    {
        private string _Name;

        public User()
        {
            Foods = new List<string>();
        }

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [JsonIgnore]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get { return _Name.ToLower(); } set { _Name = value; } }

        [BsonElement("Foods")]
        public List<string> Foods { get; set; }
    }
}