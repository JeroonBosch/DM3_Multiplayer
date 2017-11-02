using System.Collections.Generic;
using Newtonsoft.Json;

[JsonObject]
public class DtoBase
{
    [JsonProperty("error")]
    public Dictionary<string, string> error { get; set; }

    [JsonIgnore]
    public bool isSuccess { get { return this.error == null || this.error.Count == 0; } }

    [JsonIgnore]
    public string errorMessage
    {
        get
        {
            if (!isSuccess && error.ContainsKey("message"))
            {
                return error["message"];
            }
            return null;
        }
    }

    [JsonProperty("retry")]
    public bool retry { get; set; }

    [JsonProperty("new_hexaclash")]
    public string newHexaclash { get; set; }

    [JsonProperty("verified")]
    public bool verified { get; set; }

    [JsonProperty("reauth")]
    public bool reauth { get; set; }

    [JsonProperty("sid")]
    public string sid { get; set; }
}

