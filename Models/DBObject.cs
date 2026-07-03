using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MongoAPI.Models
{
    public class DBObject
    {
        string Name {get;set;}
        List<string> ColectionNames {get;set;}

        public DBObject(string name,List<string> colectionnames)
        {
            Name= name;
            ColectionNames = colectionnames;
        }
    }
}