using DSManager.LuaBase;
using MongoDB.Bson;
using MongoDB.Driver;
using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSManager
{
    class DataBase : luabase, Entity
    {
        public IMongoCollection<BsonDocument> userinfocollectio { get; private set; }
        public DataBase() : base("DataBase")
        {
            LuaTable databaseinfor = GetValueFromLua<LuaTable>("databaseinfor");
            string addr = (string)databaseinfor["addr"];
            string databasename = (string)databaseinfor["databasename"];
            string collectionname = (string)databaseinfor["collectionname"];
            var client = new MongoClient(addr);
            //获取database
            var mydb = client.GetDatabase(databasename);
            //获取collection
            userinfocollectio = mydb.GetCollection<BsonDocument>(collectionname);
        }

        void Entity.Begin()
        {
        }

        void Entity.End()
        {
        }

        void Entity.Update(uint delta)
        {
        }
    }
}
