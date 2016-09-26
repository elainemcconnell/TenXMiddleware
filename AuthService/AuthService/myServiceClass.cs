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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


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
        
        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Eq("username", userID) & builder.Eq((validPassword ? "password" : "pin"), validTxt);

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
            .Include("dob")
            .Include("mobile")
            .Include("email")
            .Include("address");

        return collection.Find(filter).Project(projection).ToList().FirstOrDefault().ToJson();
    }

    static async Task updateUser(string userID, string pin, string email, string mobile, string address)
    {
        var collection = getMongoCollection("users");

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
        
        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Eq("username", userID) & builder.Eq((validPassword ? "password" : "pin"), validTxt);

        var res2 = collection.Find(filter).ToList();

        return (res2.Count() > 0);
    }

    static string getUserPolicyList(string userID, string pin)
    {
        var collection = getMongoCollection("users");

        var filter = Builders<BsonDocument>.Filter.Eq("username", userID) & Builders<BsonDocument>.Filter.Eq("pin", pin);
        var projection = Builders<BsonDocument>.Projection
            .Exclude("_id")
            .Include("policies.policyID")
            .Include("policies.policyType");

        var x = collection.Find(filter).Project(projection).First().ToJson();

        return JObject.Parse(x)["policies"].ToString();
    }

    static string getUserPolicyDetails(string userID, string pin, string policyID)
    {
        var collection = getMongoCollection("users");

        var agg = collection.Aggregate()
                    .Match(new BsonDocument { { "policies.policyID", policyID } })
                    .Unwind("policies")
                    .Match(new BsonDocument { { "policies.policyID", policyID } })
                    .Group(new BsonDocument { {"_id", "$id"}, {"policies", new BsonDocument ( "$push", "$policies" ) } } );

        var x = agg.First().ToJson();

        return JObject.Parse(x)["policies"][0].ToString();

        /*
        var filter = Builders<BsonDocument>.Filter.Eq("username", userID) &
                     Builders<BsonDocument>.Filter.Eq("pin", pin) &
                     Builders<BsonDocument>.Filter.Eq("policies.policyID", policyID);
        var projection = Builders<BsonDocument>.Projection
            .Exclude("_id")
            .Include("policies");

        var x = collection.Find(filter).Project(projection).First().ToJson();

        return JObject.Parse(x)["policies"].ToString();
        */
    }

    static bool addPolicyToUser(string userID, string pin, string policyID)
    {
        var policyCollection = getMongoCollection("policies");

        var builder = Builders<BsonDocument>.Filter;
        var policyFilter = builder.Eq("policyID", policyID);
        var projection = Builders<BsonDocument>.Projection
            .Exclude("_id");
        var policyDoc = policyCollection.Find(policyFilter).Project(projection);

        if (policyDoc.First() == null) return false;

        var userCollection = getMongoCollection("users");
        var userFilter = builder.Eq("username", userID) & builder.Eq("pin", pin);

        return (userCollection.UpdateOne(userFilter, Builders<BsonDocument>.Update.Push("policies", policyDoc.First())).ModifiedCount > 0);

        //TODO: remove policy from policies collection once it's been added to a user.
    }

    static bool getSessionAuthStatus(string sessionID)
    {
        var collection = getMongoCollection("sessions");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", sessionID);
        return (collection.Find(filter).Count() > 0);
    }
    
    static string addSession(string sessionID, string userID, string pin)
    {
        var collection = getMongoCollection("sessions");
        var document = new BsonDocument
        {
            {"_id", sessionID},
            {"username", userID},
            {"pin", pin}
        };

        collection.InsertOne(document);

        var response = JsonConvert.SerializeObject(new
        {
            sessionAuthenticated = true
        });

        return response;
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
        var Client = new MongoClient(ConnectionString);
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
            updateSuccessful = true
        };

        var json = new JavaScriptSerializer().Serialize(jsonData);

        HttpContext.Current.Response.Write(json);
        HttpContext.Current.Response.End();
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void GetUserPolicyDetails(string userID, string pin, string policyID)
    {
        HttpContext.Current.Response.Write(getUserPolicyDetails(userID, pin, policyID));
        HttpContext.Current.Response.End();
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void GetUserPolicyList(string userID, string pin)
    {
        HttpContext.Current.Response.Write(getUserPolicyList(userID, pin));
        HttpContext.Current.Response.End();
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void AddPolicyToUser(string userID, string pin, string policyID)
    {
        var jsonData = JsonConvert.SerializeObject(new
        {
            updateSuccessful = addPolicyToUser(userID, pin, policyID)
        });

        HttpContext.Current.Response.Write(jsonData);
        HttpContext.Current.Response.End();
    }

    #region Desktop Services
    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void IsSessionAuthenticated(string sessionID)
    {
        var jsonData = JsonConvert.SerializeObject(new
        {
            authenticated = getSessionAuthStatus(sessionID)
        });

        //TODO: allow multiple domains
        string origin = HttpContext.Current.Request.Headers["Origin"];
        if (origin == null) origin = ConfigurationManager.AppSettings["GetSessionAuthStatus"];

        AddHeaders(origin);

        HttpContext.Current.Response.Write(jsonData);
        HttpContext.Current.Response.End();
    }

    [WebMethod]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
    public void AddSession(string sessionID, string userID, string pin)
    {
        HttpContext.Current.Response.Write(addSession(sessionID, userID, pin));
        HttpContext.Current.Response.End();
    }
    #endregion

    public void AddHeaders(string origin)
    {
        HttpContext.Current.Response.AppendHeader("Access-Control-Allow-Origin", origin);
        HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "POST,GET,OPTIONS,PUT,DELETE");
        HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept");
        HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");
        HttpContext.Current.Response.ContentType = "application/json";
    }
}
