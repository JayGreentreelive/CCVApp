﻿using System;
using Facebook;
using System.Collections.Generic;

namespace CCVApp.Shared
{
    public class FacebookManager
    {
        static FacebookManager _Instance = new FacebookManager();
        public static FacebookManager Instance { get { return _Instance; } }

        FacebookManager( )
        {
        }

        public Dictionary<string, object> CreateLoginRequest( )
        {
            // setup the login dictionary
            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["client_id"] = CCVApp.Shared.Config.GeneralConfig.FBAppID;
            parameters["redirect_uri"] = "https://www.facebook.com/connect/login_success.html";
            parameters["response_type"] = "token";
            parameters["display"] = "touch";

            // add the permissions we want
            parameters["scope"] = CCVApp.Shared.Config.GeneralConfig.FBAppPermissions;

            return parameters;
        }

        public string CreateInfoRequest( )
        {
            return "me";
        }

        public string GetUserID( object infoResult )
        {
            var result = (IDictionary<string, object>)infoResult;
            return (string)result["id"];
        }

        public string GetFirstName( object infoResult )
        {
            var result = (IDictionary<string, object>)infoResult;
            return (string)result["first_name"];
        }

        public string GetLastName( object infoResult )
        {
            var result = (IDictionary<string, object>)infoResult;
            return (string)result["last_name"];
        }

        public string GetEmail( object infoResult )
        {
            var result = (IDictionary<string, object>)infoResult;
            return (string)result["email"];
        }
    }
}