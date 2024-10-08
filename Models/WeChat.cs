using Newtonsoft.Json;

public class WeChatTokenResponse
{
    [JsonProperty("access_token")]
    public string? AccessToken { get; set; }

    [JsonProperty("openid")]
    public string? OpenId { get; set; }
}

public class WeChatUserInfo
{
    [JsonProperty("openid")]
    public string? OpenId { get; set; }

    [JsonProperty("nickname")]
    public string? NickName { get; set; }

    [JsonProperty("sex")]
    public int Sex { get; set; }

    [JsonProperty("province")]
    public string? Province { get; set; }

    [JsonProperty("city")]
    public string? City { get; set; }

    [JsonProperty("country")]
    public string? Country { get; set; }

    [JsonProperty("headimgurl")]
    public string? HeadImgUrl { get; set; }

    [JsonProperty("privilege")]
    public List<string>? Privilege { get; set; }

    [JsonProperty("unionid")]
    public string? UnionId { get; set; }
}
