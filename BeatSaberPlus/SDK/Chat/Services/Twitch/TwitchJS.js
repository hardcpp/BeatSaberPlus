function OnTwitchChannels() {
    var l_Channel1 = document.getElementById('twitch_channel1').value != "";
    var l_Channel2 = document.getElementById('twitch_channel2').value != "";
    var l_Channel3 = document.getElementById('twitch_channel3').value != "";
    var l_Channel4 = document.getElementById('twitch_channel4').value != "";

    document.getElementById('twitch_channel2_grp').style.display = (l_Channel1) ? "flex" : "none";
    document.getElementById('twitch_channel3_grp').style.display = (l_Channel2) ? "flex" : "none";
    document.getElementById('twitch_channel4_grp').style.display = (l_Channel3) ? "flex" : "none";
    document.getElementById('twitch_channel5_grp').style.display = (l_Channel4) ? "flex" : "none";
}

OnTwitchChannels();

if (l_URL.includes("#access_token=")) {
    var l_NewToken = l_URL.substring(l_URL.indexOf("#access_token=") + "#access_token=".length, l_URL.indexOf("&scope="));
    var l_TokenInput = document.getElementById("twitch_oauthtoken");
    l_TokenInput.value = "oauth:" + l_NewToken;

    new bootstrap.Modal(document.getElementById('TwitchTokenUpdatedModal')).show();
    window.top.history.replaceState("statedata", "{APPLICATION_NAME} Settings", l_URL.substring(0, l_URL.indexOf("#")));
}

