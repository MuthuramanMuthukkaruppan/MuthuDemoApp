using System.Text;
using TwitterDemoAPI.DTO.SampleStream;

namespace TwitterDemoAPI.Models
{
    public class TweetMetrics
	{
		public int TotalTweets { get; set; }
		public int Tweet_URL { get; set; }
		public int Tweet_Emoji { get; set; }
		public string Top_Emoji { get; set; }
		public StringBuilder Top_HashTags { get; set; }
		public int Tweet_photoURL { get; set; }
		public string Top_URL_Domains { get; set; }
		public SampledDTOStream SampledStream { get; set; }
	}
}