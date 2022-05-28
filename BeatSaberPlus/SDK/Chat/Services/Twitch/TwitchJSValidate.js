if (document.getElementById('twitch_oauthtoken').value == "") {
    var l_NoTwitchTokenModal = new bootstrap.Modal(document.getElementById('NoTwitchTokenModal'));
    l_NoTwitchTokenModal.show();
    return false;
}

var l_TwitchChannel1 = document.getElementById('twitch_channel1');
var l_TwitchChannel2 = document.getElementById('twitch_channel2');
var l_TwitchChannel3 = document.getElementById('twitch_channel3');
var l_TwitchChannel4 = document.getElementById('twitch_channel4');
var l_TwitchChannel5 = document.getElementById('twitch_channel5');

if (l_TwitchChannel1.value == "" && l_TwitchChannel2.value == "" && l_TwitchChannel3.value == "" && l_TwitchChannel4.value == "" && l_TwitchChannel5.value == "") {
    var l_NoTwitchChannelModal = new bootstrap.Modal(document.getElementById('NoTwitchChannelModal'));
    l_NoTwitchChannelModal.show();
    return false;
}