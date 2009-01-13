<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Vent</title>
    <link rel="stylesheet" type="text/css" href="style.css" />
    <script type="text/javascript" src="jquery-1.2.6.min.js"></script>
    <script type="text/javascript" src="soundmanager/script/soundmanager2-nodebug-jsmin.js"></script>
    <script type="text/javascript" src="jquery.simplemodal.js"></script>
    <script type="text/javascript" src="vent.js"></script>
</head>
<body>
    <form id="form1" runat="server">
      <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
            <Services>
                <asp:ServiceReference Path="ChatService.svc" />
            </Services>
        </asp:ScriptManager>
      </div>
    </form>
    
    <h1>Vent</h1>
    
    <div id="container">
    <div id="error" class="error"></div>
    
    <div id="ctl_block">
      <a id="mutectl">mute</a><br />
      <a id="helpctl">help</a>
    </div>
    
    <div id="user_block">
      Current user: <span id="current_user"></span>
    </div>
    
    <div id="status_block">
      <span id="status">You are not joined to the chat.</span>
      <input type="button" id="join_leave" value="Join"/>
    </div>
    
    <div id="users_block">
      Joined users:
      <ul id="users"></ul>
    </div>
    
    <div id="message_block">
        <textarea id="message" cols="80" rows="2"></textarea>
        <div id="message_privacy">This message will be public.</div>
    </div>
    
    <div id="messages_block">
      <table id="messages"></table>
    </div>
    </div>

<div id="dialog_bg"></div>
<div id="help_container">
<div id="help">
  <div id="close_dialog" title="Close help">X</div>
  <h2>Help for Vent</h2>
  
  <p>Vent lets you chat with other network
  users, using only your web browser. Think of
  a large room in which anyone and everyone can
  speak and be heard.</p>
  
  <h3>Joining</h3>
  
  <p>To participate, you must 'join' the chat by
  clicking the Join button.
  Vent knows who you are - it will use your
  Windows login ID to identify you.  All your
  messages will have your user ID on them.</p>
  
  <p>When you join, the other people in the chat
  will see that you're there, and you'll be able
  to exchange messages with them.</p>
  
  <h3>Leaving</h3>
  
  <p>You can also 'leave' the chat once you've 
  joined by clicking the 'Leave' button. If you
  do this, you won't get any new messages, and you
  won't be notified if other people join the chat.</p>
  
  <h3>Sending a message</h3>
  
  <p>To send a message, type in the big gray box
  and hit 'Enter'.  Everyone in the room will see
  your message.</p>
  
  <h3>Private messages</h3>
  
  <p>If you just want to send a message to one other
  person, and not to the whole room, click their
  username under 'Joined Users'. It will add their
  username to the box with a '@' symbol, and the
  box will turn red. Your message is now private -
  it will only be sent to that user, and no one else
  will see it.</p>
   
  <p>You can even send a private message to more than
  one user - just click their usernames, too. All users
  with '@' symbols in the message will see it, and no
  one else.</p>
  
  <h3>Sound</h3>
  
  <p>One last thing: you can turn sounds off by clicking
  'mute', and turn them back on by clicking 'unmute'.</p>
  
  <p>Enjoy!</p>
</div>
</div>
</body>
</html>
