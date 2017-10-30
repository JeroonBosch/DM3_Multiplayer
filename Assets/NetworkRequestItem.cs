using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;

namespace Com.Hypester.DM3
{
    public class NetworkRequestItem
    {
        private const string COOKIE_HEADER_KEY = "SET-COOKIE";
        private const string CONTENT_TYPE_KEY = "CONTENT-TYPE";

        protected const string ERROR_BODY = "[]";

        // "text/html; charset=UTF-8"
        private const string COOKIE_PHPSESSION = "PHPSESSID";
        protected const string HEADER_COOKIE = "COOKIE";

        private WWW wwwReq = null;

        public string url { get; private set; }
        public Dictionary<string, string> headers { get; private set; }
        public bool isJson { get; private set; }

        private byte[] postData;

        private MonoBehaviour coroutineObject;
        private Action<NetworkRequestItem> destroyCallback;
        private string wwwError = null;
        private bool pending = false;

        private int maxRetries = 0;
        private int retryCount = 0;

        private bool isCritical = false;

        public NetworkRequestItem(MonoBehaviour coroutineObject, Action<NetworkRequestItem> destroyCallback)
        {
            this.coroutineObject = coroutineObject;
            this.destroyCallback = destroyCallback;
        }

        public void MakeRequest<Response>(string url, byte[] postData, Dictionary<string, string> headers, bool isJson, Delegates.ServiceCallback<Response> requestCallback, int maxRetries, bool isCritical)
        {
            this.url = url;
            this.headers = headers;
            this.isJson = isJson;
            this.postData = postData;

            this.maxRetries = maxRetries;
            this.isCritical = isCritical;
            
            StartCoroutine(Request(requestCallback));
        }

        private IEnumerator Request<Response>(Delegates.ServiceCallback<Response> requestCallback)
        {
            pending = true;

            string reqUrl = BuildUrl();

            Debug.Log("Final URL");
            Debug.Log(reqUrl);

            wwwReq = new WWW(reqUrl, postData, headers);

            yield return wwwReq;

            pending = false;
            OnRequestCompleted(requestCallback);
        }

        private void OnRequestCompleted<Response>(Delegates.ServiceCallback<Response> requestCallback)
        {
            Debug.Log("OnRequestCompleted");
            if (wwwReq != null)
            {
                Debug.Log("wwwReq != null");
                wwwError = wwwReq.error;

                bool hasWWWError = !string.IsNullOrEmpty(wwwError);

                if (postData == null && headers == null)
                {
                    Debug.Log("It was a simple request");
                    // It was simple request
                    if (requestCallback != null)
                    {
                        requestCallback(!hasWWWError, wwwError);
                    }
                }
                else
                {
                    if (hasWWWError)
                    {
                        KillRequest();
                        if (!isRetrysExceeded)
                        {
                            retryCount++;

                            Debug.Log("RETRY " + retryCount + " " + url);

                            StartCoroutine(Request(requestCallback));
                            return;
                        }
                        if (isCritical)
                        {
                            Delete();
                            return;
                        }
                        if (requestCallback != null)
                        {
                            requestCallback(false, wwwError);
                        }
                    }
                    else
                    {
                        Debug.Log("All good");
                        Debug.Log(wwwReq.text);
                        Response responseData = ParseResponseBody<Response>(wwwReq.text);
                        Debug.Log(responseData);
                        if (requestCallback != null)
                        {
                            requestCallback(true, null, responseData);
                        }
                    }
                }
                KillRequest();
            }
            Delete();
        }

        
        protected string BuildUrl()
        {
            var dataParameters = GetQueryParameters();

            var queryStringBuilder = new StringBuilder(url);
            queryStringBuilder.Append("?");

            foreach (var queryParameter in dataParameters)
            {
                queryStringBuilder.Append(string.Format("{0}={1}&", queryParameter.Key, WWW.EscapeURL(queryParameter.Value)));
            }

            string resultUrl = queryStringBuilder.ToString();
            resultUrl = resultUrl.Trim('&');

            return resultUrl;
        }

        protected IDictionary<string, string> GetQueryParameters()
        {
            Dictionary<string, string> queryParameters = new Dictionary<string, string>();
            queryParameters.Add("rnd", UnityEngine.Random.Range(0, int.MaxValue).ToString(CultureInfo.InvariantCulture));
            // queryParameters.Add("v", Configuration.buildVersion + "." + Configuration.buildSubVersion);
            queryParameters.Add("counter", (retryCount + 1).ToString());

            return queryParameters;
        }

        private Response ParseResponseBody<Response>(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                return default(Response);
            }

            Response responseData;
            try
            {
                if (typeof(Response) == typeof(string))
                {
                    responseData = (Response)Convert.ChangeType(response, typeof(Response));
                }
                else
                {
                    if (isJson)
                    {
                        response = RestServiceBase.FixJson(response);
                        responseData = JsonConvert.DeserializeObject<Response>(response);
                    }
                    else
                    {
                        var xmlSerializer = new XmlSerializer(typeof(Response));
                        using (var textReader = new StringReader(response))
                        {
                            responseData = (Response)xmlSerializer.Deserialize(textReader);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                responseData = default(Response);
            }

            return responseData;
        }
        public void KillRequest()
        {
            if (wwwReq != null)
            {
                wwwReq.Dispose();
                wwwReq = null;
            }
        }

        private void Delete()
        {
            if (destroyCallback != null)
            {
                destroyCallback(this);
            }
        }

        private void StartCoroutine(IEnumerator routine)
        {
            coroutineObject.StartCoroutine(routine);
        }

        public bool isConnected
        {
            get
            {
                return string.IsNullOrEmpty(wwwError);
            }
        }

        public bool isPending
        {
            get
            {
                return pending;
            }
        }

        public bool isSimpleRequest
        {
            get
            {
                return postData == null && headers == null;
            }
        }

        private bool isRetrysExceeded
        {
            get
            {
                return retryCount >= maxRetries;
            }
        }
    }
}