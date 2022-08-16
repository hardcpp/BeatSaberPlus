using Newtonsoft.Json;

namespace CP_SDK.Chat.Models.Twitch
{
    public class _7TVCosmetics
    {
        public class _7TVBadge
        {
            [JsonProperty] public string id;
            [JsonProperty] public string name;
            [JsonProperty] public string tooltip;
            [JsonProperty] public string[][] urls;
            [JsonProperty] public string[] users;
            [JsonProperty] public bool misc;
        }

        public class _7TVPaint
        {
            public class Drop_Shadow
            {
                [JsonProperty] public float x_offset;
                [JsonProperty] public float y_offset;
                [JsonProperty] public float radius;
                [JsonProperty] public int color;
            }
            public class Animation
            {
                [JsonProperty] public int speed;
                [JsonProperty] public object keyframes;
            }
            public class Stop
            {
                [JsonProperty] public float at;
                [JsonProperty] public int color;
            }

            [JsonProperty] public string id;
            [JsonProperty] public string name;
            [JsonProperty] public string[] users;
            [JsonProperty] public string function;
            [JsonProperty] public int? color;
            [JsonProperty] public Stop[] stops;
            [JsonProperty] public bool repeat;
            [JsonProperty] public int angle;
            [JsonProperty] public string shape;
            [JsonProperty] public Drop_Shadow drop_shadow;
            [JsonProperty] public Drop_Shadow[] drop_shadows;
            [JsonProperty] public Animation animation;
            [JsonProperty] public string image_url;
        }

        [JsonProperty] public _7TVBadge[] badges;
        [JsonProperty] public _7TVPaint[] paints;
    }
}
