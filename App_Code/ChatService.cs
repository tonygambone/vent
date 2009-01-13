using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;
using System.Web.Caching;

namespace DEQServices
{
    /// <summary>
    /// Implementation of the chat service.
    /// </summary>
    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed),
    ServiceBehavior(
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple,
        IncludeExceptionDetailInFaults=true
        )
    ]
    public class ChatService : IChatService
    {
        #region Private Fields

        /// <summary>
        /// List of users who are currently joined to this chat.
        /// </summary>
        private IList<Message> Messages = new List<Message>();

        /// <summary>
        /// List of users known to the system, with the username as key.
        /// </summary>
        private IDictionary<string, User> Users = new Dictionary<string, User>();

        /// <summary>
        /// Object to be used for synchronization in some operations.
        /// </summary>
        private object userLockObject = new object();
        private object messageLockObject = new object();

        /// <summary>
        /// Username to be used for system messages.
        /// </summary>
        private readonly static string SYSTEM_USER = "System";

        /// <summary>
        /// Regular expression to find addressed usernames in messages,
        /// using the "@username" syntax.
        /// </summary>
        private readonly static Regex userAddressRegex = new Regex(@"
                (?:^|\s)    # match the start of the string, or a whitespace character,
                            # but do not capture it in a group
                @           # literal '@' symbol
                (\S+)       # at least one non-whitespace character, captured
            ", RegexOptions.IgnorePatternWhitespace);

        #endregion

        #region Public Web Service Methods

        /// <summary>
        /// Get the current username from Windows Integrated Authentication.
        /// </summary>
        /// <returns>current username, or null if it cannot be determined</returns>
        public string GetCurrentUsername()
        {
            try
            {
                return HttpContext.Current.User.Identity.Name.Split('\\').Last();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Join the chat with the current user.
        /// 
        /// Requires a non-empty username.
        /// </summary>
        /// <returns>true on success or already joined, false if the username is invalid.</returns>
        public bool Join()
        {
            lock (userLockObject)
            {
                string user = GetCurrentUsername();
                if (!RequireNonEmptyUser(user)) return false;
                if (Users.ContainsKey(user))
                {
                    if (!Users[user].IsJoined)
                    {
                        /*
                        Messages.Add(new Message
                        {
                            User = SYSTEM_USER,
                            Text = String.Format(" --- {0} joined the chat. --- ", user),
                            Time = DateTime.UtcNow
                        });
                        */
                        Users[user].IsJoined = true;
                        Users[user].LastSeenTime = DateTime.UtcNow;
                    }                    
                } else {
                    Users[user] = new User
                    {
                        IsJoined = true,
                        LastMessageRequestTime = DateTime.MinValue,
                        Name = user,
                        LastSeenTime = DateTime.UtcNow
                    };
                }
            }
            return true;
        }

        /// <summary>
        /// Get a list of currently joined users.
        /// 
        /// Requires that the current user has joined the chat.
        /// </summary>
        /// <returns></returns>
        public IList<string> GetJoinedUsers()
        {
            // clean up idle users
            lock (userLockObject)
            {
                if (!RequireJoinedUser(GetCurrentUsername())) return null;
                foreach (string idleUser in (from u in Users
                                             where
                                                u.Value.IsJoined &&
                                                u.Value.LastSeenTime.AddMinutes(1) < DateTime.UtcNow
                                             select u.Key))
                {
                    Users[idleUser].IsJoined = false;
                    /*
                    Messages.Add(new Message
                    {
                        User = SYSTEM_USER,
                        Text = String.Format(" --- {0} left the chat (idle). --- ", idleUser),
                        Time = DateTime.UtcNow
                    });
                    */
                }
                return (from u in Users where u.Value.IsJoined select u.Key).ToList();
            }
        }

        /// <summary>
        /// Leave the chat with the current user.
        /// 
        /// Requires that the current user has joined the chat.
        /// </summary>
        public void Leave()
        {
            lock (userLockObject)
            {
                string user = GetCurrentUsername();
                if (!RequireJoinedUser(user)) return;
                Users[user].IsJoined = false;
                /*
                Messages.Add(new Message
                {
                    User = SYSTEM_USER,
                    Text = String.Format(" --- {0} left the chat. --- ", user),
                    Time = DateTime.UtcNow
                });
                */
            }
        }

        /// <summary>
        /// Get a list of all messages since the user's last request.
        /// Only includes public messages and those addressed to this user.
        /// 
        /// Requires that the current user has joined the chat.
        /// </summary>
        /// <returns></returns>
        public IList<Message> GetMessages()
        {
            lock (userLockObject) {
                string user = GetCurrentUsername();
                if (!RequireJoinedUser(user)) return null;

                lock (messageLockObject)
                {
                    IList<Message> messages = (
                        from m in Messages
                        where m.Time > Users[user].LastMessageRequestTime
                        && IsPublicOrAddressedToOrSentBy(m, user)
                        select m).ToList();
                    Users[user].LastMessageRequestTime = DateTime.UtcNow;
                    Users[user].LastSeenTime = DateTime.UtcNow;
                    return messages;
                }
            }
        }

        /// <summary>
        /// Post a message.
        /// 
        /// Requires that the current user has joined the chat.
        /// </summary>
        /// <param name="message"></param>
        public void PostMessage(string message)
        {
            if (String.IsNullOrEmpty(message)) return;
            lock (userLockObject)
            {
                string user = GetCurrentUsername();
                if (!RequireJoinedUser(user)) return;                

                lock (messageLockObject)
                {
                    Messages.Add(new Message
                    {
                        Text = message.Trim(),
                        Time = DateTime.UtcNow,
                        User = user
                    });
                    Users[user].LastSeenTime = DateTime.UtcNow;
                }
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Get a list of users that the message is addressed to
        /// using the "@username" syntax.  An empty list is a public
        /// message.  Usernames are converted to lowercase to allow
        /// case-insensitive comparisons.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private IList<string> AddressedUsers(string message)
        {
            IList<string> users = new List<string>();
            Match match = userAddressRegex.Match(message);
            while (match.Success)
            {
                users.Add(match.Groups[1].Value.ToLower());
                match = match.NextMatch();
            }
            return users.Distinct().ToList();
        }

        /// <summary>
        /// Determine whether a message should be displayed to a given user.
        /// 
        /// This includes public messages (with no addressed users), 
        /// messages addressed to the user, and messages sent by the user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        private bool IsPublicOrAddressedToOrSentBy(Message message, string username)
        {
            if (message.User == username) { return true; }
            IList<string> users = AddressedUsers(message.Text);
            return (users.Count == 0 || users.Contains(username.ToLower()));
        } 

        /// <summary>
        /// Deny access if the username is empty.
        /// </summary>
        /// <param name="user">username to check</param>
        /// <returns>true if allowed, false if denied</returns>
        private bool RequireNonEmptyUser(string user)
        {
            if (String.IsNullOrEmpty(user))
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Forbidden;
                // send a debug message
                Messages.Add(new Message
                {
                    Text = String.Format(
                       "@amgambone - null user for {1}",
                       user,
                       HttpContext.Current.Request.Path),
                    Time = DateTime.UtcNow,
                    User = SYSTEM_USER
                });
                return false;
            }
            return true;
        }

        /// <summary>
        /// Deny access if the username is not joined to the chat.
        /// </summary>
        /// <param name="user">username to check</param>
        /// <returns>true if allowed, false if denied</returns>
        private bool RequireJoinedUser(string user)
        {
            if (!RequireNonEmptyUser(user)) return false;
            if (!IsUserJoined(user))
            {
                WebOperationContext.Current.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.Forbidden;
                // send a debug message
                Messages.Add(new Message
                {
                    Text = String.Format(
                       "@amgambone - {0} was not joined for {1}",
                       user,
                       HttpContext.Current.Request.Path),
                    Time = DateTime.UtcNow,
                    User = SYSTEM_USER
                });
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the given username is joined to the chat.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private bool IsUserJoined(string username)
        {
            return Users.ContainsKey(username) && Users[username].IsJoined;
        }

        #endregion
    }
}
