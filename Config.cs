using UnityEngine;
using System.Linq;

namespace BeatSaberPlus
{
    /// <summary>
    /// Config class helper
    /// </summary>
    internal class Config
    {
        /// <summary>
        /// Config instance
        /// </summary>
        private static SDK.Config.INIConfig m_Config = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        internal static bool FirstRun {
            get { return m_Config.GetBool("Config", "FirstRun", true, true);          }
            set {        m_Config.SetBool("Config", "FirstRun", value);               }
        }
        internal static bool FirstChatCoreRun {
            get { return m_Config.GetBool("Config", "FirstChatCoreRun", true, true);  }
            set {        m_Config.SetBool("Config", "FirstChatCoreRun", value);       }
        }

        internal class Chat
        {
            private  static string _s = "Chat";
            internal static string s_ModerationKeyDefault_Split = "#|#";
            private  static string s_ModerationKeyDefault = string.Join(s_ModerationKeyDefault_Split, new string[] {
                "/host",
                "/unban",
                "/untimeout",
                "!bsr"
            });

            internal static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);                      }
                set {        m_Config.SetBool(_s, "Enabled", value);                            }
            }
            internal static int ChatWidth {
                get { return m_Config.GetInt(_s, "ChatWidth", 120, true);                       }
                set {        m_Config.SetInt(_s, "ChatWidth", value);                           }
            }
            internal static int ChatHeight {
                get { return m_Config.GetInt(_s, "ChatHeight", 140, true);                      }
                set {        m_Config.SetInt(_s, "ChatHeight", value);                          }
            }
            internal static bool ReverseChatOrder {
                get { return m_Config.GetBool(_s, "ReverseChatOrder", false, true);             }
                set {        m_Config.SetBool(_s, "ReverseChatOrder", value);                   }
            }
            internal static string SystemFontName {
                get { return m_Config.GetString(_s, "SystemFontName", "Segoe UI", true);        }
                set {        m_Config.SetString(_s, "SystemFontName", value);                   }
            }
            internal static float FontSize {
                get { return m_Config.GetFloat(_s, "FontSize", 3.4f, true);                     }
                set {        m_Config.SetFloat(_s, "FontSize", value);                          }
            }

            internal static bool AlignWithFloor {
                get { return m_Config.GetBool(_s, "AlignWithFloor", true, true);                }
                set {        m_Config.SetBool(_s, "AlignWithFloor", value);                     }
            }
            internal static bool ShowLockIcon {
                get { return m_Config.GetBool(_s, "ShowLockIcon", true, true);                  }
                set {        m_Config.SetBool(_s, "ShowLockIcon", value);                       }
            }
            internal static bool FollowEnvironementRotation {
                get { return m_Config.GetBool(_s, "FollowEnvironementRotation", true, true);    }
                set {        m_Config.SetBool(_s, "FollowEnvironementRotation", value);         }
            }
            internal static bool ShowViewerCount {
                get { return m_Config.GetBool(_s, "ShowViewerCount", true, true);               }
                set {        m_Config.SetBool(_s, "ShowViewerCount", value);                    }
            }
            internal static bool ShowFollowEvents {
                get { return m_Config.GetBool(_s, "ShowFollowEvents", true, true);              }
                set {        m_Config.SetBool(_s, "ShowFollowEvents", value);                   }
            }
            internal static bool ShowSubscriptionEvents {
                get { return m_Config.GetBool(_s, "ShowSubscriptionEvents", true, true);        }
                set {        m_Config.SetBool(_s, "ShowSubscriptionEvents", value);             }
            }
            internal static bool ShowBitsCheeringEvents {
                get { return m_Config.GetBool(_s, "ShowBitsCheeringEvents", true, true);        }
                set {        m_Config.SetBool(_s, "ShowBitsCheeringEvents", value);             }
            }
            internal static bool ShowChannelPointsEvent {
                get { return m_Config.GetBool(_s, "ShowChannelPointsEvent", true, true);        }
                set {        m_Config.SetBool(_s, "ShowChannelPointsEvent", value);             }
            }
            internal static bool FilterViewersCommands {
                get { return m_Config.GetBool(_s, "FilterViewersCommands", false, true);        }
                set {        m_Config.SetBool(_s, "FilterViewersCommands", value);              }
            }
            internal static bool FilterBroadcasterCommands {
                get { return m_Config.GetBool(_s, "FilterBroadcasterCommands", false, true);    }
                set {        m_Config.SetBool(_s, "FilterBroadcasterCommands", value);          }
            }

            internal static float BackgroundA { get { return m_Config.GetFloat(_s, "BackgroundA", 0.5f, true); } set { m_Config.SetFloat(_s, "BackgroundA", value); } }
            internal static float BackgroundR { get { return m_Config.GetFloat(_s, "BackgroundR",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundR", value); } }
            internal static float BackgroundG { get { return m_Config.GetFloat(_s, "BackgroundG",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundG", value); } }
            internal static float BackgroundB { get { return m_Config.GetFloat(_s, "BackgroundB",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundB", value); } }
            internal static Color BackgroundColor => new Color(BackgroundR, BackgroundG, BackgroundB, BackgroundA);

            internal static float HighlightA { get { return m_Config.GetFloat(_s, "HighlightA", 0.12f, true); } set { m_Config.SetFloat(_s, "HighlightA", value); } }
            internal static float HighlightR { get { return m_Config.GetFloat(_s, "HighlightR", 0.57f, true); } set { m_Config.SetFloat(_s, "HighlightR", value); } }
            internal static float HighlightG { get { return m_Config.GetFloat(_s, "HighlightG", 0.28f, true); } set { m_Config.SetFloat(_s, "HighlightG", value); } }
            internal static float HighlightB { get { return m_Config.GetFloat(_s, "HighlightB",    1f, true); } set { m_Config.SetFloat(_s, "HighlightB", value); } }
            internal static Color HighlightColor => new Color(HighlightR, HighlightG, HighlightB, HighlightA);

            internal static float AccentA { get { return m_Config.GetFloat(_s, "AccentA", 1.00f, true); } set { m_Config.SetFloat(_s, "AccentA", value); } }
            internal static float AccentR { get { return m_Config.GetFloat(_s, "AccentR", 0.57f, true); } set { m_Config.SetFloat(_s, "AccentR", value); } }
            internal static float AccentG { get { return m_Config.GetFloat(_s, "AccentG", 0.28f, true); } set { m_Config.SetFloat(_s, "AccentG", value); } }
            internal static float AccentB { get { return m_Config.GetFloat(_s, "AccentB",    1f, true); } set { m_Config.SetFloat(_s, "AccentB", value); } }
            internal static Color AccentColor => new Color(AccentR, AccentG, AccentB, AccentA);

            internal static float TextA { get { return m_Config.GetFloat(_s, "TextA", 1f, true); } set { m_Config.SetFloat(_s, "TextA", value); } }
            internal static float TextR { get { return m_Config.GetFloat(_s, "TextR", 1f, true); } set { m_Config.SetFloat(_s, "TextR", value); } }
            internal static float TextG { get { return m_Config.GetFloat(_s, "TextG", 1f, true); } set { m_Config.SetFloat(_s, "TextG", value); } }
            internal static float TextB { get { return m_Config.GetFloat(_s, "TextB", 1f, true); } set { m_Config.SetFloat(_s, "TextB", value); } }
            internal static Color TextColor => new Color(TextR, TextG, TextB, TextA);

            internal static float PingA { get { return m_Config.GetFloat(_s, "PingA", 0.18f, true); } set { m_Config.SetFloat(_s, "PingA", value); } }
            internal static float PingR { get { return m_Config.GetFloat(_s, "PingR", 1.00f, true); } set { m_Config.SetFloat(_s, "PingR", value); } }
            internal static float PingG { get { return m_Config.GetFloat(_s, "PingG", 0.00f, true); } set { m_Config.SetFloat(_s, "PingG", value); } }
            internal static float PingB { get { return m_Config.GetFloat(_s, "PingB", 0.00f, true); } set { m_Config.SetFloat(_s, "PingB", value); } }
            internal static Color PingColor => new Color(PingR, PingG, PingB, PingA);

            internal static float MenuChatPositionX { get { return m_Config.GetFloat(_s, "MenuChatPositionX",     0, true); } set { m_Config.SetFloat(_s, "MenuChatPositionX", value); } }
            internal static float MenuChatPositionY { get { return m_Config.GetFloat(_s, "MenuChatPositionY", 3.87f, true); } set { m_Config.SetFloat(_s, "MenuChatPositionY", value); } }
            internal static float MenuChatPositionZ { get { return m_Config.GetFloat(_s, "MenuChatPositionZ", 2.50f, true); } set { m_Config.SetFloat(_s, "MenuChatPositionZ", value); } }
            internal static float MenuChatRotationX { get { return m_Config.GetFloat(_s, "MenuChatRotationX",  325f, true); } set { m_Config.SetFloat(_s, "MenuChatRotationX", value); } }
            internal static float MenuChatRotationY { get { return m_Config.GetFloat(_s, "MenuChatRotationY",    0f, true); } set { m_Config.SetFloat(_s, "MenuChatRotationY", value); } }
            internal static float MenuChatRotationZ { get { return m_Config.GetFloat(_s, "MenuChatRotationZ",    0f, true); } set { m_Config.SetFloat(_s, "MenuChatRotationZ", value); } }

            internal static float PlayingChatPositionX { get { return m_Config.GetFloat(_s, "PlayingChatPositionX",     0, true); } set { m_Config.SetFloat(_s, "PlayingChatPositionX", value); } }
            internal static float PlayingChatPositionY { get { return m_Config.GetFloat(_s, "PlayingChatPositionY", 3.75f, true); } set { m_Config.SetFloat(_s, "PlayingChatPositionY", value); } }
            internal static float PlayingChatPositionZ { get { return m_Config.GetFloat(_s, "PlayingChatPositionZ", 2.50f, true); } set { m_Config.SetFloat(_s, "PlayingChatPositionZ", value); } }
            internal static float PlayingChatRotationX { get { return m_Config.GetFloat(_s, "PlayingChatRotationX",  325f, true); } set { m_Config.SetFloat(_s, "PlayingChatRotationX", value); } }
            internal static float PlayingChatRotationY { get { return m_Config.GetFloat(_s, "PlayingChatRotationY",    0f, true); } set { m_Config.SetFloat(_s, "PlayingChatRotationY", value); } }
            internal static float PlayingChatRotationZ { get { return m_Config.GetFloat(_s, "PlayingChatRotationZ",    0f, true); } set { m_Config.SetFloat(_s, "PlayingChatRotationZ", value); } }

            internal static string ModerationKeys {
                get { return m_Config.GetString(_s, "ModerationKeys", s_ModerationKeyDefault, true); }
                set {        m_Config.SetString(_s, "ModerationKeys", value);                        }
            }

            internal static void Reset()
            {
                ChatWidth               = 120;
                ChatHeight              = 140;
                ReverseChatOrder        = false;
                SystemFontName          = "Segoe UI";
                FontSize                = 3.4f;

                AlignWithFloor              = true;
                ShowLockIcon                = true;
                FollowEnvironementRotation  = true;
                ShowViewerCount             = true;
                ShowFollowEvents            = true;
                ShowSubscriptionEvents      = true;
                ShowBitsCheeringEvents      = true;
                ShowChannelPointsEvent      = true;
                FilterViewersCommands       = false;
                FilterBroadcasterCommands   = false;

                BackgroundA = 0.5f;
                BackgroundR = 0.0f;
                BackgroundG = 0.0f;
                BackgroundB = 0.0f;

                HighlightA = 0.12f;
                HighlightR = 0.57f;
                HighlightG = 0.28f;
                HighlightB = 1.00f;

                AccentA = 0.12f;
                AccentR = 0.57f;
                AccentG = 0.28f;
                AccentB = 1.00f;

                TextA = 1.00f;
                TextR = 1.00f;
                TextG = 1.00f;
                TextB = 1.00f;

                PingA = 0.18f;
                PingR = 1.00f;
                PingG = 0.00f;
                PingB = 0.00f;

                ModerationKeys = s_ModerationKeyDefault;

                ResetPosition();
            }

            internal static void ResetPosition()
            {
                MenuChatPositionY = 3.87f;
                MenuChatPositionZ = 2.50f;
                MenuChatRotationX = 325f;
                MenuChatPositionX = MenuChatRotationY = MenuChatRotationZ = 0f;

                PlayingChatPositionY = 3.75f;
                PlayingChatPositionZ = 2.50f;
                PlayingChatRotationX = 325f;
                PlayingChatPositionX = PlayingChatRotationY = PlayingChatRotationZ = 0f;
            }
        }

        internal class ChatEmoteRain
        {
            private static string _s = "ChatEmoteRain";

            internal static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);          }
                set {        m_Config.SetBool(_s, "Enabled", value);                }
            }
            internal static string Prefix => "!er";

            internal static bool MenuRain {
                get { return m_Config.GetBool(_s, "MenuRain", true, true);          }
                set {        m_Config.SetBool(_s, "MenuRain", value);               }
            }
            internal static float MenuRainSize {
                get { return m_Config.GetFloat(_s, "MenuRainSize", 0.4f, true);     }
                set {        m_Config.SetFloat(_s, "MenuRainSize", value);          }
            }
            internal static float MenuFallSpeed {
                get { return m_Config.GetFloat(_s, "MenuFallSpeed",   3f, true);   }
                set {        m_Config.SetFloat(_s, "MenuFallSpeed", value);        }
            }

            internal static bool SongRain {
                get { return m_Config.GetBool(_s, "SongRain", true, true);          }
                set {        m_Config.SetBool(_s, "SongRain", value);               }
            }
            internal static float SongRainSize {
                get { return m_Config.GetFloat(_s, "SongRainSize", 0.6f, true);     }
                set {        m_Config.SetFloat(_s, "SongRainSize", value);          }
            }
            internal static float SongFallSpeed {
                get { return m_Config.GetFloat(_s, "SongFallSpeed",   3f, true);   }
                set {        m_Config.SetFloat(_s, "SongFallSpeed", value);        }
            }

            internal static bool ModeratorPower {
                get { return m_Config.GetBool(_s, "ModeratorPower", true, true);        }
                set {        m_Config.SetBool(_s, "ModeratorPower", value);             }
            }
            internal static bool VIPPower {
                get { return m_Config.GetBool(_s, "VIPPower", false, true);             }
                set {        m_Config.SetBool(_s, "VIPPower", value);                   }
            }
            internal static bool SubscriberPower {
                get { return m_Config.GetBool(_s, "SubscriberPower", false, true);      }
                set {        m_Config.SetBool(_s, "SubscriberPower", value);            }
            }

            internal static int EmoteDelay {
                get { return m_Config.GetInt(_s, "EmoteDelay", 8, true);            }
                set {        m_Config.SetInt(_s, "EmoteDelay", value);              }
            }

            internal static bool SubRain {
                get { return m_Config.GetBool(_s, "SubRain", true, true);           }
                set {        m_Config.SetBool(_s, "SubRain", value);                }
            }
            internal static int SubRainEmoteCount {
                get { return m_Config.GetInt(_s, "SubRainEmoteCount", 20, true);    }
                set {        m_Config.SetInt(_s, "SubRainEmoteCount", value);       }
            }

            internal static bool ComboMode {
                get { return m_Config.GetBool(_s, "ComboMode", false, true);        }
                set {        m_Config.SetBool(_s, "ComboMode", value);              }
            }
            internal static int ComboModeType {
                get { return m_Config.GetInt(_s, "ComboModeType", 0, true);         }
                set {        m_Config.SetInt(_s, "ComboModeType", value);           }
            }
            internal static float ComboTimer {
                get { return m_Config.GetFloat(_s, "ComboTimer", 5f, true);         }
                set {        m_Config.SetFloat(_s, "ComboTimer", value);            }
            }
            internal static int ComboCount {
                get { return m_Config.GetInt(_s, "ComboCount", 3, true);            }
                set {        m_Config.SetInt(_s, "ComboCount", value);              }
            }

            internal static void Reset()
            {
                MenuRain        = true;
                MenuRainSize    = 0.4f;
                MenuFallSpeed   = 3f;

                SongRain        = true;
                SongRainSize    = 0.6f;
                SongFallSpeed   = 3f;

                ModeratorPower  = true;
                VIPPower        = false;
                SubscriberPower = false;

                EmoteDelay      = 8;

                SubRain             = true;
                SubRainEmoteCount   = 20;

                ComboMode       = false;
                ComboModeType   = 0;
                ComboTimer      = 5f;
                ComboCount      = 3;
            }
        }

        internal class ChatIntegrations
        {
            private static string _s = "ChatIntegrations";

            internal static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);                              }
                set {        m_Config.SetBool(_s, "Enabled", value);                                    }
            }
        }

        internal class ChatRequest
        {
            private static string _s = "ChatRequest";

            internal static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);           }
                set {        m_Config.SetBool(_s, "Enabled", value);                 }
            }

            internal static bool QueueOpen {
                get { return m_Config.GetBool(_s, "QueueOpen", true, true);          }
                set {        m_Config.SetBool(_s, "QueueOpen", value);               }
            }

            internal static int UserMaxRequest {
                get { return m_Config.GetInt(_s, "UserMaxRequest", 2, true);         }
                set {        m_Config.SetInt(_s, "UserMaxRequest", value);           }
            }
            internal static int VIPBonusRequest {
                get { return m_Config.GetInt(_s, "VIPBonusRequest", 2, true);        }
                set {        m_Config.SetInt(_s, "VIPBonusRequest", value);          }
            }
            internal static int SubscriberBonusRequest {
                get { return m_Config.GetInt(_s, "SubscriberBonusRequest", 3, true); }
                set {        m_Config.SetInt(_s, "SubscriberBonusRequest", value);   }
            }

            internal static int HistorySize {
                get { return m_Config.GetInt(_s, "HistorySize", 50, true);           }
                set {        m_Config.SetInt(_s, "HistorySize", value);              }
            }
            internal static bool PlayPreviewMusic {
                get { return m_Config.GetBool(_s, "PlayPreviewMusic", true, true);   }
                set {        m_Config.SetBool(_s, "PlayPreviewMusic", value);        }
            }
            internal static bool ModeratorPower {
                get { return m_Config.GetBool(_s, "ModeratorPower", true, true);     }
                set {        m_Config.SetBool(_s, "ModeratorPower", value);          }
            }

            internal static int QueueCommandShowSize {
                get { return m_Config.GetInt(_s, "QueueCommandSize", 4, true);       }
                set {        m_Config.SetInt(_s, "QueueCommandSize", value);         }
            }
            internal static int QueueCommandCooldown {
                get { return m_Config.GetInt(_s, "QueueCommandCooldown", 10, true);  }
                set {        m_Config.SetInt(_s, "QueueCommandCooldown", value);     }
            }

            internal static bool NoBeatSage {
                get { return m_Config.GetBool(_s, "NoBeatSage", false, true);   }
                set {        m_Config.SetBool(_s, "NoBeatSage", value);         }
            }
            internal static bool NPSMin {
                get { return m_Config.GetBool(_s, "NPSMin", false, true);       }
                set {        m_Config.SetBool(_s, "NPSMin", value);             }
            }
            internal static bool NPSMax {
                get { return m_Config.GetBool(_s, "NPSMax", false, true);       }
                set {        m_Config.SetBool(_s, "NPSMax", value);             }
            }
            internal static bool NJSMin {
                get { return m_Config.GetBool(_s, "NJSMin", false, true);       }
                set {        m_Config.SetBool(_s, "NJSMin", value);             }
            }
            internal static bool NJSMax {
                get { return m_Config.GetBool(_s, "NJSMax", false, true);       }
                set {        m_Config.SetBool(_s, "NJSMax", value);             }
            }
            internal static bool DurationMax {
                get { return m_Config.GetBool(_s, "DurationMax", false, true);  }
                set {        m_Config.SetBool(_s, "DurationMax", value);        }
            }
            internal static bool VoteMin {
                get { return m_Config.GetBool(_s, "VoteMin", false, true);      }
                set {        m_Config.SetBool(_s, "VoteMin", value);            }
            }
            internal static bool DateMin {
                get { return m_Config.GetBool(_s, "DateMin", false, true);      }
                set {        m_Config.SetBool(_s, "DateMin", value);            }
            }
            internal static bool DateMax {
                get { return m_Config.GetBool(_s, "DateMax", false, true);      }
                set {        m_Config.SetBool(_s, "DateMax", value);            }
            }

            internal static int NPSMinV {
                get { return m_Config.GetInt(_s, "NPSMinV", 0, true);           }
                set {        m_Config.SetInt(_s, "NPSMinV", value);             }
            }
            internal static int NPSMaxV {
                get { return m_Config.GetInt(_s, "NPSMaxV", 30, true);          }
                set {        m_Config.SetInt(_s, "NPSMaxV", value);             }
            }
            internal static int NJSMinV {
                get { return m_Config.GetInt(_s, "NJSMinV", 0, true);           }
                set {        m_Config.SetInt(_s, "NJSMinV", value);             }
            }
            internal static int NJSMaxV {
                get { return m_Config.GetInt(_s, "NJSMaxV", 30, true);          }
                set {        m_Config.SetInt(_s, "NJSMaxV", value);             }
            }
            internal static int DurationMaxV {
                get { return m_Config.GetInt(_s, "DurationMaxV", 3, true);      }
                set {        m_Config.SetInt(_s, "DurationMaxV", value);        }
            }
            internal static float VoteMinV {
                get { return m_Config.GetFloat(_s, "VoteMinV", 0.5f, true);     }
                set {        m_Config.SetFloat(_s, "VoteMinV", value);          }
            }
            internal static int DateMinV {
                get { return m_Config.GetInt(_s, "DateMinV", 0, true);          }
                set {        m_Config.SetInt(_s, "DateMinV", value);            }
            }
            internal static int DateMaxV {
                get { return m_Config.GetInt(_s, "DateMaxV", 36, true);         }
                set {        m_Config.SetInt(_s, "DateMaxV", value);            }
            }

            internal static string SimpleQueueFileFormat {
                get { return m_Config.GetString(_s, "SimpleQueueFileFormat", "%i - %n by %m", true);    }
                set {        m_Config.SetString(_s, "SimpleQueueFileFormat", value);                    }
            }
            internal static int SimpleQueueFileCount {
                get { return m_Config.GetInt(_s, "SimpleQueueFileCount", 10, true);                     }
                set {        m_Config.SetInt(_s, "SimpleQueueFileCount", value);                        }
            }

            internal static void Reset()
            {
                UserMaxRequest          = 2;
                VIPBonusRequest         = 2;
                SubscriberBonusRequest  = 3;

                HistorySize         = 50;
                PlayPreviewMusic    = true;
                ModeratorPower      = true;

                QueueCommandShowSize = 4;
                QueueCommandCooldown = 10;

                NoBeatSage  = false;
                NPSMin      = false;
                NPSMax      = false;
                NJSMin      = false;
                NJSMax      = false;
                DurationMax = false;
                VoteMin     = false;
                DateMin     = false;
                DateMax     = false;

                NPSMinV         =    0;
                NPSMaxV         =   30;
                NJSMinV         =    0;
                NJSMaxV         =   30;
                DurationMaxV    =    3;
                VoteMinV        = 0.5f;
                DateMinV        =    0;
                DateMaxV        =   36;

                ///SimpleQueueFileFormat = "%i - %n by %m";
                ///SimpleQueueFileCount = 10;
            }
        }

        internal class GameTweaker
        {
            private static string _s = "GameTweaker";

            internal static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);                              }
                set {        m_Config.SetBool(_s, "Enabled", value);                                    }
            }

            internal static bool RemoveDebris {
                get { return m_Config.GetBool(_s, "RemoveDebris", false, true);                         }
                set {        m_Config.SetBool(_s, "RemoveDebris", value);                               }
            }
            internal static bool RemoveAllCutParticles {
                get { return m_Config.GetBool(_s, "RemoveAllCutParticles", m_Config.GetBool(_s, "RemoveCutRibbon", false, false), true); }
                set {        m_Config.SetBool(_s, "RemoveAllCutParticles", value);                      }
            }
            internal static bool RemoveObstacleParticles {
                get { return m_Config.GetBool(_s, "RemoveObstacleParticles", false, true);              }
                set {        m_Config.SetBool(_s, "RemoveObstacleParticles", value);                    }
            }
            internal static bool RemoveSaberBurnMarks {
                get { return m_Config.GetBool(_s, "RemoveSaberBurnMarks", false, true);                 }
                set {        m_Config.SetBool(_s, "RemoveSaberBurnMarks", value);                       }
            }
            internal static bool RemoveSaberBurnMarkSparkles {
                get { return m_Config.GetBool(_s, "RemoveSaberBurnMarkSparkles", false, true);          }
                set {        m_Config.SetBool(_s, "RemoveSaberBurnMarkSparkles", value);                }
            }
            internal static bool RemoveSaberClashEffects {
                get { return m_Config.GetBool(_s, "RemoveSaberClashEffects", false, true);              }
                set {        m_Config.SetBool(_s, "RemoveSaberClashEffects", value);                    }
            }
            internal static bool RemoveWorldParticles {
                get { return m_Config.GetBool(_s, "RemoveWorldParticles", false, true);                 }
                set {        m_Config.SetBool(_s, "RemoveWorldParticles", value);                       }
            }

            internal static bool RemoveMusicBandLogo {
                get { return m_Config.GetBool(_s, "RemoveMusicBandLogo", false, true);                  }
                set {        m_Config.SetBool(_s, "RemoveMusicBandLogo", value);                        }
            }
            internal static bool RemoveFullComboLossAnimation {
                get { return m_Config.GetBool(_s, "RemoveFullComboLossAnimation", false, true);         }
                set {        m_Config.SetBool(_s, "RemoveFullComboLossAnimation", value);               }
            }
            internal static bool NoFake360HUD {
                get { return m_Config.GetBool(_s, "NoFake360HUD", false, true);                         }
                set {        m_Config.SetBool(_s, "NoFake360HUD", value);                               }
            }

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Main menu
            internal static bool DisableBeatMapEditorButtonOnMainMenu {
                get { return m_Config.GetBool(_s, "DisableBeatMapEditorButtonOnMainMenu", false, true); }
                set {        m_Config.SetBool(_s, "DisableBeatMapEditorButtonOnMainMenu", value);       }
            }
            internal static bool RemoveNewContentPromotional {
                get { return m_Config.GetBool(_s, "RemoveNewContentPromotional", true, true);           }
                set {        m_Config.SetBool(_s, "RemoveNewContentPromotional", value);                }
            }
            internal static bool RemoveAnniversaryEvent {
                get { return m_Config.GetBool(_s, "RemoveAnniversaryEvent", true, true);           }
                set {        m_Config.SetBool(_s, "RemoveAnniversaryEvent", value);                }
            }


            /// Level selection
            internal static bool RemoveBaseGameFilterButton {
                get { return m_Config.GetBool(_s, "RemoveBaseGameFilterButton", true, true);            }
                set {        m_Config.SetBool(_s, "RemoveBaseGameFilterButton", value);                 }
            }
            internal static bool ReorderPlayerSettings {
                get { return m_Config.GetBool(_s, "ReorderPlayerSettings", true, true);                 }
                set {        m_Config.SetBool(_s, "ReorderPlayerSettings", value);                      }
            }
            internal static bool AddOverrideLightIntensityOption {
                get { return m_Config.GetBool(_s, "AddOverrideLightIntensityOption", false, true);      }
                set {        m_Config.SetBool(_s, "AddOverrideLightIntensityOption", value);            }
            }
            internal static bool MergeLightPressetOptions {
                get { return m_Config.GetBool(_s, "MergeLightPressetOptions", true, true);  }
                set {        m_Config.SetBool(_s, "MergeLightPressetOptions", value);       }
            }
            internal static float OverrideLightIntensity {
                get { return m_Config.GetFloat(_s, "OverrideLightIntensity", 1.0f, true);               }
                set {        m_Config.SetFloat(_s, "OverrideLightIntensity", value);                    }
            }
            internal static bool DeleteSongButton {
                get { return m_Config.GetBool(_s, "DeleteSongButton", true, true);                      }
                set {        m_Config.SetBool(_s, "DeleteSongButton", value);                           }
            }
            internal static bool DeleteSongBrowserTrashcan
            {
                get { return m_Config.GetBool(_s, "DeleteSongBrowserTrashcan", false, true); }
                set {        m_Config.SetBool(_s, "DeleteSongBrowserTrashcan", value);       }
            }

            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            /// Logs
            internal static bool RemoveOldLogs {
                get { return m_Config.GetBool(_s, "RemoveOldLogs", true, true);                         }
                set {        m_Config.SetBool(_s, "RemoveOldLogs", value);                              }
            }
            internal static int LogEntriesToKeep {
                get { return m_Config.GetInt(_s, "LogEntriesToKeep", 8, true);                          }
                set {        m_Config.SetInt(_s, "LogEntriesToKeep", value);                            }
            }

            /// FPFC escape
            internal static bool FPFCEscape {
                get { return m_Config.GetBool(_s, "FPFCEscape", false, true);                           }
                set {        m_Config.SetBool(_s, "FPFCEscape", value);                                 }
            }

            internal static void Reset()
            {
                RemoveDebris                    = false;
                RemoveAllCutParticles           = false;
                RemoveObstacleParticles         = false;
                RemoveSaberBurnMarks            = false;
                RemoveSaberBurnMarkSparkles     = false;
                RemoveSaberClashEffects         = false;
                RemoveWorldParticles            = false;

                RemoveMusicBandLogo             = false;
                RemoveFullComboLossAnimation    = false;
                NoFake360HUD                    = false;

                /// Main menu
                DisableBeatMapEditorButtonOnMainMenu = false;
                RemoveAnniversaryEvent               = true;
                RemoveNewContentPromotional          = true;

                /// Level selection
                RemoveBaseGameFilterButton      = true;
                ReorderPlayerSettings           = true;
                AddOverrideLightIntensityOption = true;
                MergeLightPressetOptions        = true;
                DeleteSongButton                = true;
                DeleteSongBrowserTrashcan       = false;

                /// Logs
                RemoveOldLogs                   = true;
                LogEntriesToKeep                = 8;

                /// FPFC escape
                FPFCEscape = false;
            }
        }

        internal class MenuMusic
        {
            private static string _s = "MenuMusic";

            internal static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);                          }
                set {        m_Config.SetBool(_s, "Enabled", value);                                }
            }

            internal static bool ShowPlayer {
                get { return m_Config.GetBool(_s, "ShowPlayer", true, true);                        }
                set {        m_Config.SetBool(_s, "ShowPlayer", value);                             }
            }
            internal static bool ShowPlayTime {
                get { return m_Config.GetBool(_s, "ShowPlayTime", true, true);                      }
                set {        m_Config.SetBool(_s, "ShowPlayTime", value);                           }
            }
            internal static float BackgroundA { get { return m_Config.GetFloat(_s, "BackgroundA", 0.5f, true); } set { m_Config.SetFloat(_s, "BackgroundA", value); } }
            internal static float BackgroundR { get { return m_Config.GetFloat(_s, "BackgroundR", 0f, true);   } set { m_Config.SetFloat(_s, "BackgroundR", value); } }
            internal static float BackgroundG { get { return m_Config.GetFloat(_s, "BackgroundG", 0f, true);   } set { m_Config.SetFloat(_s, "BackgroundG", value); } }
            internal static float BackgroundB { get { return m_Config.GetFloat(_s, "BackgroundB", 0f, true);   } set { m_Config.SetFloat(_s, "BackgroundB", value); } }
            internal static Color BackgroundColor => new Color(BackgroundR, BackgroundG, BackgroundB, BackgroundA);

            internal static bool StartSongFromBeginning {
                get { return m_Config.GetBool(_s, "StartSongFromBeginning", false, true);           }
                set {        m_Config.SetBool(_s, "StartSongFromBeginning", value);                 }
            }
            internal static bool StartANewMusicOnSceneChange {
                get { return m_Config.GetBool(_s, "StartANewMusicOnSceneChange", true, true);       }
                set {        m_Config.SetBool(_s, "StartANewMusicOnSceneChange", value);            }
            }
            internal static bool LoopCurrentMusic {
                get { return m_Config.GetBool(_s, "LoopCurrentMusic", false, true);                 }
                set {        m_Config.SetBool(_s, "LoopCurrentMusic", value);                       }
            }
            internal static bool UseOnlyCustomMenuSongsFolder {
                get { return m_Config.GetBool(_s, "UseOnlyCustomMenuSongsFolder", false, true);     }
                set {        m_Config.SetBool(_s, "UseOnlyCustomMenuSongsFolder", value);           }
            }
            internal static float PlaybackVolume {
                get { return m_Config.GetFloat(_s, "PlaybackVolume", 0.5f, true);                   }
                set {        m_Config.SetFloat(_s, "PlaybackVolume", value);                        }
            }

            static internal void Reset()
            {
                ShowPlayer      = true;
                ShowPlayTime    = true;
                BackgroundA     = 0.5f;
                BackgroundR     = BackgroundG = BackgroundB = 0;

                StartSongFromBeginning          = false;
                StartANewMusicOnSceneChange     = true;
                LoopCurrentMusic                = false;
                UseOnlyCustomMenuSongsFolder    = false;
                PlaybackVolume                  = 0.5f;
            }
        }

        internal class RankedAssistant
        {
            internal static bool Enabled {
                get { return m_Config.GetBool("RankedAssistant", "Enabled", false, true);   }
                set {        m_Config.SetBool("RankedAssistant", "Enabled", value);         }
            }
        }

        internal class SongChartVisualizer
        {
            private static string _s = "SongChartVisualizer";

            internal static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);                      }
                set {        m_Config.SetBool(_s, "Enabled", value);                            }
            }

            internal static bool AlignWithFloor {
                get { return m_Config.GetBool(_s, "AlignWithFloor", true, true);                }
                set {        m_Config.SetBool(_s, "AlignWithFloor", value);                     }
            }
            internal static bool ShowLockIcon {
                get { return m_Config.GetBool(_s, "ShowLockIcon", true, true);                  }
                set {        m_Config.SetBool(_s, "ShowLockIcon", value);                       }
            }
            internal static bool FollowEnvironementRotation {
                get { return m_Config.GetBool(_s, "FollowEnvironementRotation", true, true);    }
                set {        m_Config.SetBool(_s, "FollowEnvironementRotation", value);         }
            }
            internal static bool ShowNPSLegend {
                get { return m_Config.GetBool(_s, "ShowNPSLegend", false, true);                }
                set {        m_Config.SetBool(_s, "ShowNPSLegend", value);                      }
            }

            internal static float BackgroundA { get { return m_Config.GetFloat(_s, "BackgroundA", 0.5f, true); } set { m_Config.SetFloat(_s, "BackgroundA", value); } }
            internal static float BackgroundR { get { return m_Config.GetFloat(_s, "BackgroundR",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundR", value); } }
            internal static float BackgroundG { get { return m_Config.GetFloat(_s, "BackgroundG",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundG", value); } }
            internal static float BackgroundB { get { return m_Config.GetFloat(_s, "BackgroundB",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundB", value); } }
            internal static Color BackgroundColor => new Color(BackgroundR, BackgroundG, BackgroundB, BackgroundA);

            internal static float CursorA { get { return m_Config.GetFloat(_s, "CursorA", 1.00f, true); } set { m_Config.SetFloat(_s, "CursorA", value); } }
            internal static float CursorR { get { return m_Config.GetFloat(_s, "CursorR", 1.00f, true); } set { m_Config.SetFloat(_s, "CursorR", value); } }
            internal static float CursorG { get { return m_Config.GetFloat(_s, "CursorG", 0.03f, true); } set { m_Config.SetFloat(_s, "CursorG", value); } }
            internal static float CursorB { get { return m_Config.GetFloat(_s, "CursorB", 0.00f, true); } set { m_Config.SetFloat(_s, "CursorB", value); } }
            internal static Color CursorColor => new Color(CursorR, CursorG, CursorB, CursorA);

            internal static float LineA { get { return m_Config.GetFloat(_s, "LineA", 0.50f, true); } set { m_Config.SetFloat(_s, "LineA", value); } }
            internal static float LineR { get { return m_Config.GetFloat(_s, "LineR", 0.00f, true); } set { m_Config.SetFloat(_s, "LineR", value); } }
            internal static float LineG { get { return m_Config.GetFloat(_s, "LineG", 0.85f, true); } set { m_Config.SetFloat(_s, "LineG", value); } }
            internal static float LineB { get { return m_Config.GetFloat(_s, "LineB", 0.91f, true); } set { m_Config.SetFloat(_s, "LineB", value); } }
            internal static Color LineColor => new Color(LineR, LineG, LineB, LineA);

            internal static float LegendA { get { return m_Config.GetFloat(_s, "LegendA", 1.00f, true); } set { m_Config.SetFloat(_s, "LegendA", value); } }
            internal static float LegendR { get { return m_Config.GetFloat(_s, "LegendR", 0.37f, true); } set { m_Config.SetFloat(_s, "LegendR", value); } }
            internal static float LegendG { get { return m_Config.GetFloat(_s, "LegendG", 0.10f, true); } set { m_Config.SetFloat(_s, "LegendG", value); } }
            internal static float LegendB { get { return m_Config.GetFloat(_s, "LegendB", 0.86f, true); } set { m_Config.SetFloat(_s, "LegendB", value); } }
            internal static Color LegendColor => new Color(LegendR, LegendG, LegendB, LegendA);

            internal static float DashLineA { get { return m_Config.GetFloat(_s, "DashA", 0.10f, true); } set { m_Config.SetFloat(_s, "DashA", value); } }
            internal static float DashLineR { get { return m_Config.GetFloat(_s, "DashR", 0.05f, true); } set { m_Config.SetFloat(_s, "DashR", value); } }
            internal static float DashLineG { get { return m_Config.GetFloat(_s, "DashG", 0.59f, true); } set { m_Config.SetFloat(_s, "DashG", value); } }
            internal static float DashLineB { get { return m_Config.GetFloat(_s, "DashB", 0.00f, true); } set { m_Config.SetFloat(_s, "DashB", value); } }
            internal static Color DashLineColor => new Color(DashLineR, DashLineG, DashLineB, DashLineA);

            internal static float ChartStandardPositionX { get { return m_Config.GetFloat(_s, "ChartStandardPositionX",     0, true); } set { m_Config.SetFloat(_s, "ChartStandardPositionX", value); } }
            internal static float ChartStandardPositionY { get { return m_Config.GetFloat(_s, "ChartStandardPositionY", -0.4f, true); } set { m_Config.SetFloat(_s, "ChartStandardPositionY", value); } }
            internal static float ChartStandardPositionZ { get { return m_Config.GetFloat(_s, "ChartStandardPositionZ", 2.25f, true); } set { m_Config.SetFloat(_s, "ChartStandardPositionZ", value); } }
            internal static float ChartStandardRotationX { get { return m_Config.GetFloat(_s, "ChartStandardRotationX",   35f, true); } set { m_Config.SetFloat(_s, "ChartStandardRotationX", value); } }
            internal static float ChartStandardRotationY { get { return m_Config.GetFloat(_s, "ChartStandardRotationY",    0f, true); } set { m_Config.SetFloat(_s, "ChartStandardRotationY", value); } }
            internal static float ChartStandardRotationZ { get { return m_Config.GetFloat(_s, "ChartStandardRotationZ",    0f, true); } set { m_Config.SetFloat(_s, "ChartStandardRotationZ", value); } }

            internal static float Chart360_90PositionX { get { return m_Config.GetFloat(_s, "Chart360_90PositionX",     0, true); } set { m_Config.SetFloat(_s, "Chart360_90PositionX", value); } }
            internal static float Chart360_90PositionY { get { return m_Config.GetFloat(_s, "Chart360_90PositionY", 3.50f, true); } set { m_Config.SetFloat(_s, "Chart360_90PositionY", value); } }
            internal static float Chart360_90PositionZ { get { return m_Config.GetFloat(_s, "Chart360_90PositionZ", 3.00f, true); } set { m_Config.SetFloat(_s, "Chart360_90PositionZ", value); } }
            internal static float Chart360_90RotationX { get { return m_Config.GetFloat(_s, "Chart360_90RotationX",  -30f, true); } set { m_Config.SetFloat(_s, "Chart360_90RotationX", value); } }
            internal static float Chart360_90RotationY { get { return m_Config.GetFloat(_s, "Chart360_90RotationY",    0f, true); } set { m_Config.SetFloat(_s, "Chart360_90RotationY", value); } }
            internal static float Chart360_90RotationZ { get { return m_Config.GetFloat(_s, "Chart360_90RotationZ",    0f, true); } set { m_Config.SetFloat(_s, "Chart360_90RotationZ", value); } }

            internal static void Reset()
            {
                AlignWithFloor              = true;
                ShowLockIcon                = true;
                FollowEnvironementRotation  = true;
                ShowNPSLegend               = false;

                BackgroundA = 0.5f;
                BackgroundR = 0.0f;
                BackgroundG = 0.0f;
                BackgroundB = 0.0f;

                CursorA = 1.00f;
                CursorR = 1.00f;
                CursorG = 0.03f;
                CursorB = 0.00f;

                LineA = 0.50f;
                LineR = 0.00f;
                LineG = 0.85f;
                LineB = 0.91f;

                LegendA = 1.00f;
                LegendR = 0.37f;
                LegendG = 0.10f;
                LegendB = 0.86f;

                DashLineA = 0.10f;
                DashLineR = 0.05f;
                DashLineG = 0.59f;
                DashLineB = 0.00f;

                ResetPosition();
            }
            internal static void ResetPosition()
            {
                ChartStandardPositionX =     0;
                ChartStandardPositionY = -0.4f;
                ChartStandardPositionZ = 2.25f;
                ChartStandardRotationX =   35f;
                ChartStandardRotationY =    0f;
                ChartStandardRotationZ =    0f;

                Chart360_90PositionX =     0;
                Chart360_90PositionY = 3.50f;
                Chart360_90PositionZ = 3.00f;
                Chart360_90RotationX =  -30f;
                Chart360_90RotationY =    0f;
                Chart360_90RotationZ =    0f;
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init config
        /// </summary>
        internal static void Init() => m_Config = new SDK.Config.INIConfig("BeatSaberPlus");
    }
}
