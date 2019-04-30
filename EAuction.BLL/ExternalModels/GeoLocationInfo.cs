using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace EAuction.BLL.ExternalModels
{
    public class GeoLocationInfo
    {
        public string ip { get; set; }
        public string country_name { get; set; }
        public string city { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }


        public static GeoLocationInfo GetGeolocationInfo()
        {
            WebClient webClient = new WebClient();
            string externalIp = webClient
                .DownloadString("http://icanhazip.com");

            string ipStackAccessKey = "cb6a8892805bdd4727b7669b1f584318";
            string ipStackUrl = $"api.ipstack.com/{externalIp}?access_key={ipStackAccessKey}";
            ipStackUrl = "http://" + ipStackUrl;

            string ipInfoAsJson = webClient.DownloadString(ipStackUrl);

            GeoLocationInfo geoLocationInfo = JsonConvert.DeserializeObject<GeoLocationInfo>(ipInfoAsJson);
            return geoLocationInfo;
        }
    }
}