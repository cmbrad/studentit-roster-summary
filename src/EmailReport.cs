﻿using System;
using System.Collections.Generic;
using System.IO;
using Amazon.S3.Model;
using DotLiquid;
using MimeKit;
using MailKit.Net.Smtp;

namespace StudentIT.Roster.Summary
{
    internal class EmailReport
    {
        public List<string> RecipientEmails { get; }

        public EmailReport()
        {
            RecipientEmails = RecipientsFromEnvironment();
        }

        public void Send(RosterSummary roster)
        {
            var (address, port, username, password, templateFilename) = EmailServerDetailsFromEnvironment();
            Console.WriteLine($"Using template file {templateFilename}");
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Student IT and eLearning Support", "student-it@unimelb.edu.au"));

            foreach (var emailAddress in RecipientEmails)
            {
                message.To.Add(new MailboxAddress(emailAddress));
            }
            message.Subject = $"Shift Summary ({roster.StartDate.ToString("yyyy-MM-dd")} - {roster.EndDate.ToString("yyyy-MM-dd")})";


            var tableHeader = new List<string>();
            DateTime curDate = roster.StartDate;
            while (curDate < roster.EndDate)
            {
                tableHeader.Add(curDate.ToString("dd-MM"));
                curDate = curDate.AddDays(1);
            }
            tableHeader.Add("Total");

            roster.Employees.Sort((a, b) => a.Name.CompareTo(b.Name));
            var model = new
            {
                Employees = roster.Employees,
                Header = tableHeader
            };
            Template.RegisterSafeType(typeof(Employee), new[] {"Name", "TotalHours", "Shifts"});
            Template.RegisterFilter(typeof(HoursFilter));
            Template.RegisterFilter(typeof(ShiftStyleFilter));
            Template template = Template.Parse(File.ReadAllText(Path.Combine("templates", templateFilename)));
            var bodyText = template.Render(Hash.FromAnonymousObject(model));

            Console.WriteLine(bodyText);
            message.Body = new TextPart("html") { Text = bodyText };

            using (var client = new SmtpClient())
            {
                Console.WriteLine("Sending email");

                client.Timeout = 10000;
                client.Connect(address, port);

                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(username, password);

                client.Send(message);
                client.Disconnect(true);
            }
        }

        private List<string> RecipientsFromEnvironment()
        {
            var emailAddresses = new List<string>();

            for (var i = 0; i < 100; i++)
            {
                var envVar = $"RECIPIENT_EMAIL_{i}";
                var curEmail = Environment.GetEnvironmentVariable(envVar);
                if (curEmail != null)
                {
                    Console.WriteLine($"{envVar}={curEmail}");
                    emailAddresses.Add(curEmail);
                    break;
                }
            }

            Console.WriteLine($"Found {emailAddresses.Count} email addresses");

            return emailAddresses;
        }

        private (string, int, string, string, string) EmailServerDetailsFromEnvironment()
        {
            var address = Environment.GetEnvironmentVariable("SMTP_ADDRESS");
            var port = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT"));
            var username = Environment.GetEnvironmentVariable("SMTP_USERNAME");
            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
            var templateFilename = Environment.GetEnvironmentVariable("EMAIL_TEMPLATE");

            return (address, port, username, password, templateFilename);
        }
    }

    internal static class HoursFilter
    {
        public static string Hours(double input)
        {
            return String.Format("{0:F}", input);
        }
    }

    internal static class ShiftStyleFilter
    {
        public static string ShiftStyle(double input)
        {
            return input == 0.0 ? "grey" : "white";
        }
    }
}
