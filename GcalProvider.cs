using System;
using System.IO;
using System.Text;
using System.Threading;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;

namespace StudentIT.Roster.Summary
{
    public static class GcalProvider
    {
        // Configuration
        static string[] Scopes = { CalendarService.Scope.CalendarReadonly };
        static string ApplicationName = "StudentIT Roster Summary";

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private static UserCredential CreateCredentials()
        {
            UserCredential credential;

            String encoded = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRETS");
            String decoded = Base64Decode(encoded);

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(decoded));

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

        public static CalendarService MakeService()
        {
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                ApplicationName = ApplicationName,
                HttpClientInitializer = CreateCredentials(),
            });

            return service;
        }
    }
}
