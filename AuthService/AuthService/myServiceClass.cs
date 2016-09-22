using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Security;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Security.Principal;
using System.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;



[WebService]
public class myServiceClass : System.Web.Services.WebService
{
    static IMongoCollection<BsonDocument> getMongoCollection(string collectionName)
    {
        const string ConnectionString = "mongodb://etsid005181.europa.internal:27017";
        var Client = new MongoClient(ConnectionString);
        var blog = Client.GetDatabase("tenxtest");
        return blog.GetCollection<BsonDocument>(collectionName);
    }

    static async Task vaidateUser(string userID, string validTxt, bool validPassword)
    {
        var collection = getMongoCollection("users");

        using (var cursor = await collection.Find(new BsonDocument()).ToCursorAsync())
        {
            while (await cursor.MoveNextAsync())
            {
                foreach (var doc in cursor.Current)
                {
                    Console.WriteLine(doc);
                }
            }
        }

        string validateOn;
        if (validPassword == true)  //we are validating with a password not a pin
        {
            validateOn = "password";
        }
        else
        {
            validateOn = "pin";
        }

        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Eq("username", userID) & builder.Eq(validateOn, validTxt);

        var res2 = collection.Find(filter).ToList();

        var result = await collection.Find(filter).ToListAsync();


    }

    static string getUserDetails(string userID, string pin)
    {
        var collection = getMongoCollection("users");

        var filter = Builders<BsonDocument>.Filter.Eq("username", userID) & Builders<BsonDocument>.Filter.Eq("pin", pin);
        var projection = Builders<BsonDocument>.Projection
            .Exclude("_id")
            .Include("title")
            .Include("forename")
            .Include("surname")
            .Include("ppsn")
            .Include("mobile")
            .Include("email")
            .Include("address");

        return collection.Find(filter).Project(projection).ToList().FirstOrDefault().ToJson();
    }

    static async Task updateUser(string userID, string pin, string email, string mobile, string address)
    {
        var collection = getMongoCollection("users");

        /*
        using (var cursor = await collection.Find(new BsonDocument()).ToCursorAsync())
        {
            while (await cursor.MoveNextAsync())
            {
                foreach (var doc in cursor.Current)
                {
                    Console.WriteLine(doc);
                }
            }
        }
        */

        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Eq("username", userID) & builder.Eq("pin", pin);
        var update = Builders<BsonDocument>.Update
            .Set("email", email)
            .Set("mobile", mobile)
            .Set("address", address)
            .CurrentDate("lastModified");
        var result = await collection.UpdateOneAsync(filter, update);
    }

    static bool vaidateUserSync(string userID, string validTxt, bool validPassword)
    {
        var collection = getMongoCollection("users");


        string validateOn;
        if (validPassword == true)  //we are validating with a password not a pin
        {
            validateOn = "password";
        }
        else
        {
            validateOn = "pin";
        }

        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Eq("username", userID) & builder.Eq(validateOn, validTxt);

        var res2 = collection.Find(filter).ToList();

        if (res2.Count() > 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }


    class Person
    {
        public ObjectId Id { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string pin { get; set; }
        public string name { get; set; }
        public string address { get; set; }
        public string dob { get; set; }
    }


    static async Task CallMain()
    {
        const string ConnectionString = "mongodb://etsid005181.europa.internal:27017";
        //var server = MongoServer.Create(ConnectionString);
        var Client = new MongoClient(ConnectionString);
        // var blog = server.GetDatabase("tenxtest");
        var blog = Client.GetDatabase("tenxtest");
        var collection = blog.GetCollection<BsonDocument>("users");

        using (var cursor = await collection.Find(new BsonDocument()).ToCursorAsync())
        {
            while (await cursor.MoveNextAsync())
            {
                foreach (var doc in cursor.Current)
                {
                    Console.WriteLine(doc);
                }
            }
        }

    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void AuthenticateWithPassword(string userID, string password)
    {
        //bool valid = vaidateUserSync(userID, password, true).Wait();
        bool valid = vaidateUserSync(userID, password, true);

        var jsonData = new
        {
            authenticated = valid
        };


        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(jsonData);

        HttpContext.Current.Response.Write(json);
        HttpContext.Current.Response.End();
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void AuthenticateWithPin(string userID, string pin)
    {
        bool valid = false;
        valid = vaidateUserSync(userID, pin, false);

        var jsonData = new
        {
            authenticated = valid
        };


        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(jsonData);

        HttpContext.Current.Response.Write(json);
        HttpContext.Current.Response.End();
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void GetUserDetails(string userID, string pin)
    {
        HttpContext.Current.Response.Write(getUserDetails(userID, pin));
        HttpContext.Current.Response.End();
    }


    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void SaveUserDetails(string userID, string pin, string email, string mobile, string address)
    {

        updateUser(userID, pin, email, mobile, address).Wait();

        var jsonData = new
        {
            authenticated = true
        };


        var serializer = new JavaScriptSerializer();
        var json = serializer.Serialize(jsonData);

        HttpContext.Current.Response.Write(json);
        HttpContext.Current.Response.End();
    }

    public void AddHeaders(string origin)
    {

        HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Origin", origin);
        HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS,PUT,DELETE");
        HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
        HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");
        HttpContext.Current.Response.ContentType = "application/json";

    }

    public class LoggedInUserData
    {
        public String Message;
    }

}
