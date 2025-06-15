using ChatPlexMod_ChatIntegrations;
using CP_SDK.Chat.Interfaces;
using CP_SDK.XUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BeatSaberPlus_ChatIntegrations.BeatSaber.Actions
{
    public class ChatRequest_AddToQueue
        : ChatPlexMod_ChatIntegrations.Interfaces.IAction<ChatRequest_AddToQueue, Models.Actions.ChatRequest_AddToQueue>
    {
        private XUIDropdown         m_ValueSource       = null;
        private XUIText             m_BSRKey            = null;
        private XUITextInput        m_TitlePrefix       = null;
        private XUIToggle           m_AsModerator       = null;
        private XUIToggle           m_AddToTop          = null;
        private XUIToggle           m_SendMessage       = null;
        private XUIPrimaryButton    m_SetFromGameButton = null;
        public XUIPrimaryButton     m_SetFromChatButton = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Add a song to Chat Request queue";


        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform parent)
        {
            var l_Choices = new List<string>(Enums.ValueSource.S);
            if (   Event.GetType() != typeof(ChatPlexMod_ChatIntegrations.Events.ChatCommand)
                && Event.GetType() != typeof(ChatPlexMod_ChatIntegrations.Events.ChatPointsReward))
                l_Choices.Remove(Enums.ValueSource.ToStr(Enums.ValueSource.E.Event));

            l_Choices.Remove(Enums.ValueSource.ToStr(Enums.ValueSource.E.Config));
            l_Choices.Remove(Enums.ValueSource.ToStr(Enums.ValueSource.E.Random));

            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Value source",
                    XUIDropdown.Make()
                        .SetOptions(l_Choices).SetValue(Enums.ValueSource.ToStr(Model.ValueSource)).OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ValueSource)
                ),

                Templates.SettingsVGroup("BSR Key",
                    XUIText.Make(Model.BaseValue)
                        .SetAlign(TMPro.TextAlignmentOptions.Center)
                        .SetWrapping(true)
                        .Bind(ref m_BSRKey),

                    XUIHLayout.Make(
                        XUIPrimaryButton.Make("Set from game", OnSetFromGameButton).Bind(ref m_SetFromGameButton),
                        XUIPrimaryButton.Make("Set from chat", OnSetFromChatButton).Bind(ref m_SetFromChatButton)
                    )
                    .SetPadding(0)
                    .OnReady(x => x.CSizeFitter.enabled = false)
                    .ForEachDirect<XUIPrimaryButton>  (y => {
                        y.OnReady((x) => x.CSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained);
                    })
                ),

                Templates.SettingsHGroup("Options",
                    XUIVLayout.Make(
                        XUIText.Make("Title Prefix"),
                        XUIText.Make("As moderator?"),
                        XUIText.Make("Add to top?"),
                        XUIText.Make("Send chat message?")
                    )
                    .SetSpacing(1)
                    .SetPadding(0)
                    .OnReady(x => x.HOrVLayoutGroup.childForceExpandWidth = true)
                    .ForEachDirect<XUIText>(x => x.SetAlign(TMPro.TextAlignmentOptions.CaplineLeft)),

                    XUIVLayout.Make(
                        XUITextInput.Make(string.Empty)
                            .SetValue(Model.TitlePrefix)
                            .Bind(ref m_TitlePrefix),

                        XUIToggle.Make()
                            .SetValue(Model.AsModerator).OnValueChanged((_) => OnSettingChanged())
                            .Bind(ref m_AsModerator),

                        XUIToggle.Make()
                            .SetValue(Model.AddToTop).OnValueChanged((_) => OnSettingChanged())
                            .Bind(ref m_AddToTop),

                        XUIToggle.Make()
                            .SetValue(Model.SendChatMessage).OnValueChanged((_) => OnSettingChanged())
                            .Bind(ref m_SendMessage)
                    )
                    .SetSpacing(1)
                    .SetPadding(0)
                    .OnReady(x => x.VLayoutGroup.childAlignment = TextAnchor.MiddleLeft)
                )
            };

            BuildUIAuto(parent);

            OnSettingChanged();

            if (!ModulePresence.ChatRequest)
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatRequest module is missing or disabled!");
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
        {
            Model.ValueSource       = Enums.ValueSource.ToEnum(m_ValueSource.Element.GetValue());
            Model.BaseValue         = m_BSRKey.Element.GetText();
            Model.TitlePrefix       = m_TitlePrefix.Element.GetValue();
            Model.AsModerator       = m_AsModerator.Element.GetValue();
            Model.AddToTop          = m_AddToTop.Element.GetValue();
            Model.SendChatMessage   = m_SendMessage.Element.GetValue();

            m_BSRKey.SetActive(Model.ValueSource == Enums.ValueSource.E.User);
            m_SetFromGameButton.SetInteractable(Model.ValueSource == Enums.ValueSource.E.User);
            m_SetFromChatButton.SetInteractable(Model.ValueSource == Enums.ValueSource.E.User);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSetFromGameButton()
        {
            View.ShowKeyboardModal(Model.BaseValue, (p_Result) =>
            {
                Model.BaseValue = p_Result;

                m_BSRKey.SetText(Model.BaseValue);
                OnSettingChanged();
            }, null);
        }
        private void OnSetFromChatButton()
        {
            ChatIntegrations.Instance.OnBroadcasterChatMessage += Instance_OnBroadcasterChatMessage;

            var l_Message = "Please input a message in chat with your streaming account.";
            View.ShowLoadingModal(l_Message, true, () =>
            {
                ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;
            });
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void Instance_OnBroadcasterChatMessage(IChatMessage message)
        {
            Model.BaseValue = message.Message;
            ChatIntegrations.Instance.OnBroadcasterChatMessage -= Instance_OnBroadcasterChatMessage;

            View.CloseLoadingModal();

            m_BSRKey.SetText(Model.BaseValue);
            OnSettingChanged();
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext context)
        {
            if (!ModulePresence.ChatRequest)
            {
                context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatRequest module is missing or disabled!");
                yield break;
            }

            var l_BSRKey = Model.BaseValue;
            if (Model.ValueSource == Enums.ValueSource.E.Event && (context.Message != null || context.PointsEvent != null)) /// Event user input
            {
                var l_Src = (context.Message?.Message ?? context.PointsEvent?.UserInput) ?? "";
                var l_Parts = l_Src.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
                if (l_Parts.Length >= 1)
                    l_BSRKey = l_Parts[0];
                else
                {
                    context.HasActionFailed = true;
                    if (Model.SendChatMessage && context.ChatService != null && context.Channel != null && context.User != null)
                        context.ChatService.SendTextMessage(context.Channel, $"! @{context.User.DisplayName} the syntax is: #BSRKey");

                    yield break;
                }
            }

            BeatSaberPlus_ChatRequest.Models.AddToQueueResult l_Result = null;
            BeatSaberPlus_ChatRequest.ChatRequest.Instance.AddToQueueFromBSRKey(
                bsrKey:             l_BSRKey,
                requester:          context.User,
                onBehalfOf:         "$ChatIntegrations",
                forceNamePrefix:    Model.TitlePrefix,
                asModAdd:           Model.AsModerator,
                addToTop:           Model.AddToTop,
                callback:           (x) => l_Result = x
            );

            while (l_Result == null)
                yield return null;

            if (l_Result.Result == BeatSaberPlus_ChatRequest.Models.EAddToQueueResult.OK)
            {
                if (Model.SendChatMessage && context.ChatService != null && context.Channel != null && context.User != null)
                    context.ChatService.SendTextMessage(context.Channel, $"! @{context.User.DisplayName} Added to queue!");
            }
            else
            {
                context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage($"ChatIntegrations: Action ChatRequest.AddToQueue failed: {l_Result.Result}!");

                if (Model.SendChatMessage && context.ChatService != null && context.Channel != null && context.User != null)
                    context.ChatService.SendTextMessage(context.Channel, $"! @{context.User.DisplayName} Error: {l_Result.GetFormattedError()}");

                yield break;
            }

            yield return null;
        }
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public class ChatRequest_ToggleQueue
       : ChatPlexMod_ChatIntegrations.Interfaces.IAction<ChatRequest_ToggleQueue, Models.Actions.ChatRequest_ToggleQueue>
    {
        private XUIDropdown m_ChangeType = null;

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override string Description => "Open or close ChatRequest queue";

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override sealed void BuildUI(Transform parent)
        {
            XUIElements = new IXUIElement[]
            {
                Templates.SettingsHGroup("Change type",
                    XUIDropdown.Make()
                        .SetOptions(Enums.EChatRequestQueueToggle.S)
                        .SetValue(Enums.EChatRequestQueueToggle.ToStr(Model.ChangeType))
                        .OnValueChanged((_, __) => OnSettingChanged())
                        .Bind(ref m_ChangeType)
                )
            };

            BuildUIAuto(parent);
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        private void OnSettingChanged()
            => Model.ChangeType = Enums.EChatRequestQueueToggle.ToEnum(m_ChangeType.Element.GetValue());

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        public override IEnumerator Eval(ChatPlexMod_ChatIntegrations.Models.EventContext context)
        {
            if (!ModulePresence.ChatRequest)
            {
                context.HasActionFailed = true;
                CP_SDK.Chat.Service.Multiplexer?.InternalBroadcastSystemMessage("ChatIntegrations: Action failed, ChatRequest module is missing or disabled!");
                yield break;
            }

            var l_Instance = BeatSaberPlus_ChatRequest.ChatRequest.Instance;
            switch (Model.ChangeType)
            {
                case Enums.EChatRequestQueueToggle.E.Toggle:
                    l_Instance.ToggleQueueStatus();
                    break;
                case Enums.EChatRequestQueueToggle.E.Open:
                    if (l_Instance.QueueOpen == false)
                        l_Instance.ToggleQueueStatus();
                    break;
                case Enums.EChatRequestQueueToggle.E.Close:
                    if (l_Instance.QueueOpen == true)
                        l_Instance.ToggleQueueStatus();
                    break;
            }

            yield return null;
        }
    }
}
