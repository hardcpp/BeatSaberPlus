using UnityEngine;
using System.Linq;

namespace BeatSaberPlus
{
    /// <summary>
    /// Config class helper
    /// </summary>
    public class Config
    {
        /// <summary>
        /// Config instance
        /// </summary>
        private static SDK.Config.INIConfig m_Config = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public static bool FirstRun {
            get { return m_Config.GetBool("Config", "FirstRun", true, true);          }
            set {        m_Config.SetBool("Config", "FirstRun", value);               }
        }
        public static bool FirstChatCoreRun {
            get { return m_Config.GetBool("Config", "FirstChatCoreRun", true, true);  }
            set {        m_Config.SetBool("Config", "FirstChatCoreRun", value);       }
        }

        public class Chat
        {
            private  static string _s = "Chat";
            public static string s_ModerationKeyDefault_Split = "#|#";
            private  static string s_ModerationKeyDefault = string.Join(s_ModerationKeyDefault_Split, new string[] {
                "/host",
                "/unban",
                "/untimeout",
                "!bsr"
            });

            public static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", true, true);                      }
                set {        m_Config.SetBool(_s, "Enabled", value);                            }
            }
            public static bool OldConfigMigrated {
                get { return m_Config.GetBool(_s, "OldConfigMigrated", false, true); }
                set {        m_Config.SetBool(_s, "OldConfigMigrated", value);       }
            }

            public static int ChatWidth {
                get { return m_Config.GetInt(_s, "ChatWidth", 120, true);                       }
                set {        m_Config.SetInt(_s, "ChatWidth", value);                           }
            }
            public static int ChatHeight {
                get { return m_Config.GetInt(_s, "ChatHeight", 140, true);                      }
                set {        m_Config.SetInt(_s, "ChatHeight", value);                          }
            }
            public static bool ReverseChatOrder {
                get { return m_Config.GetBool(_s, "ReverseChatOrder", false, true);             }
                set {        m_Config.SetBool(_s, "ReverseChatOrder", value);                   }
            }
            public static string SystemFontName {
                get { return m_Config.GetString(_s, "SystemFontName", "Segoe UI", true);        }
                set {        m_Config.SetString(_s, "SystemFontName", value);                   }
            }
            public static float FontSize {
                get { return m_Config.GetFloat(_s, "FontSize", 3.4f, true);                     }
                set {        m_Config.SetFloat(_s, "FontSize", value);                          }
            }

            public static bool AlignWithFloor {
                get { return m_Config.GetBool(_s, "AlignWithFloor", true, true);                }
                set {        m_Config.SetBool(_s, "AlignWithFloor", value);                     }
            }
            public static bool ShowLockIcon {
                get { return m_Config.GetBool(_s, "ShowLockIcon", true, true);                  }
                set {        m_Config.SetBool(_s, "ShowLockIcon", value);                       }
            }
            public static bool FollowEnvironementRotation {
                get { return m_Config.GetBool(_s, "FollowEnvironementRotation", true, true);    }
                set {        m_Config.SetBool(_s, "FollowEnvironementRotation", value);         }
            }
            public static bool ShowViewerCount {
                get { return m_Config.GetBool(_s, "ShowViewerCount", true, true);               }
                set {        m_Config.SetBool(_s, "ShowViewerCount", value);                    }
            }
            public static bool ShowFollowEvents {
                get { return m_Config.GetBool(_s, "ShowFollowEvents", true, true);              }
                set {        m_Config.SetBool(_s, "ShowFollowEvents", value);                   }
            }
            public static bool ShowSubscriptionEvents {
                get { return m_Config.GetBool(_s, "ShowSubscriptionEvents", true, true);        }
                set {        m_Config.SetBool(_s, "ShowSubscriptionEvents", value);             }
            }
            public static bool ShowBitsCheeringEvents {
                get { return m_Config.GetBool(_s, "ShowBitsCheeringEvents", true, true);        }
                set {        m_Config.SetBool(_s, "ShowBitsCheeringEvents", value);             }
            }
            public static bool ShowChannelPointsEvent {
                get { return m_Config.GetBool(_s, "ShowChannelPointsEvent", true, true);        }
                set {        m_Config.SetBool(_s, "ShowChannelPointsEvent", value);             }
            }
            public static bool FilterViewersCommands {
                get { return m_Config.GetBool(_s, "FilterViewersCommands", false, true);        }
                set {        m_Config.SetBool(_s, "FilterViewersCommands", value);              }
            }
            public static bool FilterBroadcasterCommands {
                get { return m_Config.GetBool(_s, "FilterBroadcasterCommands", false, true);    }
                set {        m_Config.SetBool(_s, "FilterBroadcasterCommands", value);          }
            }

