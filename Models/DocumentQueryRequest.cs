using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace MongoAPI.Models
{
    public class DocumentQueryRequest
    {
        public string Database{get;set;}
        public string Colection {get;set;}
        public int Page{get;set;}
        public int PageSize {get;set;} = 100;
        public List<FilterRequest> Filters {get;set;} = [];
    }
    public class FilterRequest
    {
        public string Field{get;set;}
        public string Operator{get;set;}
        public object Value {get;set;}
    }
}
