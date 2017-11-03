using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;

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

        private GameSession session;
        private MonoBehaviour coroutineObject;
        private Action<NetworkRequestItem> destroyCallback;
        private string wwwError = null;
        private bool pending = false;

        private int maxRetries = 0;
        private int retryCount = 0;

        private bool isCritical = false;

        private bool gotNewSessiodID = false;

        public NetworkRequestItem(MonoBehaviour coroutineObject, Action<NetworkRequestItem> destroyCallback, GameSession session)
        {
            this.coroutineObject = coroutineObject;
            this.destroyCallback = destroyCallback;
            this.session = session;
        }

        public void MakeRequest<Response>(string url, byte[] postData, Dictionary<string, string> headers, bool isJson, Delegates.ServiceCallback<Response> requestCallback, int maxRetries, bool isCritical)
        {
            this.url = url;
            this.headers = headers;
            this.isJson = isJson;
            this.postData = postData;

            this.maxRetries = maxRetries;
            this.isCritical = isCritical;

            UseSession();
            StartCoroutine(Request(requestCallback));
        }

        private void UseSession()
        {
            Debug.Log("UseSession()");
            Debug.Log("headers != null("+ (headers != null).ToString() + ") && session != null(" + (session != null).ToString() + ") && !string.IsNullOrEmpty(session.sessionId)("+ (!string.IsNullOrEmpty(session.sessionId)).ToString()+")");
            if (headers != null && session != null && !string.IsNullOrEmpty(session.sessionId))
            {
                Debug.Log("sid: " + session.sessionId);
                headers[HEADER_COOKIE] = session.sessionId;
                session.pendingSessionRequest = true;
            }
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
                UpdateSessionTs(hasWWWError);

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
                        
                        /*
                        if (RefreshingSessionId(wwwReq.text, wwwReq, requestCallback))
                        {
                            return;
                        }
                        */
                        
                        Debug.Log("Starting to ParseResponseBody");
                        Response responseData = ParseResponseBody<Response>(wwwReq.text);
                        UpdateSessionId(wwwReq, wwwReq.text, responseData);
                        Debug.Log("Response body parsed.");
                        
                        /*
                        if (isJson && !IsValidSessionResponse(wwwReq.text, responseData, requestCallback))
                        {
                            return;
                        }
                        */
                        
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
            // if (session != null && !string.IsNullOrEmpty(session.sessionId)) { queryParameters.Add("sid", session.sessionId); }

            return queryParameters;
        }

        private Response ParseResponseBody<Response>(string response)
        {
            if (string.IsNullOrEmpty(response))
            {
                Debug.LogError("response is null or empty");
                return default(Response);
            }

            Response responseData;
            try
            {
                if (typeof(Response) == typeof(string))
                {
                    Debug.Log("typeof(Response) == typeof(string) is true");
                    responseData = (Response)Convert.ChangeType(response, typeof(Response));
                }
                else
                {
                    if (isJson)
                    {
                        Debug.Log("isJson is true");
                        response = RestServiceBase.FixJson(response);
                        responseData = JsonConvert.DeserializeObject<Response>(response);
                    }
                    else
                    {
                        Debug.Log("isJson is false");
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
                Debug.Log("CAUGHT AN EXCEPTION");
                Debug.LogException(exception);
                responseData = default(Response);
            }

            return responseData;
        }

        private bool RefreshingSessionId<Response>(string responseString, WWW www, Delegates.ServiceCallback<Response> requestCallback)
        {
            Response responseData;
            try
            {
                if (typeof(Response) == typeof(string))
                {
                    Debug.Log("typeof(Response) == typeof(string) is true");
                    responseData = (Response)Convert.ChangeType(responseString, typeof(Response));
                }
                else
                {
                    if (isJson)
                    {
                        Debug.Log("isJson is true");
                        responseString = RestServiceBase.FixJson(responseString);
                        responseData = JsonConvert.DeserializeObject<Response>(responseString);
                    }
                    else
                    {
                        Debug.Log("isJson is false");
                        var xmlSerializer = new XmlSerializer(typeof(Response));
                        using (var textReader = new StringReader(responseString))
                        {
                            responseData = (Response)xmlSerializer.Deserialize(textReader);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.Log("CAUGHT AN EXCEPTIONNNNNNM");
                Debug.LogException(exception);
                string cookieString = www.responseHeaders[COOKIE_HEADER_KEY];
                if (!string.IsNullOrEmpty(cookieString) && cookieString.Contains(COOKIE_PHPSESSION))
                {
                    Debug.Log("yay! we in!");
                    Regex regex = new Regex(COOKIE_PHPSESSION + "=(.*?);");
                    string sessionId = regex.Match(cookieString).Value;
                    sessionId = sessionId.Replace(";", string.Empty);
                    session.sessionId = sessionId;
                    session.requestSuccessReceivedTS = Time.realtimeSinceStartup;
                    headers[HEADER_COOKIE] = session.sessionId;
                    session.pendingSessionRequest = true;
                    StartCoroutine(Request(requestCallback));
                    return true;
                }
            }
            return false;
        }

        private bool IsValidSessionResponse<Response>(string responseString, Response response, Delegates.ServiceCallback<Response> requestCallback)
        {
            DtoBase baseDto = JsonConvert.DeserializeObject<DtoBase>(responseString);
            KillRequest();
            if (baseDto == null || baseDto.reauth || (baseDto.retry && isRetrysExceeded && isCritical))
            {
                Delete();
                return false;
            }
            else if (baseDto.retry && isRetrysExceeded && !isCritical)
            {
                requestCallback(false, null, response);
                Delete();
                return false;
            }
            else if (baseDto.retry)
            {
                if (!string.IsNullOrEmpty(baseDto.newHexaclash))
                {
                    MainController.settingsService.hexaClash = baseDto.newHexaclash;
                }
                retryCount++;

                Debug.Log("RETRY " + retryCount + " " + url);

                StartCoroutine(Request(requestCallback));
                return false;
            }
            return true;
        }

        private void UpdateSessionTs(bool hasWWWError)
        {
            if (!hasWWWError)
            {
                if (session != null && session.pendingSessionRequest)
                {
                    session.requestSuccessReceivedTS = Time.realtimeSinceStartup;
                }
            }
            if (session != null)
            {
                session.pendingSessionRequest = false;
            }
        }
        private void UpdateSessionId<Response>(WWW www, string responseString, Response response)
        {
            Debug.Log("UpdateSessionId");
            var sessionId = "";
            /* PlayerService.LoginRequestObject lro = response as PlayerService.LoginRequestObject;
            if (lro != null)
            {
                Debug.Log("lro != null");
                sessionId = lro.sid;
            }
            */
            sessionId = RetrieveSessionId(www);
            Debug.Log("Received sessionId: " + sessionId);
            if (!string.IsNullOrEmpty(sessionId))
            {
                session.sessionId = sessionId;
                session.requestSuccessReceivedTS = Time.realtimeSinceStartup;
                gotNewSessiodID = true;
            }
        }
        private string RetrieveSessionId(WWW www)
        {
            Debug.Log("Retrieve session ID");
            foreach (KeyValuePair<string, string> header in www.responseHeaders)
            {
                Debug.Log(header.Key + ":" + header.Value);
            }
            string cookieString = www.responseHeaders[COOKIE_HEADER_KEY];
            Debug.Log("We got the cookieString!!! (" +cookieString+ ")");
            if (string.IsNullOrEmpty(cookieString) || !cookieString.Contains(COOKIE_PHPSESSION))
            {
                Debug.LogError("cookieString is empty(" + (string.IsNullOrEmpty(cookieString)) + ") or does not contain COOKIE_PHPSESSION("+ !cookieString.Contains(COOKIE_PHPSESSION) + ")");
                return null;
            }

            Regex regex = new Regex(COOKIE_PHPSESSION + "=(.*?);");
            string sessionId = regex.Match(cookieString).Value;
            sessionId = sessionId.Replace(";", string.Empty);

            return sessionId;
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