            public static float BackgroundA { get { return m_Config.GetFloat(_s, "BackgroundA", 0.5f, true); } set { m_Config.SetFloat(_s, "BackgroundA", value); } }
            public static float BackgroundR { get { return m_Config.GetFloat(_s, "BackgroundR",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundR", value); } }
            public static float BackgroundG { get { return m_Config.GetFloat(_s, "BackgroundG",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundG", value); } }
            public static float BackgroundB { get { return m_Config.GetFloat(_s, "BackgroundB",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundB", value); } }
            public static Color BackgroundColor => new Color(BackgroundR, BackgroundG, BackgroundB, BackgroundA);

            public static float HighlightA { get { return m_Config.GetFloat(_s, "HighlightA", 0.12f, true); } set { m_Config.SetFloat(_s, "HighlightA", value); } }
            public static float HighlightR { get { return m_Config.GetFloat(_s, "HighlightR", 0.57f, true); } set { m_Config.SetFloat(_s, "HighlightR", value); } }
            public static float HighlightG { get { return m_Config.GetFloat(_s, "HighlightG", 0.28f, true); } set { m_Config.SetFloat(_s, "HighlightG", value); } }
            public static float HighlightB { get { return m_Config.GetFloat(_s, "HighlightB",    1f, true); } set { m_Config.SetFloat(_s, "HighlightB", value); } }
            public static Color HighlightColor => new Color(HighlightR, HighlightG, HighlightB, HighlightA);

            public static float AccentA { get { return m_Config.GetFloat(_s, "AccentA", 1.00f, true); } set { m_Config.SetFloat(_s, "AccentA", value); } }
            public static float AccentR { get { return m_Config.GetFloat(_s, "AccentR", 0.57f, true); } set { m_Config.SetFloat(_s, "AccentR", value); } }
            public static float AccentG { get { return m_Config.GetFloat(_s, "AccentG", 0.28f, true); } set { m_Config.SetFloat(_s, "AccentG", value); } }
            public static float AccentB { get { return m_Config.GetFloat(_s, "AccentB",    1f, true); } set { m_Config.SetFloat(_s, "AccentB", value); } }
            public static Color AccentColor => new Color(AccentR, AccentG, AccentB, AccentA);

            public static float TextA { get { return m_Config.GetFloat(_s, "TextA", 1f, true); } set { m_Config.SetFloat(_s, "TextA", value); } }
            public static float TextR { get { return m_Config.GetFloat(_s, "TextR", 1f, true); } set { m_Config.SetFloat(_s, "TextR", value); } }
            public static float TextG { get { return m_Config.GetFloat(_s, "TextG", 1f, true); } set { m_Config.SetFloat(_s, "TextG", value); } }
            public static float TextB { get { return m_Config.GetFloat(_s, "TextB", 1f, true); } set { m_Config.SetFloat(_s, "TextB", value); } }
            public static Color TextColor => new Color(TextR, TextG, TextB, TextA);

            public static float PingA { get { return m_Config.GetFloat(_s, "PingA", 0.18f, true); } set { m_Config.SetFloat(_s, "PingA", value); } }
            public static float PingR { get { return m_Config.GetFloat(_s, "PingR", 1.00f, true); } set { m_Config.SetFloat(_s, "PingR", value); } }
            public static float PingG { get { return m_Config.GetFloat(_s, "PingG", 0.00f, true); } set { m_Config.SetFloat(_s, "PingG", value); } }
            public static float PingB { get { return m_Config.GetFloat(_s, "PingB", 0.00f, true); } set { m_Config.SetFloat(_s, "PingB", value); } }
            public static Color PingColor => new Color(PingR, PingG, PingB, PingA);

            public static float MenuChatPositionX { get { return m_Config.GetFloat(_s, "MenuChatPositionX",     0, true); } set { m_Config.SetFloat(_s, "MenuChatPositionX", value); } }
            public static float MenuChatPositionY { get { return m_Config.GetFloat(_s, "MenuChatPositionY", 3.87f, true); } set { m_Config.SetFloat(_s, "MenuChatPositionY", value); } }
            public static float MenuChatPositionZ { get { return m_Config.GetFloat(_s, "MenuChatPositionZ", 2.50f, true); } set { m_Config.SetFloat(_s, "MenuChatPositionZ", value); } }
            public static float MenuChatRotationX { get { return m_Config.GetFloat(_s, "MenuChatRotationX",  325f, true); } set { m_Config.SetFloat(_s, "MenuChatRotationX", value); } }
            public static float MenuChatRotationY { get { return m_Config.GetFloat(_s, "MenuChatRotationY",    0f, true); } set { m_Config.SetFloat(_s, "MenuChatRotationY", value); } }
            public static float MenuChatRotationZ { get { return m_Config.GetFloat(_s, "MenuChatRotationZ",    0f, true); } set { m_Config.SetFloat(_s, "MenuChatRotationZ", value); } }

            public static float PlayingChatPositionX { get { return m_Config.GetFloat(_s, "PlayingChatPositionX",     0, true); } set { m_Config.SetFloat(_s, "PlayingChatPositionX", value); } }
            public static float PlayingChatPositionY { get { return m_Config.GetFloat(_s, "PlayingChatPositionY", 3.75f, true); } set { m_Config.SetFloat(_s, "PlayingChatPositionY", value); } }
            public static float PlayingChatPositionZ { get { return m_Config.GetFloat(_s, "PlayingChatPositionZ", 2.50f, true); } set { m_Config.SetFloat(_s, "PlayingChatPositionZ", value); } }
            public static float PlayingChatRotationX { get { return m_Config.GetFloat(_s, "PlayingChatRotationX",  325f, true); } set { m_Config.SetFloat(_s, "PlayingChatRotationX", value); } }
            public static float PlayingChatRotationY { get { return m_Config.GetFloat(_s, "PlayingChatRotationY",    0f, true); } set { m_Config.SetFloat(_s, "PlayingChatRotationY", value); } }
            public static float PlayingChatRotationZ { get { return m_Config.GetFloat(_s, "PlayingChatRotationZ",    0f, true); } set { m_Config.SetFloat(_s, "PlayingChatRotationZ", value); } }

            public static string ModerationKeys {
                get { return m_Config.GetString(_s, "ModerationKeys", s_ModerationKeyDefault, true); }
                set {        m_Config.SetString(_s, "ModerationKeys", value);                        }
            }
        }

        public class ChatEmoteRain
        {
            private static string _s = "ChatEmoteRain";

            public static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", true, false);         }
                set {        m_Config.SetBool(_s, "Enabled", value);                }
            }
            public static string Prefix => "!er";

            public static bool MenuRain {
                get { return m_Config.GetBool(_s, "MenuRain", true, false);         }
                set {        m_Config.SetBool(_s, "MenuRain", value);               }
            }
            public static float MenuRainSize {
                get { return m_Config.GetFloat(_s, "MenuRainSize", 0.4f, false);    }
                set {        m_Config.SetFloat(_s, "MenuRainSize", value);          }
            }
            public static float MenuFallSpeed {
                get { return m_Config.GetFloat(_s, "MenuFallSpeed",   3f, false);  }
                set {        m_Config.SetFloat(_s, "MenuFallSpeed", value);        }
            }

            public static bool SongRain {
                get { return m_Config.GetBool(_s, "SongRain", true, false);         }
                set {        m_Config.SetBool(_s, "SongRain", value);               }
            }
            public static float SongRainSize {
                get { return m_Config.GetFloat(_s, "SongRainSize", 0.6f, false);    }
                set {        m_Config.SetFloat(_s, "SongRainSize", value);          }
            }
            public static float SongFallSpeed {
                get { return m_Config.GetFloat(_s, "SongFallSpeed",   3f, false);   }
                set {        m_Config.SetFloat(_s, "SongFallSpeed", value);         }
            }

            public static bool ModeratorPower {
                get { return m_Config.GetBool(_s, "ModeratorPower", true, false);   }
                set {        m_Config.SetBool(_s, "ModeratorPower", value);         }
            }
            public static bool VIPPower {
                get { return m_Config.GetBool(_s, "VIPPower", false, false);        }
                set {        m_Config.SetBool(_s, "VIPPower", value);               }
            }
            public static bool SubscriberPower {
                get { return m_Config.GetBool(_s, "SubscriberPower", false, false); }
                set {        m_Config.SetBool(_s, "SubscriberPower", value);        }
            }

            public static int EmoteDelay {
                get { return m_Config.GetInt(_s, "EmoteDelay", 8, false);           }
                set {        m_Config.SetInt(_s, "EmoteDelay", value);              }
            }

            public static bool SubRain {
                get { return m_Config.GetBool(_s, "SubRain", true, false);          }
                set {        m_Config.SetBool(_s, "SubRain", value);                }
            }
            public static int SubRainEmoteCount {
                get { return m_Config.GetInt(_s, "SubRainEmoteCount", 20, false);   }
                set {        m_Config.SetInt(_s, "SubRainEmoteCount", value);       }
            }

            public static bool ComboMode {
                get { return m_Config.GetBool(_s, "ComboMode", false, false);       }
                set {        m_Config.SetBool(_s, "ComboMode", value);              }
            }
            public static int ComboModeType {
                get { return m_Config.GetInt(_s, "ComboModeType", 0, false);        }
                set {        m_Config.SetInt(_s, "ComboModeType", value);           }
            }
            public static float ComboTimer {
                get { return m_Config.GetFloat(_s, "ComboTimer", 5f, false);        }
                set {        m_Config.SetFloat(_s, "ComboTimer", value);            }
            }
            public static int ComboCount {
                get { return m_Config.GetInt(_s, "ComboCount", 3, false);           }
                set {        m_Config.SetInt(_s, "ComboCount", value);              }
            }
        }

        public class ChatIntegrations
        {
            private static string _s = "ChatIntegrations";

            public static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);                              }
                set {        m_Config.SetBool(_s, "Enabled", value);                                    }
            }
            public static bool OldConfigMigrated {
                get { return m_Config.GetBool(_s, "OldConfigMigrated", false, true); }
                set {        m_Config.SetBool(_s, "OldConfigMigrated", value);       }
            }
        }

        public class ChatRequest
        {
            private static string _s = "ChatRequest";

            public static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", true, true);           }
                set {        m_Config.SetBool(_s, "Enabled", value);                 }
            }
            public static bool OldConfigMigrated {
                get { return m_Config.GetBool(_s, "OldConfigMigrated", false, true); }
                set {        m_Config.SetBool(_s, "OldConfigMigrated", value);       }
            }

            public static bool QueueOpen {
                get { return m_Config.GetBool(_s, "QueueOpen", true, true);          }
                set {        m_Config.SetBool(_s, "QueueOpen", value);               }
            }

            public static int UserMaxRequest {
                get { return m_Config.GetInt(_s, "UserMaxRequest", 2, true);         }
                set {        m_Config.SetInt(_s, "UserMaxRequest", value);           }
            }
            public static int VIPBonusRequest {
                get { return m_Config.GetInt(_s, "VIPBonusRequest", 2, true);        }
                set {        m_Config.SetInt(_s, "VIPBonusRequest", value);          }
            }
            public static int SubscriberBonusRequest {
                get { return m_Config.GetInt(_s, "SubscriberBonusRequest", 3, true); }
                set {        m_Config.SetInt(_s, "SubscriberBonusRequest", value);   }
            }

            public static int HistorySize {
                get { return m_Config.GetInt(_s, "HistorySize", 50, true);           }
                set {        m_Config.SetInt(_s, "HistorySize", value);              }
            }
            public static bool PlayPreviewMusic {
                get { return m_Config.GetBool(_s, "PlayPreviewMusic", true, true);   }
                set {        m_Config.SetBool(_s, "PlayPreviewMusic", value);        }
            }
            public static bool ModeratorPower {
                get { return m_Config.GetBool(_s, "ModeratorPower", true, true);     }
                set {        m_Config.SetBool(_s, "ModeratorPower", value);          }
            }

            public static int QueueCommandShowSize {
                get { return m_Config.GetInt(_s, "QueueCommandSize", 4, true);       }
                set {        m_Config.SetInt(_s, "QueueCommandSize", value);         }
            }
            public static int QueueCommandCooldown {
                get { return m_Config.GetInt(_s, "QueueCommandCooldown", 10, true);  }
                set {        m_Config.SetInt(_s, "QueueCommandCooldown", value);     }
            }

            public static bool NoBeatSage {
                get { return m_Config.GetBool(_s, "NoBeatSage", false, true);   }
                set {        m_Config.SetBool(_s, "NoBeatSage", value);         }
            }
            public static bool NPSMin {
                get { return m_Config.GetBool(_s, "NPSMin", false, true);       }
                set {        m_Config.SetBool(_s, "NPSMin", value);             }
            }
            public static bool NPSMax {
                get { return m_Config.GetBool(_s, "NPSMax", false, true);       }
                set {        m_Config.SetBool(_s, "NPSMax", value);             }
            }
            public static bool NJSMin {
                get { return m_Config.GetBool(_s, "NJSMin", false, true);       }
                set {        m_Config.SetBool(_s, "NJSMin", value);             }
            }
            public static bool NJSMax {
                get { return m_Config.GetBool(_s, "NJSMax", false, true);       }
                set {        m_Config.SetBool(_s, "NJSMax", value);             }
            }
            public static bool DurationMax {
                get { return m_Config.GetBool(_s, "DurationMax", false, true);  }
                set {        m_Config.SetBool(_s, "DurationMax", value);        }
            }
            public static bool VoteMin {
                get { return m_Config.GetBool(_s, "VoteMin", false, true);      }
                set {        m_Config.SetBool(_s, "VoteMin", value);            }
            }
            public static bool DateMin {
                get { return m_Config.GetBool(_s, "DateMin", false, true);      }
                set {        m_Config.SetBool(_s, "DateMin", value);            }
            }
            public static bool DateMax {
                get { return m_Config.GetBool(_s, "DateMax", false, true);      }
                set {        m_Config.SetBool(_s, "DateMax", value);            }
            }

            public static int NPSMinV {
                get { return m_Config.GetInt(_s, "NPSMinV", 0, true);           }
                set {        m_Config.SetInt(_s, "NPSMinV", value);             }
            }
            public static int NPSMaxV {
                get { return m_Config.GetInt(_s, "NPSMaxV", 30, true);          }
                set {        m_Config.SetInt(_s, "NPSMaxV", value);             }
            }
            public static int NJSMinV {
                get { return m_Config.GetInt(_s, "NJSMinV", 0, true);           }
                set {        m_Config.SetInt(_s, "NJSMinV", value);             }
            }
            public static int NJSMaxV {
                get { return m_Config.GetInt(_s, "NJSMaxV", 30, true);          }
                set {        m_Config.SetInt(_s, "NJSMaxV", value);             }
            }
            public static int DurationMaxV {
                get { return m_Config.GetInt(_s, "DurationMaxV", 3, true);      }
                set {        m_Config.SetInt(_s, "DurationMaxV", value);        }
            }
            public static float VoteMinV {
                get { return m_Config.GetFloat(_s, "VoteMinV", 0.5f, true);     }
                set {        m_Config.SetFloat(_s, "VoteMinV", value);          }
            }
            public static int DateMinV {
                get { return m_Config.GetInt(_s, "DateMinV", 0, true);          }
                set {        m_Config.SetInt(_s, "DateMinV", value);            }
            }
            public static int DateMaxV {
                get { return m_Config.GetInt(_s, "DateMaxV", 36, true);         }
                set {        m_Config.SetInt(_s, "DateMaxV", value);            }
            }

            public static string SimpleQueueFileFormat {
                get { return m_Config.GetString(_s, "SimpleQueueFileFormat", "%i - %n by %m", true);    }
                set {        m_Config.SetString(_s, "SimpleQueueFileFormat", value);                    }
            }
            public static int SimpleQueueFileCount {
                get { return m_Config.GetInt(_s, "SimpleQueueFileCount", 10, true);                     }
                set {        m_Config.SetInt(_s, "SimpleQueueFileCount", value);                        }
            }
        }

        public class GameTweaker
        {
            private static string _s = "GameTweaker";

            public static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);                              }
                set {        m_Config.SetBool(_s, "Enabled", value);                                    }
            }
            public static bool OldConfigMigrated
            {
                get { return m_Config.GetBool(_s, "OldConfigMigrated", false, true); }
                set { m_Config.SetBool(_s, "OldConfigMigrated", value); }
            }

            public static bool RemoveDebris {
                get { return m_Config.GetBool(_s, "RemoveDebris", false, true);                         }
                set {        m_Config.SetBool(_s, "RemoveDebris", value);                               }
            }
            public static bool RemoveAllCutParticles {
                get { return m_Config.GetBool(_s, "RemoveAllCutParticles", m_Config.GetBool(_s, "RemoveCutRibbon", false, false), true); }
                set {        m_Config.SetBool(_s, "RemoveAllCutParticles", value);                      }
            }
            public static bool RemoveObstacleParticles {
                get { return m_Config.GetBool(_s, "RemoveObstacleParticles", false, true);              }
                set {        m_Config.SetBool(_s, "RemoveObstacleParticles", value);                    }
            }
            public static bool RemoveSaberBurnMarks {
                get { return m_Config.GetBool(_s, "RemoveSaberBurnMarks", false, true);                 }
                set {        m_Config.SetBool(_s, "RemoveSaberBurnMarks", value);                       }
            }
            public static bool RemoveSaberBurnMarkSparkles {
                get { return m_Config.GetBool(_s, "RemoveSaberBurnMarkSparkles", false, true);          }
                set {        m_Config.SetBool(_s, "RemoveSaberBurnMarkSparkles", value);                }
            }
            public static bool RemoveSaberClashEffects {
                get { return m_Config.GetBool(_s, "RemoveSaberClashEffects", false, true);              }
                set {        m_Config.SetBool(_s, "RemoveSaberClashEffects", value);                    }
            }
            public static bool RemoveWorldParticles {
                get { return m_Config.GetBool(_s, "RemoveWorldParticles", false, true);                 }
                set {        m_Config.SetBool(_s, "RemoveWorldParticles", value);                       }
            }

            public static bool RemoveMusicBandLogo {
                get { return m_Config.GetBool(_s, "RemoveMusicBandLogo", false, true);                  }
                set {        m_Config.SetBool(_s, "RemoveMusicBandLogo", value);                        }
            }
            public static bool RemoveFullComboLossAnimation {
                get { return m_Config.GetBool(_s, "RemoveFullComboLossAnimation", false, true);         }
                set {        m_Config.SetBool(_s, "RemoveFullComboLossAnimation", value);               }
            }
            public static bool NoFake360HUD {
                get { return m_Config.GetBool(_s, "NoFake360HUD", false, true);                         }
                set {        m_Config.SetBool(_s, "NoFake360HUD", value);                               }
            }

            ////////////////////////////////////////////////////////////////////////////
            /// Menu
            ////////////////////////////////////////////////////////////////////////////

            /// Main menu
            public static bool DisableBeatMapEditorButtonOnMainMenu {
                get { return m_Config.GetBool(_s, "DisableBeatMapEditorButtonOnMainMenu", false, true); }
                set {        m_Config.SetBool(_s, "DisableBeatMapEditorButtonOnMainMenu", value);       }
            }
            public static bool RemoveNewContentPromotional {
                get { return m_Config.GetBool(_s, "RemoveNewContentPromotional", true, true);           }
                set {        m_Config.SetBool(_s, "RemoveNewContentPromotional", value);                }
            }

            /// Level selection
            public static bool RemoveBaseGameFilterButton {
                get { return m_Config.GetBool(_s, "RemoveBaseGameFilterButton", true, true);            }
                set {        m_Config.SetBool(_s, "RemoveBaseGameFilterButton", value);                 }
            }
            public static bool ReorderPlayerSettings {
                get { return m_Config.GetBool(_s, "ReorderPlayerSettings", true, true);                 }
                set {        m_Config.SetBool(_s, "ReorderPlayerSettings", value);                      }
            }
            public static bool AddOverrideLightIntensityOption {
                get { return m_Config.GetBool(_s, "AddOverrideLightIntensityOption", false, true);      }
                set {        m_Config.SetBool(_s, "AddOverrideLightIntensityOption", value);            }
            }
            public static bool MergeLightPressetOptions {
                get { return m_Config.GetBool(_s, "MergeLightPressetOptions", true, true);  }
                set {        m_Config.SetBool(_s, "MergeLightPressetOptions", value);       }
            }
            public static float OverrideLightIntensity {
                get { return m_Config.GetFloat(_s, "OverrideLightIntensity", 1.0f, true);               }
                set {        m_Config.SetFloat(_s, "OverrideLightIntensity", value);                    }
            }
            public static bool DeleteSongButton {
                get { return m_Config.GetBool(_s, "DeleteSongButton", true, true);                      }
                set {        m_Config.SetBool(_s, "DeleteSongButton", value);                           }
            }
            public static bool DeleteSongBrowserTrashcan
            {
                get { return m_Config.GetBool(_s, "DeleteSongBrowserTrashcan", false, true); }
                set {        m_Config.SetBool(_s, "DeleteSongBrowserTrashcan", value);       }
            }
            public static bool HighlightPlayedSong
            {
                get { return m_Config.GetBool(_s, "HighlightPlayedSong", true, true);  }
                set {        m_Config.SetBool(_s, "HighlightPlayedSong", value);       }
            }


            ////////////////////////////////////////////////////////////////////////////
            /// Dev / Testing
            ////////////////////////////////////////////////////////////////////////////

            /// Logs
            public static bool RemoveOldLogs {
                get { return m_Config.GetBool(_s, "RemoveOldLogs", true, true);                         }
                set {        m_Config.SetBool(_s, "RemoveOldLogs", value);                              }
            }
            public static int LogEntriesToKeep {
                get { return m_Config.GetInt(_s, "LogEntriesToKeep", 8, true);                          }
                set {        m_Config.SetInt(_s, "LogEntriesToKeep", value);                            }
            }

            /// FPFC escape
            public static bool FPFCEscape {
                get { return m_Config.GetBool(_s, "FPFCEscape", false, true);                           }
                set {        m_Config.SetBool(_s, "FPFCEscape", value);                                 }
            }
        }

