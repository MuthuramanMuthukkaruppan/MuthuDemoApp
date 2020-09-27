
using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TwitterDemoAPI;
using TwitterDemoAPI.Models;
using models = TwitterDemoAPI.Models.SampleStream;
using dto = TwitterDemoAPI.DTO.SampleStream;
using TwitterDemoAPI.Utils;
using TwitterDemoAPI.DTO.SampleStream;
using TwitterDemoAPI.Models.SampleStream;
using TwitterDemoAPI.Client;

namespace TwitterDemoAPI.Service
{
    public class SampledStreamService
    {
        private OAuthInfo _oAuthInfo;
        private IMapper _iMapper;

        public event EventHandler DataReceivedEvent;
        public class DataReceivedEventArgs : EventArgs
        {
            public models.SampledStream StreamDataResponse { get; set; }
        }
        protected void OnDataReceivedEvent(DataReceivedEventArgs dataReceivedEventArgs)
        {
            if (DataReceivedEvent == null)
                return;
            DataReceivedEvent(this, dataReceivedEventArgs);
        }

        public SampledStreamService(OAuthInfo oAuth)
        {
            _oAuthInfo = oAuth;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<dto.SampledDTOStream, models.SampledStream>();
                cfg.CreateMap<dto.Annotation, models.Annotation>();
                cfg.CreateMap<dto.ContextAnnotation, models.ContextAnnotation>();
                cfg.CreateMap<dto.Data, models.Data>();
                cfg.CreateMap<dto.Domain, models.Domain>();
                cfg.CreateMap<dto.Entities, models.Entities>();
                cfg.CreateMap<dto.Entity, models.Entity>();
                cfg.CreateMap<dto.ReferencedTweet, models.ReferencedTweet>();
                cfg.CreateMap<dto.Stats, models.Stats>();
                cfg.CreateMap<dto.Url, models.Url>();
            });

            _iMapper = config.CreateMapper();
        }

        public void StartStream(string address, int maxTweets, int maxConnectionAttempts)
        {
            SampledStreamClient streamClient = new SampledStreamClient(_oAuthInfo.ConsumerKey, 
                _oAuthInfo.ConsumerSecret, _oAuthInfo.BearerToken);

            streamClient.StreamDataReceivedEvent += StreamClient_StreamDataReceivedEvent;

            streamClient.StartStream(address, maxTweets, maxConnectionAttempts);
        }

        private void StreamClient_StreamDataReceivedEvent(object sender, EventArgs e)
        {
            // convert to dto and model
            SampledStreamClient.TweetReceivedEventArgs eventArgs = e as SampledStreamClient.TweetReceivedEventArgs;
            SampledDTOStream resultsDTO = JsonConvert.DeserializeObject<SampledDTOStream>(eventArgs.StreamDataResponse);
            SampledStream model = _iMapper.Map<SampledDTOStream, SampledStream>(resultsDTO);

            // raise event with Model
            OnDataReceivedEvent(new DataReceivedEventArgs { StreamDataResponse = model });
        }
    }
}
