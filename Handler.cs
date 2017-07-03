using System;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using System.Collections.Generic;

namespace StudentIT.Roster.Summary
{
    public class Handler
    {
        List<String> calendars = new List<string> {
            "cs45o1aqp0nc3sff4d4i6q1jms@group.calendar.google.com", // Baillieu
            "1b6sqii4bqnqr1588ba80r8nts@group.calendar.google.com", // BEE
            "opshfv1nkjm6o29rhcvh0458kk@group.calendar.google.com", // ERC
            "uqi13i003btmgjt5gf78qaf5cc@group.calendar.google.com"  // Southbank
        };

        public Response Hello(Request req)
        {
            var service = GcalProvider.MakeService();

            // Define parameters of request.
            foreach (String calendarId in calendars)
            {
                var request = service.Events.List(calendarId: calendarId);
                request.TimeMin = DateTime.Now;
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.MaxResults = 2500;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
                request.TimeMax = DateTime.Now;

                Events events = request.Execute();
                if (events.Items != null)
                {
                    foreach (var eventItem in events.Items)
                    {
                        string when = eventItem.Start.DateTime.ToString();
                        if (String.IsNullOrEmpty(when))
                        {
                            when = eventItem.Start.Date;
                        }
                    }
                }
            }
            
            return new Response("", req);
        }
    }

    public class Response
    {
        public string Message { get; set; }
        public Request Request { get; set; }

        public Response(string message, Request request)
        {
            Message = message;
            Request = request;
        }
    }

    public class Request
    {
        public string Key1 { get; set; }
        public string Key2 { get; set; }
        public string Key3 { get; set; }

        public Request(string key1, string key2, string key3)
        {
            Key1 = key1;
            Key2 = key2;
            Key3 = key3;
        }
    }

}
