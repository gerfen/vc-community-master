﻿#region

using System.Configuration;
using VirtoCommerce.ApiClient.Utilities;

#endregion

namespace VirtoCommerce.ApiClient
{
    public class CommerceConfiguration
    {
        #region Constructors and Destructors

        public CommerceConfiguration()
        {
            ConnectionString = GetConnectionString("VirtoCommerce");
            ApiAppId = ConfigurationManager.AppSettings["vc-public-ApiAppId"];
            ApiSecretKey = ConfigurationManager.AppSettings["vc-public-ApiSecretKey"];
            IsCacheEnabled = true;
        }

        #endregion

        #region Public Properties

        public string ApiAppId { get; set; }
        public string ApiSecretKey { get; set; }
        public string ConnectionString { get; set; }
        public bool IsCacheEnabled { get; set; }

        #endregion

        #region Methods

        private static string GetConnectionString(string name)
        {
            var connectionString = ConnectionHelper.GetConnectionString(name);

            if (!connectionString.EndsWith("/"))
            {
                connectionString += "/";
            }

            return connectionString;
        }

        #endregion
    }
}
