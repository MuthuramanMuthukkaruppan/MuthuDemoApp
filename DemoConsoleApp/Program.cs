using System;
using System.Collections.Generic;
using System.Configuration;
using TwitterDemoAPI.Models;
using TwitterDemoAPI.Client;
using TwitterDemoAPI.DTO.SampleStream;
using TwitterDemoAPI.Models.SampleStream;
using TwitterDemoAPI.Service;
using TwitterDemoAPI.Utils;


//using Tweetinvi;
//using Tweetinvi.Models;
//using Emitter;

namespace DemoConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            /*TwitinfoModel ti = new TwitinfoModel
            {
                TwitJson = "sample twitter info"
            };*/

            //List<SampledStream> lstSampledStream = new List<SampledStream>();
            string _ConsumerKey = ConfigurationManager.AppSettings.Get("ConsumerKey");
            string _ConsumerSecret = ConfigurationManager.AppSettings.Get("ConsumerSecret");
            string _AccessToken = ConfigurationManager.AppSettings.Get("AccessToken");
            string _AccessTokenSecret = ConfigurationManager.AppSettings.Get("AccessTokenSecret");
            string _BearerToken = ConfigurationManager.AppSettings.Get("BearerToken");

            //// Connect to emitter
            //var emitter = Connection.Establish();

            // Set up your credentials
            OAuthInfo oAuthInfo = new OAuthInfo
            {
                AccessSecret = _AccessTokenSecret,
                AccessToken = _AccessToken,
                ConsumerSecret = _ConsumerSecret,
                ConsumerKey = _ConsumerKey,
                BearerToken = _BearerToken
            };


            // Sampled Stream Service Test
            SampledStreamService streamService = new SampledStreamService(oAuthInfo);
            streamService.DataReceivedEvent += StreamService_DataReceivedEvent;
            streamService.StartStream("https://api.twitter.com/2/tweets/sample/stream?expansions=attachments.poll_ids,attachments.media_keys,author_id,entities.mentions.username,geo.place_id,in_reply_to_user_id,referenced_tweets.id,referenced_tweets.id.author_id", 100, 5);

            //streamService.StartStream("https://api.twitter.com/2/tweets/sample/stream", 100, 1);

            //Console.WriteLine("Twitter Analytics");
            /*Console.WriteLine($"{ ti.TwitJson }");*/
        }

        private static void StreamService_DataReceivedEvent(object sender, EventArgs e)
        {
            SampledStreamService.DataReceivedEventArgs eventArgs = e as SampledStreamService.DataReceivedEventArgs;
            SampledStream model = eventArgs.StreamDataResponse;
            
        }
    }
}
