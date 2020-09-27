namespace TwitterDemoAPI.Models
{
    public class DataAnalytics
	{
		public int TotalTweets { get; set; }
		public double PercentTweet_URL { get; set; }
		public string Top_Emoji { get; set; }
		public string Top_HashTags { get; set; }
		public string AverageTweets { get; set; }
		public double PercentTweet_photoURL { get; set; }
		public string Top_URL_Domains { get; set; }
	}
}