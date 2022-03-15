using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using Microsoft.Extensions.Configuration;

namespace GMailOauthImap
{
    class Program
    {
        static void Main(string[] args)
        {
            DoWork().GetAwaiter().GetResult();
        }

        private static async Task DoWork()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = config.GetSection("AppSettings")["client_id"],
                    ClientSecret = config.GetSection("AppSettings")["client_secret"]
                },
                new[] { "email", "profile", "https://mail.google.com/" },
                "user",
                CancellationToken.None,
                new NullDataStore()
            );

            var jwtPayload = GoogleJsonWebSignature.ValidateAsync(credential.Token.IdToken).Result;
            var username = jwtPayload.Email;
            
            ReceiveMailByOauth(username, credential.Token.AccessToken);
        }
        
        // Generate an unqiue email file name based on date time
        static string _generateFileName(int sequence)
        {
            DateTime currentDateTime = DateTime.Now;
            return string.Format("{0}-{1:000}-{2:000}.eml",
                currentDateTime.ToString("yyyyMMddHHmmss", new CultureInfo("en-US")),
                currentDateTime.Millisecond,
                sequence);
        }

        static void ReceiveMailByOauth(string user, string accessToken)
        {
            try
            {
                // Create a folder named "inbox" under current directory
                // to save the email retrieved.
                string localInbox = string.Format("{0}\\inbox", Directory.GetCurrentDirectory());
                // If the folder is not existed, create it.
                if (!Directory.Exists(localInbox))
                {
                    Directory.CreateDirectory(localInbox);
                }
                
                var oauth2 = new SaslMechanismOAuth2 (user, accessToken);
                
                using (var client = new ImapClient ()) {
                    client.Connect ("imap.gmail.com", 993, true);

                    client.Authenticate(oauth2);

                    // The Inbox folder is always available on all IMAP servers...
                    var inbox = client.Inbox;
                    inbox.Open (FolderAccess.ReadOnly);

                    Console.WriteLine ("Total messages: {0}", inbox.Count);
                    Console.WriteLine ("Recent messages: {0}", inbox.Recent);

                    for (int i = inbox.Count - 1; i >= Math.Max(0, inbox.Count - 10); i--) {
                        var message = inbox.GetMessage (i);
                        Console.WriteLine ("Subject: {0}", message.Subject);
                    }

                    client.Disconnect (true);
                }

                Console.WriteLine("Completed!");
            }
            catch (Exception ep)
            {
                Console.WriteLine("Error: {0}", ep.Message);
            }
        }
    }
}