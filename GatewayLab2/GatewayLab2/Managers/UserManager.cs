using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GatewayLab2.Managers
{
    public class UserManager
    {
        public static User GetUser(string url, string authorization)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.UserAgent = "gateway 1.0.0";
                request.Headers.Add("Authorization", authorization);

                Stream requestStream = request.GetRequestStream();
                requestStream.Close();

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);

                string responseFromServer = reader.ReadToEnd();


                reader.Close();
                dataStream.Close();

                var user = JsonSerializer.Deserialize<User>(responseFromServer);
                user.Nickname = user.Nickname[0].ToString().ToUpper() + user.Nickname.Substring(1);
                response.Close();

                return user;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public class User
    {
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; }
    }
}
