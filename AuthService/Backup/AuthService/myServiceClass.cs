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






[WebService]
public class myServiceClass : System.Web.Services.WebService

    {
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
