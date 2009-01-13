// enable the jQuery IntelliSense docs in Visual Studio
// (requires VS 2008 SP1 with the hotfix from MS KB958502)
/// <reference path="~/jquery-1.2.6-vsdoc.js"/>

// add a reference to SoundManager
/// <reference path="~/soundmanager/script/soundmanager2.js"/>

// add a reference to jQuery SimpleModal
/// <reference path="~/jquery.simplemodal.js"/>

// settings
var checkForUsersInterval = 10000;
var checkForMessagesInterval = 4000;
var errorMessageDisplayTime = 7000;

// create a service object
var service;

var userFetchIntervalHandle;
var messageFetchIntervalHandle;
var joined;

var rowStyleToggle = false;

var userCount = 0;

window.onbeforeunload = function() {
    if (joined) {
        return "You're currently joined to the chat. If you continue, you'll no longer receive chat messages!";
    }
}

// init sound
if (soundManager) {
    soundManager.url = 'soundmanager/swf/';
    soundManager.onload = function() {
            soundManager.createSound(
            {
                id: 'alert',
                url: 'sounds/alert.wav.mp3',
                autoLoad: true
            });
            soundManager.createSound(
            {
                id: 'login',
                url: 'sounds/login.wav.mp3',
                autoLoad: true
            });
            soundManager.createSound(
            {
                id: 'logout',
                url: 'sounds/logout.wav.mp3',
                autoLoad: true
            });
            soundManager.createSound(
            {
                id: 'receive',
                url: 'sounds/receive.wav.mp3',
                autoLoad: true
            });
            soundManager.createSound(
            {
                id: 'send',
                url: 'sounds/send.wav.mp3',
                autoLoad: true
            });
    };
}

var soundEnabled = true;

var privateMsgRegex = /(^|\s)@\S+/;

$(document).ready(function() {
    // create a service object
    service = new DEQServices.IChatService();

    // fetch and display the current username
    service.GetCurrentUsername(function(name) { $('#current_user').text(name); },
        function() { handleError('username'); });

    // set the join button click handler
    $('#join_leave').click(function() { (joined) ? leave() : join(); });

    // set the textarea keypress handler
    $('#message').keypress(function(e) {
        e = (e) ? e : event;
        chr = (e.which) ? e.which : e.keyCode;
        if (chr == 13) {
            service.PostMessage($('#message').val(),
                function() {
                    $('#message').val('');
                    fetchMessages(false); // fetch the message so it is displayed, but no sound
                    playSound('send');
                    highlightPrivateMessage();
                }, function() { handleError('post'); });
            return false;
        }
    });

    // set the textarea keyup handler
    $('#message').keyup(highlightPrivateMessage);

    // set the mute click handler
    $('#mutectl').click(function() {
        soundEnabled = !soundEnabled;
        $('#mutectl').text((soundEnabled) ? 'mute' : 'unmute');
    });

    // set the help click handler
    $('#helpctl').click(function(e) {
        e.preventDefault();
        $('#help').modal({
            close: false,
            position: ["5%", "5%"],
            overlayId: 'dialog_bg',
            containerId: 'help_container'
        });
    });

    // set the modal close handler
    $('#close_dialog').click(function() { $.modal.close(); });
});

function highlightPrivateMessage() {
    if (privateMsgRegex.test($('#message').val())) {
        $('#message').addClass('private');
        $('#message_privacy').text('This message will be private.');
    } else {
        $('#message').removeClass('private');
        $('#message_privacy').text('This message will be public.');
    }
}

function join() {
    // join the chat
    service.Join(function() {
        joined = true;
        $('#status').html('You are joined to the chat.');
        $('#join_leave').val('Leave');
        playSound('login');
    }, function() { handleError('join'); });

    // start fetching a list of users at an interval
    fetchUsers(false);
    if (!userFetchIntervalHandle) {
        userFetchIntervalHandle = window.setInterval(function() { fetchUsers(true); }, checkForUsersInterval);
    }

    // start fetching a list of messages at an interval
    fetchMessages(true);
    if (!messageFetchIntervalHandle) {
        messageFetchIntervalHandle = window.setInterval(function() { fetchMessages(true); }, checkForMessagesInterval);
    }
}

function leave() {
    service.Leave(setNotJoined, function() { handleError('leave'); setNotJoined(); });
    window.clearInterval(userFetchIntervalHandle);
    userFetchIntervalHandle = null;
    window.clearInterval(messageFetchIntervalHandle);
    messageFetchIntervalHandle = null;
}

function setNotJoined() {
    joined = false;
    $('#status').html('You are not joined to the chat.');
    $('#join_leave').val('Join');
    playSound('logout');
}

function fetchUsers(withSound) {
    service.GetJoinedUsers(function(users) {
        $('#users').html('');
        $.each(users, function() { $('<li>' + this + '</li>').appendTo($('#users')); });
        $('#users>li').click(function() {
            var content = $('#message').val();
            $('#message').val(content + ((content.length == 0 || content.charAt(content.length - 1) == ' ') ? '' : ' ') + '@' + $(this).text() + ' ');
            highlightPrivateMessage();
            $('#message').focus();
        });
        if (withSound) {
            if (userCount < users.length) {
                playSound('login');
            } else if (userCount > users.length) {
                playSound('logout');
            }
        }
        userCount = users.length;
    }, function() { handleError('users'); });
}

function fetchMessages(withSound) {    
    service.GetMessages(function(messages) {
        $.each(messages, function() {
            $('<tr class="row_style_' + ((rowStyleToggle = !rowStyleToggle) ? '1' : '2') + '"><td class="msg_meta"><span class="msg_user' + ((this.User == $('#current_user').text()) ? ' current_user' : '') + '">' + this.User + '</span><br/><span class="msg_time">' + formatTime(this.Time) + '</span></td></tr>').append($('<td class="msg_text"></td>').text(this.Text)).prependTo($('#messages')).hide().show("fast");
        });
        if (withSound && messages.length > 0) {
            playSound('receive');
        }
    }, function() { handleError('messages'); });    
}

function formatTime(time) {
    var year = time.getFullYear();
    var month = leadingZero(time.getMonth() + 1);
    var date = leadingZero(time.getDate());
    var hours = time.getHours();
    var minutes = leadingZero(time.getMinutes());
    return year + "-" + month + "-" + date + " " + hours + ":" + minutes;
}

function leadingZero(value) {
    if (value >= 10) return value;
    return "0" + value;
}

function handleError(key) {
    playSound('alert');
    var message;
    switch (key) {
        case 'username':
            message = "Couldn't get your username. Try reloading the page or logging out and back in.";
            break;
        case 'post':
            if (joined) {
                message = "Couldn't send your message. Try reloading the page and joining again.";
            } else {
                message = "Couldn't send your message. You need to join the chat first!";
            }
            break;
        case 'join':
            message = "Couldn't join the chat. Try reloading the page and joining again.";
            break;
        case 'leave':
            message = "Couldn't leave the chat. That's probably okay, though; just reload the page.";
            break;
        case 'users':
            message = "Couldn't get the list of joined users. If this keeps happening, try reloading the page and joining again.";
            break;
        case 'messages':
            message = "Couldn't get the list of messages. If this keeps happening, try reloading the page and joining again.";
            break;
    }    
    $('#error').html(message).slideDown("fast");
    window.setTimeout(function() {
        $('#error').html('').slideUp("fast");
    }, errorMessageDisplayTime);
}

function playSound(label) {
    if (soundManager && soundEnabled) {
        soundManager.play(label);
    }
}


