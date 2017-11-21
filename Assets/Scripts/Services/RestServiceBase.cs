using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace Com.Hypester.DM3
{
    public class RestServiceBase : MonoBehaviour
    {
        protected const string CONTENT_TYPE_JSON = "application/json";
        protected const string CONTENT_TYPE_XML = "text/xml";

        protected const string METHOD_GET = "GET";
        protected const string METHOD_POST = "POST";

        protected const string HEADER_METHOD = "METHOD";
        protected const string HEADER_SET_COOKIE = "SET-COOKIE";
        protected const string HEADER_CONTENT_TYPE = "Content-Type";

        protected const string COOKIE_PHPSESSION = "PHPSESSID";

        protected const string ERROR_BODY = "[]";

        protected string contentType = CONTENT_TYPE_JSON;

        protected IEnumerator Request<Response>(string url, Action<Response> callback)
        {
            string requestUrl = Configuration.apiUrl + url;
            WWW request = new WWW(requestUrl);

            yield return request;

            if (!string.IsNullOrEmpty(request.error))
            {
                print("Error pinging: " + request.error);
                callback(default(Response));
            }
            else
            {
                Response response = JsonConvert.DeserializeObject<Response>(request.text);
                callback(response);
            }
        }

        protected void AsyncServerRequest<Response>(string path, IDictionary<string, object> data, Delegates.ServiceCallback<Response> requestCallback, int maxRetrys, bool isCritical)
        {
            var headers = new Dictionary<string, string>();
            headers[HEADER_CONTENT_TYPE] = contentType;
            headers[HEADER_METHOD] = METHOD_POST;

            StringBuilder jsonStringBuilder = new StringBuilder();
            if (data != null)
            {
                string json = JsonConvert.SerializeObject(data, Formatting.None);
                jsonStringBuilder.Append(json);
            } else { jsonStringBuilder.Append("{}"); }

            string dataString = jsonStringBuilder.ToString();
            string url = (Configuration.apiUrl + path).Replace(@"\", @"/");
            Debug.Log("dataString: " + dataString);
            Debug.Log("url: " + url);

            string postDataString = EncodeBody(dataString);

			Debug.Log ("Encoded dataString: " + postDataString);

            MainController.ServiceNetwork.MakeRequest(url, Encoding.UTF8.GetBytes(postDataString), headers, contentType == CONTENT_TYPE_JSON, requestCallback, maxRetrys, isCritical);
            headers.Clear();

            Debug.LogError("data: " + dataString);
        }

        /// <summary>
        /// Fixes the json. 
        /// </summary>
        /// <param name="rawJson">The raw json.</param>
        /// <returns></returns>
        public static string FixJson(string rawJson)
        {
            Debug.Log("In FixJson()");
            Debug.Log("rawJson: " + rawJson);
            if (string.IsNullOrEmpty(rawJson))
            {
                return rawJson;
            }

            var rawJsonStringBuilder = new StringBuilder(rawJson);
            rawJsonStringBuilder.Replace("\"[", "[");
            rawJsonStringBuilder.Replace("]\"", "]");
            rawJsonStringBuilder.Replace("[]", "null");

            Debug.Log("result: " + rawJsonStringBuilder.ToString());

            return rawJsonStringBuilder.ToString();
        }

        /// <summary>
        /// Encodes the body.
        /// </summary>
        /// <returns></returns>
        public static string EncodeBody(string dataString)
        {

            if (MainController.settingsService == null)
            {
                Debug.LogError("settings == null");
                return EncodeData(dataString);
            }
            if (string.IsNullOrEmpty(dataString))
            {
                Debug.LogError("dataString empty or null");
                dataString = "{}";
            }

            Debug.LogError("dataString: " + dataString);

            string checksum = (dataString + MainController.settingsService.hexaClash).GetMd5();
            string requestString = "";

            Debug.LogError("checksum: " + checksum);

            IDictionary dataDict = JsonConvert.DeserializeObject<IDictionary>(dataString);
            dataDict.Add("chk", checksum);

            requestString = JsonConvert.SerializeObject(dataDict);


            //DebugLog.Log("DATA + mahjong: " + dataString + MahjongGame.gameInfo.mahjong + "\n(DATA + mahjong).MD5: " + checksum + "\nDATA: " + requestString);

            return EncodeData(requestString);
        }

        private static string EncodeData(string data)
        {
            byte[] bodyStringBytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bodyStringBytes);
        }
    }
}