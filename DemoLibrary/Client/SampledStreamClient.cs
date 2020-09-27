using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using TwitterDemoAPI.Model;
using TwitterDemoAPI.Models;
using TwitterDemoAPI.Models.SampleStream;
using models = TwitterDemoAPI.Models.SampleStream;

namespace TwitterDemoAPI.Client
{
    public class SampledStreamClient
    {
        private string _ConsumerKey = "";
        private string _ConsumerSecret = "";
        private string _BearerToken = "";
        private string _streamEndpoint = "https://api.twitter.com/2/tweets/sample/stream";
        List<string> jsonResponse = new List<string>();

        public event EventHandler StreamDataReceivedEvent;
        public class TweetReceivedEventArgs : EventArgs
        {
            public string StreamDataResponse { get; set; }
        }

        protected void OnStreamDataReceivedEvent(TweetReceivedEventArgs dataReceivedEventArgs)
        {
            if (StreamDataReceivedEvent == null)
                return;
            StreamDataReceivedEvent(this, dataReceivedEventArgs);
        }

        public SampledStreamClient(string consumerKey, string ConsumerSecret, string BearerToken)
        {
            _ConsumerKey = consumerKey;
            _ConsumerSecret = ConsumerSecret;
            _BearerToken = BearerToken;
            //GetBearerToken();
        }

        private void GetBearerToken()
        {
            ////https://dev.twitter.com/oauth/application-only
            ////Step 1
            //string strBearerRequest = HttpUtility.UrlEncode(_ConsumerKey) + ":" + HttpUtility.UrlEncode(_ConsumerSecret);

            //strBearerRequest = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(strBearerRequest));

            ////Step 2
            //WebRequest request = WebRequest.Create("https://api.twitter.com/oauth2/token");
            ////request.Headers.Add("Authorization", "Basic " + strBearerRequest);
            //request.Headers.Add("Authorization", "Basic " + HttpUtility.UrlEncode(_BearerToken));
            //request.Method = "POST";
            //request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

            //string strRequestContent = "grant_type=client_credentials";
            //byte[] bytearrayRequestContent = System.Text.Encoding.UTF8.GetBytes(strRequestContent);
            //System.IO.Stream requestStream = request.GetRequestStream();
            //requestStream.Write(bytearrayRequestContent, 0, bytearrayRequestContent.Length);
            //requestStream.Close();

            //string responseJson = string.Empty;

            //HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //if (response.StatusCode == HttpStatusCode.OK)
            //{
            //    System.IO.Stream responseStream = response.GetResponseStream();
            //    responseJson = new StreamReader(responseStream).ReadToEnd();
            //}

            //JObject jobjectResponse = JObject.Parse(responseJson);

            //_BearerToken = jobjectResponse["access_token"].ToString();
        }

