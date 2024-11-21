using Common.PubSub;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading;
using net_test_generator_svc.Models;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Common.Interfaces;
using Common.Logging;

namespace net_test_generator_svc.Repositories.PubSub
{
    public interface IDriverPubSub
    {
        Task<bool> PublishDriverToHO(Ztdrmd driver);
    }

    public class UpdateDriverPubSub : IDriverPubSub
    {
        //SESUAIKAN DENGAN KEBUTUHAN
        private IPubSubService _pubsubService;
        public UpdateDriverPubSub(IConfiguration configuration)
        {
            /// Setup GCP variables
            var projectId = configuration["GCP:ProjectID"];
            var topicId = configuration["GCP:Topics:TopicAction"];
            var subscriptionId = configuration["GCP:Subscriptions:TopicAction"];
            var pubSubCredentials = configuration["GCP:Credentials"];

            if (!String.IsNullOrEmpty(pubSubCredentials))
            {
                pubSubCredentials = Directory.GetCurrentDirectory() + $"/{pubSubCredentials}";
                _pubsubService = new PubSubService(pubSubCredentials, projectId, subscriptionId, topicId);
            }
            else
            {
                _pubsubService = new PubSubService(projectId, subscriptionId, topicId);
            }
        }

        public async Task<bool> PublishDriverToHO(Ztdrmd driver)
        {
            try
            {
                await _pubsubService.PublishAsync(driver);
                return true;    
            }
            catch (System.Exception ex)
            {
                SDLogging.Log($"Error publish data.\nError : {ex.Message}", SDLogging.ERROR);
                return false;
            }            
        }
    }

    //DUMMY kebutuhan class di atas
    public class Ztdrmd
    {
        public string id {get;set;}
    }
}