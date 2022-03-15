# GMail OAuth IMAP Demo

This is a very basic demo that demonstrates (interactively) 
getting a Google OAuth token and using it to get email using IMAP.

## Why?

This is just a quick proof of concept that I threw together using a few 
libraries based on various samples across the web to demonstrate the 
steps and requirements to get this to work. It might give others a 
point of reference to build from when setting up an app that 
communicates with a GMail mailbox.

## How?

To set this up, use the following guide to get your Google 
Client ID and Client Secret: 

https://github.com/jstedfast/MailKit/blob/master/GMailOAuth2.md

Then, edit `appsettings.json` with those credentials and you should
be able to run the program. 
