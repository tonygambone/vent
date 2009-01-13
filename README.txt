
Vent is a browser-based chat application written for ASP.NET 3.5, 
using jQuery and Integrated Windows Authentication (NTLM).  It also uses
SoundManager 2 for sound.

Its goal is to be a simple, easy-to-deploy, easy-to-use chat room 
application for people within a corporate Intranet.

I wrote it because I've been trying to get an instant messaging system
approved at work for years now, without much success. This is the 
lowest-impact chat system I can think of - integrated authentication, 
no database, no archiving, no client software installation, using a 
mainstream enterprise platform. If this won't work, nothing will.

Some features:

 * Does not require a database; just IIS, ASP.NET, and .NET 3.5.
   
 * Users are automatically authenticated with their Windows username,
   and this username is what identifies them in the chat.
   
 * Users can see who's signed in to the chat.
 
 * Idle users are automatically signed out.
 
 * Users can send private messages to one or more users by including
   text like "@username" in their message.
   
 * Sounds played for join, leave, message sent, message received,
   and error events.
   
 * Sounds can be turned off, too.
 
 * Nifty help text.
 
 * Some nice DHTML eye candy.
 
 * No archiving of chat history - restart IIS and the messages are gone
   forever.

Some things that ought to be supported, but aren't, yet:

 * Multiple chat rooms in one instance of the application. If you want
   separate chat rooms, you'll have to install more than one copy of
   the application.
 
 * One user signed in from multiple browsers. Currently, only
   one browser gets the user's messages.
 
 * Other forms of authentication.
 
 * Maybe someone needs archiving of message history.

 Finally, many thanks to Charlie and Sarah Park for the name!