        public void StartStream(string address, int maxTweets, int maxConnectionAttempts)
        {
            int maxTries = maxConnectionAttempts;
            int tried = 0;
            int requestCount = 0;
            List<TweetMetrics> tweetMetric = new List<TweetMetrics>();
            dynamic emojiObj = new JObject();
            // read JSON directly from a file
            using (StreamReader file = File.OpenText(@"c:\MuthuDemoApp\DemoLibrary\Model\emoji.json"))

            using (JsonTextReader reader = new JsonTextReader(file))
            {
               emojiObj = (JArray)JToken.ReadFrom(reader);
            }

            //List<emoji> items = ((JArray)emojiObj).Select(x => new emoji
            //{
            //    codes = (string)x["codes"],
            //    emojichar = (string)x["emojichar"],
            //    name = (string)x["name"],
            //    category = (string)x["category"],
            //    group = (string)x["group"],
            //    subgroup = (string)x["subgroup"]
            //}).ToList();



            while (tried < maxTries)
            {
                tried++;
                try
                {
                    Console.WriteLine("Entered Twitter Analytics Demo at:" + DateTime.Now.ToString("F"));
                    int recordsFetch = 0;

                    WebRequest request = WebRequest.Create(address);
                    request.Headers.Add("Authorization", "Bearer " + _BearerToken);
                    request.Method = "GET";
                    request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";

                    try
                    {
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        requestCount++;
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            //stream opened!
                            using (StreamReader str = new StreamReader(response.GetResponseStream()))
                            {
                                // loop through each item in the Filtered Stream API
                                do
                                {
                                    //if (recordsFetch == maxTweets)
                                    //{
                                    //    break;
                                    //}

                                    string json = str.ReadLine();

                                    if (!string.IsNullOrEmpty(json))
                                    {
                                        // raise an event for a potential client to know we recieved data
                                        OnStreamDataReceivedEvent(new TweetReceivedEventArgs { StreamDataResponse = json});
                                        jsonResponse.Add(json);
                                        recordsFetch = recordsFetch + 1;
                                        Console.WriteLine("records fetched:" + recordsFetch + " at " + DateTime.Now.ToString("F"));
                                        Console.WriteLine("Twitter Data : \n" + json);
                                    }
                                } while (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable()
                                            && !str.EndOfStream && recordsFetch <= maxTweets);

                                //Store Tweet Metric info in memory -- Later we can use MongoDB
                                TweetMetrics tw = new TweetMetrics();
                                tw.TotalTweets = recordsFetch;
                                
                                int _URLCount = 0;
                                int _PhotoURLCount = 0;
                                int _EmojiCount = 0;
                                StringBuilder _hastags = new StringBuilder();
                                foreach (var data in jsonResponse)
                                {
                                    DTO.SampleStream.SampledDTOStream resultsDTO = JsonConvert.DeserializeObject<DTO.SampleStream.SampledDTOStream>(data);

                                    if (data.Contains("http"))
                                        _URLCount++;
                                    if (data.Contains("pic.twitter.com") || data.Contains("instagram"))
                                        _PhotoURLCount++;
                                    if(resultsDTO.data.entities != null)
                                    if(resultsDTO.data.entities.hashtags != null)
                                    {
                                        var s = resultsDTO.data.entities;
                                        foreach (var h in s.hashtags)
                                        {
                                            _hastags.AppendLine(h.tag);
                                        }
                                    }


                                    foreach(var j in emojiObj)
                                    {
                                        if (data.Contains(j.unified))
                                            _EmojiCount++;
                                    }
                                }
                                tw.Tweet_URL = _URLCount;
                                tw.Tweet_photoURL = _PhotoURLCount;
                                tw.Tweet_Emoji = _EmojiCount;
                                tw.Top_HashTags = _hastags;
                                tweetMetric.Add(tw);
                            }
                            Console.WriteLine("Exited Entered Twitter Analytics Processing Demo at:" + DateTime.Now.ToString("F"));
                        }
                        else
                        {
                            Console.WriteLine("response.StatusCode not HttpStatusCode.OK. Currently: " + response.StatusCode + " " + response.StatusDescription);
                        }
                    }
                    catch (WebException ex)
                    {
                        Console.WriteLine(ex.Message);

                    }
                    catch (Exception ex)
                    {
                        // Something more serious happened. like for example you don't have network access
                        // we cannot talk about a server exception here as the server probably was never reached
                        Console.WriteLine(ex.Message);
                    }
                    //we double-check the tries here just so if we aren't "trying" again we don't unnecessarily wait a few seconds
                    if (tried < maxTries)
                        System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(10));
                }
                catch (Exception ex)
                {
                    if (tried < maxTries)
                        System.Threading.Thread.Sleep(System.TimeSpan.FromSeconds(10));
                    Console.WriteLine(ex.Message);
                }
            }

            var TotalTweets = 0;
            var URLCount = 0;
            var PhotoURLCount = 0;
            var EmojiCount = 0;
            StringBuilder hashtags = new StringBuilder();

            foreach (var tw in tweetMetric)
            {
                TotalTweets += tw.TotalTweets;
                URLCount += tw.Tweet_URL;
                PhotoURLCount += tw.Tweet_photoURL;
                EmojiCount += tw.Tweet_Emoji;
                hashtags.AppendLine(tw.Top_HashTags.ToString());
            }

            Console.WriteLine("Data Analytics Report:");
            Console.WriteLine("Total Number of Tweets Received: " + TotalTweets);
            Console.WriteLine("Average of Tweets per Hour/Minute/Second : " + ((double)TotalTweets / 3600).ToString() + "/" + ((double)TotalTweets / 60).ToString() + "/" + (TotalTweets).ToString());
            Console.WriteLine("Total Number of URLs in Tweets: " + URLCount);
            Console.WriteLine("% of Tweets contains URL: " + ((double)URLCount / 100).ToString("P", CultureInfo.InvariantCulture));
            Console.WriteLine("Total Number of Photo URLs in Tweets: " + PhotoURLCount);
            Console.WriteLine("% of Tweets contains Photo URL: " + ((double)PhotoURLCount / 100).ToString("P", CultureInfo.InvariantCulture));
            Console.WriteLine("Hashtags: " + hashtags);

        }

    }
}
