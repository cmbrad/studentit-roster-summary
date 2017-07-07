using System;
using System.Collections.Generic;

using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace StudentIT.Roster.Summary
{
    class EmailReport
    {
        public List<String> RecipientEmails { get; private set; }

        public EmailReport()
        {
            RecipientEmails = RecipientsFromEnvironment();
        }

        public void Send()
        {
            var (address, port, username, password) = EmailServerDetailsFromEnvironment();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Student IT and eLearning Support", "student-it@unimelb.edu.au"));

            foreach (String emailAddress in RecipientEmails)
            {
                message.To.Add(new MailboxAddress(emailAddress));
            }
            message.Subject = "";

            message.Body = new TextPart("plain") { Text = "Test" };

            using (var client = new SmtpClient())
            {
                // For demo-purposes, accept all SSL certificates (in case the server supports STARTTLS)
                // client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                // Timeout connect to 10 seconds
                client.Timeout = 10000;

                client.Connect(address, port);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                // Note: only needed if the SMTP server requires authentication
                client.Authenticate(username, password);

                Console.WriteLine("Sending email");
                client.Send(message);
                client.Disconnect(true);
            }
        }

        private List<String> RecipientsFromEnvironment()
        {
            List<String> emailAddresses = new List<string>();

            for (int i = 0; i < 100; i++)
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

        private (string, int, string, string) EmailServerDetailsFromEnvironment()
        {
            var address = Environment.GetEnvironmentVariable("SMTP_ADDRESS");
            var port = Int32.Parse(Environment.GetEnvironmentVariable("SMTP_PORT"));
            var username = Environment.GetEnvironmentVariable("SMTP_USERNAME");
            var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

            return (address, port, username, password);
        }
    }
}
