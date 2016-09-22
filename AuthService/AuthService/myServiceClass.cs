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

        static async Task vaidateUser(string userID, string validTxt, bool validPassword)
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

        static async Task updateUser(string userID, string pin)
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

            var builder = Builders<BsonDocument>.Filter;
            var filter = builder.Eq("username", userID) & builder.Eq("pin", pin);
            var update = Builders<BsonDocument>.Update
                .Set("address", "I will buy cakes on Monday for my birthday")
                .CurrentDate("lastModified");
            var result = await collection.UpdateOneAsync(filter, update);

            //var res2 = collection.Find(filter).ToList();

            // var result = await collection.Find(filter).ToListAsync();



            // now we have the user back it the result we need to update with the correct information


        }
        static bool vaidateUserSync(string userID, string validTxt, bool validPassword)
        {
            const string ConnectionString = "mongodb://etsid005181.europa.internal:27017";
            //var server = MongoServer.Create(ConnectionString);
            var Client = new MongoClient(ConnectionString);
            // var blog = server.GetDatabase("tenxtest");
            var blog = Client.GetDatabase("tenxtest");
            var collection = blog.GetCollection<BsonDocument>("users");

       

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
        public void SaveUserDetails(string userID, string pin)
        {
            
            updateUser(userID, pin).Wait();

            var jsonData = new
            {
                authenticated = true
            };


            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(jsonData);

            HttpContext.Current.Response.Write(json);
            HttpContext.Current.Response.End();
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void getDBData()
        {

        } 

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void ConnectWithDatabase()
        {
            CallMain().Wait();
            //var conString = "mongodb://etsid005181.europa.internal:27017";
            //var Client = new MongoClient(conString);
            //var DB = Client.GetDatabase("test");
            //var collection = DB.GetCollection<BsonDocument>("store");

            const string ConnectionString = "mongodb://etsid005181.europa.internal:27017";
            //var server = MongoServer.Create(ConnectionString);
            var Client = new MongoClient(ConnectionString);
            // var blog = server.GetDatabase("tenxtest");
            var blog = Client.GetDatabase("tenxtest");
            var collection = blog.GetCollection<BsonDocument>("users");

            var jsonData = new
            {
                test = "connected"
            };


            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(jsonData);

            HttpContext.Current.Response.Write(json);
            HttpContext.Current.Response.End();
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void GetUser()
        {
            
            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";

            IPrincipal myPrincial = this.User;

            string userName;
            userName = Context.User.Identity.Name;
            // Determine where last backslash is.
            int position = userName.LastIndexOf('\\');
            
            if (position > -1)
            {
                // Return USERAME without domain.
                userName = userName.Substring(position + 1);
             }

            //userName = Page.User.Identity.Name;

            var jsonData = new {
                userId = userName
            };

            
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(jsonData);


            string origin = ConfigurationManager.AppSettings["OriginFMSecurity"];

            AddHeaders(origin);
            
            HttpContext.Current.Response.Write(json);
            HttpContext.Current.Response.End();
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void IsInGroup(string inGroup)
        {

            JavaScriptSerializer js = new JavaScriptSerializer();
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";

            Boolean hasPrivilege;          
            if (this.User.IsInRole(inGroup))
            {
                hasPrivilege = true;
            }
            else 
            {
                hasPrivilege = false;
            }

            string userName;
            userName = Context.User.Identity.Name;
            // Determine where last backslash is.
            int position = userName.LastIndexOf('\\');

            if (position > -1)
            {
                // Return USERAME without domain.
                userName = userName.Substring(position + 1);
            }

            var jsonData = new
            {
                user = userName, 
                group = inGroup,                
                hasPrivilege = hasPrivilege
            };


            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(jsonData);

            string origin = ConfigurationManager.AppSettings["OriginCancelTrades"];

            AddHeaders(origin);

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