        public class MenuMusic
        {
            private static string _s = "MenuMusic";

            public static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);                          }
                set {        m_Config.SetBool(_s, "Enabled", value);                                }
            }
            public static bool OldConfigMigrated {
                get { return m_Config.GetBool(_s, "OldConfigMigrated", false, true); }
                set {        m_Config.SetBool(_s, "OldConfigMigrated", value);       }
            }

            public static bool ShowPlayer {
                get { return m_Config.GetBool(_s, "ShowPlayer", true, true);                        }
                set {        m_Config.SetBool(_s, "ShowPlayer", value);                             }
            }
            public static bool ShowPlayTime {
                get { return m_Config.GetBool(_s, "ShowPlayTime", true, true);                      }
                set {        m_Config.SetBool(_s, "ShowPlayTime", value);                           }
            }
            public static float BackgroundA { get { return m_Config.GetFloat(_s, "BackgroundA", 0.5f, true); } set { m_Config.SetFloat(_s, "BackgroundA", value); } }
            public static float BackgroundR { get { return m_Config.GetFloat(_s, "BackgroundR", 0f, true);   } set { m_Config.SetFloat(_s, "BackgroundR", value); } }
            public static float BackgroundG { get { return m_Config.GetFloat(_s, "BackgroundG", 0f, true);   } set { m_Config.SetFloat(_s, "BackgroundG", value); } }
            public static float BackgroundB { get { return m_Config.GetFloat(_s, "BackgroundB", 0f, true);   } set { m_Config.SetFloat(_s, "BackgroundB", value); } }
            public static Color BackgroundColor => new Color(BackgroundR, BackgroundG, BackgroundB, BackgroundA);

            public static bool StartSongFromBeginning {
                get { return m_Config.GetBool(_s, "StartSongFromBeginning", false, true);           }
                set {        m_Config.SetBool(_s, "StartSongFromBeginning", value);                 }
            }
            public static bool StartANewMusicOnSceneChange {
                get { return m_Config.GetBool(_s, "StartANewMusicOnSceneChange", true, true);       }
                set {        m_Config.SetBool(_s, "StartANewMusicOnSceneChange", value);            }
            }
            public static bool LoopCurrentMusic {
                get { return m_Config.GetBool(_s, "LoopCurrentMusic", false, true);                 }
                set {        m_Config.SetBool(_s, "LoopCurrentMusic", value);                       }
            }
            public static bool UseOnlyCustomMenuSongsFolder {
                get { return m_Config.GetBool(_s, "UseOnlyCustomMenuSongsFolder", false, true);     }
                set {        m_Config.SetBool(_s, "UseOnlyCustomMenuSongsFolder", value);           }
            }
            public static float PlaybackVolume {
                get { return m_Config.GetFloat(_s, "PlaybackVolume", 0.5f, true);                   }
                set {        m_Config.SetFloat(_s, "PlaybackVolume", value);                        }
            }
        }

        public class NoteTweaker
        {
            private static string _s = "NoteTweaker";

            public static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);              }
                set {        m_Config.SetBool(_s, "Enabled", value);                    }
            }
            public static bool OldConfigMigrated {
                get { return m_Config.GetBool(_s, "OldConfigMigrated", false, true); }
                set {        m_Config.SetBool(_s, "OldConfigMigrated", value);       }
            }

            public static bool ShowDotsWithArrow {
                get { return m_Config.GetBool(_s, "ShowDotsWithArrow", true, true);     }
                set {        m_Config.SetBool(_s, "ShowDotsWithArrow", value);          }
            }
            public static bool OverrideArrowColors {
                get { return m_Config.GetBool(_s, "OverrideArrowColors", false, true);  }
                set {        m_Config.SetBool(_s, "OverrideArrowColors", value);        }
            }
            public static bool OverrideDotColors {
                get { return m_Config.GetBool(_s, "OverrideDotColors", false, true);    }
                set {        m_Config.SetBool(_s, "OverrideDotColors", value);          }
            }
            public static float Scale {
                get { return m_Config.GetFloat(_s, "Scale", 0.9f, true);                }
                set {        m_Config.SetFloat(_s, "Scale", value);                     }
            }

            public static float ArrowScale {
                get { return m_Config.GetFloat(_s, "ArrowScale", 1.0f, true);           }
                set {        m_Config.SetFloat(_s, "ArrowScale", value);                }
            }
            public static float ArrowA  { get { return m_Config.GetFloat(_s, "ArrowA", 1.00f, true);  } set { m_Config.SetFloat(_s, "ArrowA", value);  } }
            public static float ArrowLR { get { return m_Config.GetFloat(_s, "ArrowLR", 0.12f, true); } set { m_Config.SetFloat(_s, "ArrowLR", value); } }
            public static float ArrowLG { get { return m_Config.GetFloat(_s, "ArrowLG", 0.75f, true); } set { m_Config.SetFloat(_s, "ArrowLG", value); } }
            public static float ArrowLB { get { return m_Config.GetFloat(_s, "ArrowLB", 1.00f, true); } set { m_Config.SetFloat(_s, "ArrowLB", value); } }
            public static float ArrowRR { get { return m_Config.GetFloat(_s, "ArrowRR", 0.12f, true); } set { m_Config.SetFloat(_s, "ArrowRR", value); } }
            public static float ArrowRG { get { return m_Config.GetFloat(_s, "ArrowRG", 0.75f, true); } set { m_Config.SetFloat(_s, "ArrowRG", value); } }
            public static float ArrowRB { get { return m_Config.GetFloat(_s, "ArrowRB", 1.00f, true); } set { m_Config.SetFloat(_s, "ArrowRB", value); } }
            public static Color ArrowLColor => new Color(ArrowLR, ArrowLG, ArrowLB, ArrowA);
            public static Color ArrowRColor => new Color(ArrowRR, ArrowRG, ArrowRB, ArrowA);

            public static float DotScale {
                get { return m_Config.GetFloat(_s, "DotScale", 0.85f, true);            }
                set {        m_Config.SetFloat(_s, "DotScale", value);                  }
            }
            public static float DotA  { get { return m_Config.GetFloat(_s, "DotA", 1.00f, true);  } set { m_Config.SetFloat(_s, "DotA", value);  } }
            public static float DotLR { get { return m_Config.GetFloat(_s, "DotLR", 0.12f, true); } set { m_Config.SetFloat(_s, "DotLR", value); } }
            public static float DotLG { get { return m_Config.GetFloat(_s, "DotLG", 0.75f, true); } set { m_Config.SetFloat(_s, "DotLG", value); } }
            public static float DotLB { get { return m_Config.GetFloat(_s, "DotLB", 1.00f, true); } set { m_Config.SetFloat(_s, "DotLB", value); } }
            public static float DotRR { get { return m_Config.GetFloat(_s, "DotRR", 0.12f, true); } set { m_Config.SetFloat(_s, "DotRR", value); } }
            public static float DotRG { get { return m_Config.GetFloat(_s, "DotRG", 0.75f, true); } set { m_Config.SetFloat(_s, "DotRG", value); } }
            public static float DotRB { get { return m_Config.GetFloat(_s, "DotRB", 1.00f, true); } set { m_Config.SetFloat(_s, "DotRB", value); } }
            public static Color DotLColor => new Color(DotLR, DotLG, DotLB, DotA);
            public static Color DotRColor => new Color(DotRR, DotRG, DotRB, DotA);

            public static bool OverrideBombColor {
                get { return m_Config.GetBool(_s, "OverrideBombColor", false, true);    }
                set {        m_Config.SetBool(_s, "OverrideBombColor", value);          }
            }
            public static float BombR { get { return m_Config.GetFloat(_s, "BombR", 1.0000f, true); } set { m_Config.SetFloat(_s, "BombR", value); } }
            public static float BombG { get { return m_Config.GetFloat(_s, "BombG", 0.0000f, true); } set { m_Config.SetFloat(_s, "BombG", value); } }
            public static float BombB { get { return m_Config.GetFloat(_s, "BombB", 0.6469f, true); } set { m_Config.SetFloat(_s, "BombB", value); } }
            public static Color BombColor => new Color(BombR, BombG, BombB, 1f);

            public static float PrecisionDotScale {
                get { return m_Config.GetFloat(_s, "PrecisionDotScale", 0.40f, true);   }
                set {        m_Config.SetFloat(_s, "PrecisionDotScale", value);         }
            }
        }

        public class SongChartVisualizer
        {
            private static string _s = "SongChartVisualizer";

            public static bool Enabled {
                get { return m_Config.GetBool(_s, "Enabled", false, true);                      }
                set {        m_Config.SetBool(_s, "Enabled", value);                            }
            }
            public static bool OldConfigMigrated {
                get { return m_Config.GetBool(_s, "OldConfigMigrated", false, true); }
                set {        m_Config.SetBool(_s, "OldConfigMigrated", value);       }
            }

            public static bool AlignWithFloor {
                get { return m_Config.GetBool(_s, "AlignWithFloor", true, true);                }
                set {        m_Config.SetBool(_s, "AlignWithFloor", value);                     }
            }
            public static bool ShowLockIcon {
                get { return m_Config.GetBool(_s, "ShowLockIcon", true, true);                  }
                set {        m_Config.SetBool(_s, "ShowLockIcon", value);                       }
            }
            public static bool FollowEnvironementRotation {
                get { return m_Config.GetBool(_s, "FollowEnvironementRotation", true, true);    }
                set {        m_Config.SetBool(_s, "FollowEnvironementRotation", value);         }
            }
            public static bool ShowNPSLegend {
                get { return m_Config.GetBool(_s, "ShowNPSLegend", false, true);                }
                set {        m_Config.SetBool(_s, "ShowNPSLegend", value);                      }
            }

            public static float BackgroundA { get { return m_Config.GetFloat(_s, "BackgroundA", 0.5f, true); } set { m_Config.SetFloat(_s, "BackgroundA", value); } }
            public static float BackgroundR { get { return m_Config.GetFloat(_s, "BackgroundR",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundR", value); } }
            public static float BackgroundG { get { return m_Config.GetFloat(_s, "BackgroundG",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundG", value); } }
            public static float BackgroundB { get { return m_Config.GetFloat(_s, "BackgroundB",   0f, true); } set { m_Config.SetFloat(_s, "BackgroundB", value); } }
            public static Color BackgroundColor => new Color(BackgroundR, BackgroundG, BackgroundB, BackgroundA);

            public static float CursorA { get { return m_Config.GetFloat(_s, "CursorA", 1.00f, true); } set { m_Config.SetFloat(_s, "CursorA", value); } }
            public static float CursorR { get { return m_Config.GetFloat(_s, "CursorR", 1.00f, true); } set { m_Config.SetFloat(_s, "CursorR", value); } }
            public static float CursorG { get { return m_Config.GetFloat(_s, "CursorG", 0.03f, true); } set { m_Config.SetFloat(_s, "CursorG", value); } }
            public static float CursorB { get { return m_Config.GetFloat(_s, "CursorB", 0.00f, true); } set { m_Config.SetFloat(_s, "CursorB", value); } }
            public static Color CursorColor => new Color(CursorR, CursorG, CursorB, CursorA);

            public static float LineA { get { return m_Config.GetFloat(_s, "LineA", 0.50f, true); } set { m_Config.SetFloat(_s, "LineA", value); } }
            public static float LineR { get { return m_Config.GetFloat(_s, "LineR", 0.00f, true); } set { m_Config.SetFloat(_s, "LineR", value); } }
            public static float LineG { get { return m_Config.GetFloat(_s, "LineG", 0.85f, true); } set { m_Config.SetFloat(_s, "LineG", value); } }
            public static float LineB { get { return m_Config.GetFloat(_s, "LineB", 0.91f, true); } set { m_Config.SetFloat(_s, "LineB", value); } }
            public static Color LineColor => new Color(LineR, LineG, LineB, LineA);

            public static float LegendA { get { return m_Config.GetFloat(_s, "LegendA", 1.00f, true); } set { m_Config.SetFloat(_s, "LegendA", value); } }
            public static float LegendR { get { return m_Config.GetFloat(_s, "LegendR", 0.37f, true); } set { m_Config.SetFloat(_s, "LegendR", value); } }
            public static float LegendG { get { return m_Config.GetFloat(_s, "LegendG", 0.10f, true); } set { m_Config.SetFloat(_s, "LegendG", value); } }
            public static float LegendB { get { return m_Config.GetFloat(_s, "LegendB", 0.86f, true); } set { m_Config.SetFloat(_s, "LegendB", value); } }
            public static Color LegendColor => new Color(LegendR, LegendG, LegendB, LegendA);

            public static float DashLineA { get { return m_Config.GetFloat(_s, "DashA", 1.00f, true); } set { m_Config.SetFloat(_s, "DashA", value); } }
            public static float DashLineR { get { return m_Config.GetFloat(_s, "DashR", 0.37f, true); } set { m_Config.SetFloat(_s, "DashR", value); } }
            public static float DashLineG { get { return m_Config.GetFloat(_s, "DashG", 0.10f, true); } set { m_Config.SetFloat(_s, "DashG", value); } }
            public static float DashLineB { get { return m_Config.GetFloat(_s, "DashB", 0.86f, true); } set { m_Config.SetFloat(_s, "DashB", value); } }
            public static Color DashLineColor => new Color(DashLineR, DashLineG, DashLineB, DashLineA);

            public static float ChartStandardPositionX { get { return m_Config.GetFloat(_s, "ChartStandardPositionX",     0, true); } set { m_Config.SetFloat(_s, "ChartStandardPositionX", value); } }
            public static float ChartStandardPositionY { get { return m_Config.GetFloat(_s, "ChartStandardPositionY", -0.4f, true); } set { m_Config.SetFloat(_s, "ChartStandardPositionY", value); } }
            public static float ChartStandardPositionZ { get { return m_Config.GetFloat(_s, "ChartStandardPositionZ", 2.25f, true); } set { m_Config.SetFloat(_s, "ChartStandardPositionZ", value); } }
            public static float ChartStandardRotationX { get { return m_Config.GetFloat(_s, "ChartStandardRotationX",   35f, true); } set { m_Config.SetFloat(_s, "ChartStandardRotationX", value); } }
            public static float ChartStandardRotationY { get { return m_Config.GetFloat(_s, "ChartStandardRotationY",    0f, true); } set { m_Config.SetFloat(_s, "ChartStandardRotationY", value); } }
            public static float ChartStandardRotationZ { get { return m_Config.GetFloat(_s, "ChartStandardRotationZ",    0f, true); } set { m_Config.SetFloat(_s, "ChartStandardRotationZ", value); } }

            public static float Chart360_90PositionX { get { return m_Config.GetFloat(_s, "Chart360_90PositionX",     0, true); } set { m_Config.SetFloat(_s, "Chart360_90PositionX", value); } }
            public static float Chart360_90PositionY { get { return m_Config.GetFloat(_s, "Chart360_90PositionY", 3.50f, true); } set { m_Config.SetFloat(_s, "Chart360_90PositionY", value); } }
            public static float Chart360_90PositionZ { get { return m_Config.GetFloat(_s, "Chart360_90PositionZ", 3.00f, true); } set { m_Config.SetFloat(_s, "Chart360_90PositionZ", value); } }
            public static float Chart360_90RotationX { get { return m_Config.GetFloat(_s, "Chart360_90RotationX",  -30f, true); } set { m_Config.SetFloat(_s, "Chart360_90RotationX", value); } }
            public static float Chart360_90RotationY { get { return m_Config.GetFloat(_s, "Chart360_90RotationY",    0f, true); } set { m_Config.SetFloat(_s, "Chart360_90RotationY", value); } }
            public static float Chart360_90RotationZ { get { return m_Config.GetFloat(_s, "Chart360_90RotationZ",    0f, true); } set { m_Config.SetFloat(_s, "Chart360_90RotationZ", value); } }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Init config
        /// </summary>
        public static void Init() => m_Config = new SDK.Config.INIConfig("BeatSaberPlus");
    }
}
