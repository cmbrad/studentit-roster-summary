using System;
using System.IO;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Json;

namespace StudentIT.Roster.Summary
{
    public class Handler
    {
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "StudentIT Roster Summary";

        public string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public UserCredential CreateCredentials()
        {
            UserCredential credential;

            String encoded = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRETS");
            String decoded = Base64Decode(encoded);

            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(decoded));

            var flow = new GoogleAuthorizationCodeFlow(
                new GoogleAuthorizationCodeFlow.Initializer
                    {
                        ClientSecrets = GoogleClientSecrets.Load(stream).Secrets,
                        Scopes = Scopes,
                        DataStore = new EnvironmentDataStore("GOOGLE_AUTH_TOKEN")
                    });

            credential = new AuthorizationCodeInstalledApp(
                flow,
                new PromptCodeReceiver()).AuthorizeAsync("user", CancellationToken.None).Result;

            return credential;
       }

       public Response Hello(Request req)
       {
          UserCredential credential = CreateCredentials();

          var service = new CalendarService(new BaseClientService.Initializer()
              {
                  ApplicationName = "StudentIT Roster Summary",
                  HttpClientInitializer = credential,
              });
           // Define parameters of request.
           EventsResource.ListRequest request = service.Events.List("primary");
           request.TimeMin = DateTime.Now;
           request.ShowDeleted = false;
           request.SingleEvents = true;
           request.MaxResults = 10;
           request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

           // List events.
           String res = "";
           Events events = request.Execute();
           Console.WriteLine("Upcoming events:");
           if (events.Items != null && events.Items.Count > 0)
           {
               foreach (var eventItem in events.Items)
               {
                   string when = eventItem.Start.DateTime.ToString();
                   if (String.IsNullOrEmpty(when))
                   {
                       when = eventItem.Start.Date;
                   }
                   res += String.Format("{0} ({1})", eventItem.Summary, when);
                   Console.WriteLine("{0} ({1})", eventItem.Summary, when);
               }
           }
           else
           {
               Console.WriteLine("No upcoming events found.");
           }
           return new Response(res, req);
       }
    }

    public class Roster
    {
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "StudentIT Roster Summary";

        public void CreateCredentials()
        {
            UserCredential credential;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                String credPath = "credentials.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }
        }

        public void Fetch(String startDate, String endDate)
        {
            var service = new CalendarService(new BaseClientService.Initializer()
                {
                    ApplicationName = "StudentIT Roster Summary",
                    ApiKey="",
                });
        }
    }

    public class Response
    {
      public string Message {get; set;}
      public Request Request {get; set;}

      public Response(string message, Request request){
        Message = message;
        Request = request;
      }
    }

    public class Request
    {
      public string Key1 {get; set;}
      public string Key2 {get; set;}
      public string Key3 {get; set;}

      public Request(string key1, string key2, string key3){
        Key1 = key1;
        Key2 = key2;
        Key3 = key3;
      }
    }

    public class EnvironmentDataStore : IDataStore
    {
        readonly string environmentKey;
        public string EnvironmentKey { get { return environmentKey; } }

        public EnvironmentDataStore(string environmentKey)
        {
            this.environmentKey = environmentKey;
        }

        public string Base64Encode(string plainText) {
            if (plainText == null)
                return "";
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var serialized = NewtonsoftJsonSerializer.Instance.Serialize(value);
            var encoded = Base64Encode(serialized);
            Environment.SetEnvironmentVariable(this.environmentKey, encoded);

            return Task.Delay(0);
        }

        public Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            Environment.SetEnvironmentVariable(this.environmentKey, "");

            return Task.Delay(0);
        }

        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            var encoded = Environment.GetEnvironmentVariable(this.environmentKey);

            if (encoded != null)
            {
                try
                {
                    var decoded = Base64Decode(encoded);
                    tcs.SetResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(decoded));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
            else
            {
                tcs.SetResult(default(T));
            }
            return tcs.Task;
        }

        public Task ClearAsync()
        {
            Environment.SetEnvironmentVariable(this.environmentKey, "");

            return Task.Delay(0);
        }
    }
}
