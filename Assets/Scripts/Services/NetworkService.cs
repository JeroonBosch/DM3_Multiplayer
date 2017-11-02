using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Hypester.DM3
{
    public class NetworkService : MonoBehaviour
    {
        private GameSession session;
        private List<NetworkRequestItem> pendingRequestItems = new List<NetworkRequestItem>();

        public void Init()
        {
            session = new GameSession();
        }

        public void MakeRequest<Response>(string url, byte[] data, Dictionary<string, string> headers, bool isJson, Delegates.ServiceCallback<Response> requestCallback, int maxRetrys, bool isCritical)
        {
            if (HasPendingUrl(url))
            {
                Debug.LogWarning("Request: " + url + " is already pending!");
            }

            NetworkRequestItem reqItem = new NetworkRequestItem(this, OnRequestCompleted, session);
            pendingRequestItems.Add(reqItem);
            reqItem.MakeRequest(url, data, headers, isJson, requestCallback, maxRetrys, isCritical);
        }

        private void OnRequestCompleted(NetworkRequestItem reqItem)
        {
            pendingRequestItems.Remove(reqItem);
        }

        private bool HasPendingUrl(string url)
        {
            for (int i = 0; i < pendingRequestItems.Count; i++)
            {
                if (pendingRequestItems[i].url == url)
                {
                    return true;
                }
            }
            return false;
        }

        private void KillRequests()
        {
            for (int i = 0; i < pendingRequestItems.Count; i++)
            {
                pendingRequestItems[i].KillRequest();
            }
            pendingRequestItems.Clear();
        }

        private void ClearSession()
        {
            KillRequests();
            session.sessionId = null;
        }

        public static void DestroySession()
        {
            if (MainController.ServiceNetwork != null)
            {
                MainController.ServiceNetwork.ClearSession();
            }
        }
    }
}