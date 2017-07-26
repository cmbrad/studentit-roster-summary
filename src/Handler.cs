using System;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using System.Collections.Generic;
using System.Globalization;

namespace StudentIT.Roster.Summary
{
    public class Handler
    {
        private readonly List<string> _calendars = new List<string> {
            "cs45o1aqp0nc3sff4d4i6q1jms@group.calendar.google.com", // Baillieu
            "1b6sqii4bqnqr1588ba80r8nts@group.calendar.google.com", // BEE
            "opshfv1nkjm6o29rhcvh0458kk@group.calendar.google.com", // ERC
            "uqi13i003btmgjt5gf78qaf5cc@group.calendar.google.com"  // Southbank
        };

        private DateTime GetDateFromEnvOrDefault(string envVar, Func<DateTime> getDefault)
        {
            string dateString = Environment.GetEnvironmentVariable(envVar);
            return DateTime.TryParse(dateString, out DateTime date) ? date : getDefault();
        }

        private (DateTime, DateTime, bool) GetRosterBounds()
        {
            var periodStart = GetDateFromEnvOrDefault("ROSTER_START_DATE", GetLastRosterStartDate);
            var periodEnd = periodStart.AddDays(14); // Roster period is 14 days
            // Run if within a week of roster end, or if a custom date has been supplied
            var shouldRun = Environment.GetEnvironmentVariable("ROSTER_START_DATE") != null || 
                DateTime.Now > periodEnd.AddDays(-7);

            return (periodStart, periodEnd, shouldRun);
        }

        private DateTime GetLastRosterStartDate()
        {
            DateTime rosterEpoch = new DateTime(2017, 7, 1, 0, 0, 0);
            DateTime startDate = rosterEpoch;
            while (startDate.AddDays(14) < DateTime.Now)
            {
                startDate.AddDays(14);
            }
            return startDate;
        }

        public void MakeSummary()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            var (periodStart, periodEnd, shouldRun) = GetRosterBounds();

            if (!shouldRun)
            {
                // Workaround the fact that you cannot schedule a lambda to run every 2 weeks
                Console.WriteLine("Cancelling summary as it's not the end of a pay week");
                return;
            }

            Console.WriteLine($"Loading summary for period {periodStart} - {periodEnd}");

            RosterSummary rosterSummary = new RosterSummary(periodStart, periodEnd);

            var service = GcalProvider.MakeService();
            var internProvider = new InternProvider();

            // Define parameters of request.
            foreach (var calendarId in _calendars)
            {
                var request = service.Events.List(calendarId);
                request.TimeMin = periodStart;
                request.TimeMax = periodEnd;
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.MaxResults = 2500;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                Events events = request.Execute();
                if (events.Items != null)
                {
                    Console.WriteLine($"Got {events.Items.Count} events from {calendarId}");

                    foreach (var eventItem in events.Items)
                    {
                        // Location will be a string of either BEE/Baillieu/ERC
                        // It's irrelevant for hours calculation but helpful for logging
                        var startTime = eventItem.Start.DateTime;
                        var endTime = eventItem.End.DateTime;
                        var location = eventItem.Location;

                        if (eventItem.Status == "cancelled")
                        {
                            // Ignore cancelled events since it means the shift was not actully worked
                            Console.WriteLine($"Skipping cancelled event {startTime} - {endTime} @ {location}");
                            continue;
                        }
                        if (eventItem.Attendees.Count == 0)
                        {
                            // This event is probably a valid shift, but we have no way of knowing
                            // Who owns it. Log it and continue
                            Console.WriteLine($"Skipping event with 0 attendees {startTime} - {endTime} @ {location}");
                            continue;
                        }
                        if (startTime < rosterSummary.StartDate || endTime > rosterSummary.EndDate)
                        {
                            Console.WriteLine($"Skipping event which is outside of roster timeframe {startTime} - {endTime} @ {location}");
                        }

                        var internEmail = eventItem.Attendees[0].Email;
                        if (eventItem.Attendees.Count > 1)
                        {
                            // People sometimes add personal and work emails to shifts. This shouldn't
                            // be a problem, but good to log it just in case
                            Console.WriteLine($"Found event with multiple attendees {startTime}-{endTime} @ {location}. Using {internEmail}");
                        }

                        var internName = internProvider.NameFromEmail(internEmail);

                        var employee = EmployeeByName(internName ?? internEmail, rosterSummary);
                        var hours = (endTime - startTime).Value.TotalHours;
                        DateTime curDate = rosterSummary.StartDate;
                        for (var i = 0; i < rosterSummary.Days; i++)
                        {
                            if (curDate.Date == endTime.Value.Date)
                            {
                                employee.Shifts[i] += hours;
                                break;
                            }
                            curDate = curDate.AddDays(1);
                        }
                    }
                } else
                {
                    Console.WriteLine($"Got no events from {calendarId}. Is the selected time period correct? {request.TimeMin}-{request.TimeMax}");
                }
            }

            var report = new EmailReport();
            report.Send(rosterSummary);
        }

        private Employee EmployeeByName(string name, RosterSummary rosterSummary)
        {
            Employee employeeWithName = null;

            foreach (Employee employee in rosterSummary.Employees)
            {
                if (employee.Name == name)
                {
                    employeeWithName = employee;
                }
            }

            if (employeeWithName == null)
            {
                employeeWithName = new Employee(name, rosterSummary.Days);
                rosterSummary.Employees.Add(employeeWithName);
            }

            return employeeWithName;
        }
    }
}
