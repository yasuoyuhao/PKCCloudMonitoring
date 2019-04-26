using Base.Helpers;
using CloudKeyFileProvider;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CloudMonitoring.Helpers
{
    class CloudMonitoringHelper
    {
        private readonly IConfiguration _configuration;

        private readonly string jsonKey = "";
        private GoogleCredential googleCredential;

        public CloudMonitoringHelper(IConfiguration configuration)
        {
            this._configuration = configuration;
            jsonKey = KeyProvider.GetCloudKey(_configuration["KeyFilesSetting:KeyFileName:StackdriverMonitoring"], KeyType.GoogleCloudMonitoring);
        }

        public GoogleCredential GetGoogleCredential()
        {
            try
            {
                googleCredential = GoogleCredential.FromJson(jsonKey);
                return googleCredential;
            }
            catch (Exception ex)
            {
                #region Error Catch
                Logger.PringDebug($"-----Error{ System.Reflection.MethodBase.GetCurrentMethod().Name }-----");
                Logger.PringDebug(ex.ToString());
                return null;
                #endregion
            }
        }

        public string GetProjectId()
        {
            string result = _configuration["GCPSetting:PROJECTNAME"];
            return result ?? "";
        }
    }
}
