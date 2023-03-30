using Newtonsoft.Json;


namespace UserProfileService
{
    class UserProfile
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "interests")]
        public string[] Interests { get; set; }
    }
}
