<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Vent</title>
    <link rel="stylesheet" type="text/css" href="style.css" />
    <script type="text/javascript" src="jquery-1.2.6.min.js"></script>
    <script type="text/javascript" src="soundmanager/script/soundmanager2-nodebug-jsmin.js"></script>
    <script type="text/javascript" src="jquery.pageslide-0.2.js"></script>
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
    
    <div id="container">
    <div id="error" class="error"></div>
    
    <div id="ctl_block">
      <a id="mutectl">mute</a><br />
      <a id="helpctl" href="help.html">help</a>
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
</body>
</html>
