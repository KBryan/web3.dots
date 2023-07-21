using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Web3Unity.Scripts.Library.Ethers.InternalEvents
{
    public class DataHog
    {
        private readonly string _baseUrl = "https://http-intake.logs.datadoghq.com/api/v2/logs";
        private readonly string _apiKey;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public static DataHog Client = new DataHog("");

        public DataHog(string apiKey, string baseUrl = null)
        {
            _apiKey = apiKey;

            if (baseUrl != null)
            {
                _baseUrl = baseUrl;
            }
        }

        public async void Capture(string eventName, Dictionary<string, object> properties)
        {
            properties["$lib"] = "web3.unity-datadog";
            properties["$lib_version"] = "2.0.0-beta"; // TODO: get version dynamically

            var userUuid = PlayerPrefs.GetString("ph_user_uuid", null);
            if (string.IsNullOrEmpty(userUuid))
            {
                userUuid = Guid.NewGuid().ToString();
                PlayerPrefs.SetString("ph_user_uuid", userUuid);
                PlayerPrefs.Save();
            }

            try
            {
                var json = JsonConvert.SerializeObject(new DataDogEvent
                {
                    DD_SOURCE = _apiKey,
                    Event = eventName,
                    DDTAGS = properties,
                    PROJECT_ID = userUuid
                }, _jsonSerializerSettings);

                var req = new UnityWebRequest(_baseUrl + "/batch/", "POST");
                req.uploadHandler = new UploadHandlerRaw(new UTF8Encoding().GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                await req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    req.Dispose();
                    throw new Exception(req.error);
                }

                req.Dispose();
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
    }
}