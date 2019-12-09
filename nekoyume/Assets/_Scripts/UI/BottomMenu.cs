using System;
using System.Collections.Generic;
using System.Linq;
using Nekoyume.Game.Mail;
using Nekoyume.Game.Quest;
using Nekoyume.Model;
using UniRx;
using UnityEngine;

namespace Nekoyume.UI.Module
{
    public class BottomMenu : Widget
    {
        public enum ToggleableType
        {
            Mail,
            Quest,
            Chat,
            IllustratedBook,
            Character,
            Inventory,
            WorldMap,
            Settings
        }

        public class Model
        {
            public readonly ReactiveProperty<UINavigator.NavigationType> NavigationType =
                new ReactiveProperty<UINavigator.NavigationType>(UINavigator.NavigationType.Back);

            public Action<BottomMenu> NavigationAction;

            public readonly ReactiveProperty<bool> HasNotificationInMail = new ReactiveProperty<bool>();
            public readonly ReactiveProperty<bool> HasNotificationInQuest = new ReactiveProperty<bool>();
            //public readonly ReactiveProperty<bool> HasNotificationInChat = new ReactiveProperty<bool>();
            public readonly ReactiveProperty<bool> HasNotificationInIllustratedBook = new ReactiveProperty<bool>();
            public readonly ReactiveProperty<bool> HasNotificationInCharacter = new ReactiveProperty<bool>();
            public readonly ReactiveProperty<bool> HasNotificationInInventory = new ReactiveProperty<bool>();
            public readonly ReactiveProperty<bool> HasNotificationInWorldMap = new ReactiveProperty<bool>();
            public readonly ReactiveProperty<bool> HasNotificationInSettings = new ReactiveProperty<bool>();
        }

        // 네비게이션 버튼.
        public NormalButton quitButton;
        public NormalButton mainButton;
        public NormalButton backButton;
        public GlowingButton leaveBattleButton;

        // 토글 그룹과 버튼.
        private ToggleGroup _toggleGroup;
        public IToggleGroup ToggleGroup => _toggleGroup;
        public NotifiableButton chatButton;
        public NotifiableButton mailButton;
        public NotifiableButton questButton;
        public NotifiableButton illustratedBookButton;
        public NotifiableButton characterButton;
        public NotifiableButton inventoryButton;
        public NotifiableButton worldMapButton;
        public NotifiableButton settingsButton;

        private readonly List<IDisposable> _disposablesAtOnEnable = new List<IDisposable>();

        public readonly Model SharedModel = new Model();

        #region Mono

        public override void Initialize()
        {
            base.Initialize();

            SharedModel.NavigationType.Subscribe(SubscribeNavigationType).AddTo(gameObject);
            SharedModel.HasNotificationInMail.SubscribeTo(mailButton.SharedModel.HasNotification).AddTo(gameObject);
            SharedModel.HasNotificationInQuest.SubscribeTo(questButton.SharedModel.HasNotification).AddTo(gameObject);
            SharedModel.HasNotificationInIllustratedBook.SubscribeTo(illustratedBookButton.SharedModel.HasNotification)
                .AddTo(gameObject);
            SharedModel.HasNotificationInCharacter.SubscribeTo(characterButton.SharedModel.HasNotification)
                .AddTo(gameObject);
            SharedModel.HasNotificationInInventory.SubscribeTo(inventoryButton.SharedModel.HasNotification)
                .AddTo(gameObject);
            SharedModel.HasNotificationInWorldMap.SubscribeTo(worldMapButton.SharedModel.HasNotification)
                .AddTo(gameObject);
            SharedModel.HasNotificationInSettings.SubscribeTo(settingsButton.SharedModel.HasNotification)
                .AddTo(gameObject);

            backButton.button.OnClickAsObservable().Subscribe(_ => _toggleGroup.SetToggledOffAll()).AddTo(gameObject);
            backButton.button.OnClickAsObservable().Subscribe(SubscribeNavigationButtonClick).AddTo(gameObject);
            mainButton.button.OnClickAsObservable().Subscribe(SubscribeNavigationButtonClick).AddTo(gameObject);
            quitButton.button.OnClickAsObservable().Subscribe(SubscribeNavigationButtonClick).AddTo(gameObject);
            leaveBattleButton.button.OnClickAsObservable().Subscribe(SubscribeNavigationButtonClick).AddTo(gameObject);

            _toggleGroup = new ToggleGroup();
            _toggleGroup.RegisterToggleable(mailButton);
            _toggleGroup.RegisterToggleable(questButton);
            _toggleGroup.RegisterToggleable(illustratedBookButton);
            _toggleGroup.RegisterToggleable(characterButton);
            _toggleGroup.RegisterToggleable(inventoryButton);
            _toggleGroup.RegisterToggleable(worldMapButton);
            _toggleGroup.RegisterToggleable(settingsButton);
            _toggleGroup.RegisterToggleable(chatButton);

            mailButton.SetWidgetType<Mail>();
            questButton.SetWidgetType<Quest>();
            characterButton.SetWidgetType<StatusDetail>();
            inventoryButton.SetWidgetType<UI.Inventory>();
            // todo: 지금 월드맵 띄우는 것을 위젯으로 빼고, 여기서 설정하기?
            // worldMapButton.SetWidgetType<WorldMapPaper>();

            chatButton.button.OnClickAsObservable().Subscribe(SubScribeOnClickChat).AddTo(gameObject);
            // 미구현
            illustratedBookButton.button.OnClickAsObservable().Subscribe(SubscribeOnClick).AddTo(gameObject);
            illustratedBookButton.SetWidgetType<Alert>();
            settingsButton.SetWidgetType<Settings>();
            chatButton.SetWidgetType<Confirm>();
        } 

        private void SubScribeOnClickChat(Unit unit)
        {
            var confirm = Find<Confirm>();
            confirm.CloseCallback = result =>
            {
                if (result == ConfirmResult.No)
                    return;
                Application.OpenURL(GameConfig.DiscordLink);
            };
            confirm?.Show("UI_PROCEED_DISCORD", "UI_PROCEED_DISCORD_CONTENT");
        }

        private void SubscribeOnClick(Unit unit)
        {
            Find<Alert>().Set("UI_ALERT_NOT_IMPLEMENTED_TITLE", "UI_ALERT_NOT_IMPLEMENTED_CONTENT");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _disposablesAtOnEnable.DisposeAllAndClear();
            ReactiveCurrentAvatarState.MailBox?.Subscribe(SubscribeAvatarMailBox).AddTo(_disposablesAtOnEnable);
            ReactiveCurrentAvatarState.QuestList?.Subscribe(SubscribeAvatarQuestList).AddTo(_disposablesAtOnEnable);
        }

        protected override void OnDisable()
        {
            _toggleGroup?.SetToggledOffAll();
            _disposablesAtOnEnable.DisposeAllAndClear();
            base.OnDisable();
        }

        #endregion

        public void Show(UINavigator.NavigationType navigationType, Action<BottomMenu> navigationAction,
            bool useShowButtons = false, params ToggleableType[] showButtons)
        {
            base.Show();
            SharedModel.NavigationType.Value = navigationType;
            SharedModel.NavigationAction = navigationAction;

            if (!useShowButtons)
            {
                mailButton.Show();
                questButton.Show();
                chatButton.Show();
                illustratedBookButton.Show();
                characterButton.Show();
                inventoryButton.Show();
                worldMapButton.Show();
                settingsButton.Show();

                return;
            }

            mailButton.Hide();
            questButton.Hide();
            chatButton.Hide();
            illustratedBookButton.Hide();
            characterButton.Hide();
            inventoryButton.Hide();
            worldMapButton.Hide();
            settingsButton.Hide();

            foreach (var toggleableType in showButtons)
            {
                switch (toggleableType)
                {
                    case ToggleableType.Mail:
                        mailButton.Show();
                        break;
                    case ToggleableType.Quest:
                        questButton.Show();
                        break;
                    case ToggleableType.Chat:
                        chatButton.Show();
                        break;
                    case ToggleableType.IllustratedBook:
                        illustratedBookButton.Show();
                        break;
                    case ToggleableType.Character:
                        characterButton.Show();
                        break;
                    case ToggleableType.Inventory:
                        inventoryButton.Show();
                        break;
                    case ToggleableType.WorldMap:
                        worldMapButton.Show();
                        break;
                    case ToggleableType.Settings:
                        settingsButton.Show();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        // 이 위젯은 애니메이션 없이 바로 닫히는 것을 기본으로 함.
        public override void Close(bool ignoreCloseAnimation = false)
        {
            foreach (var toggleable in _toggleGroup.Toggleables)
            {
                if (!(toggleable is IWidgetControllable widgetControllable))
                    continue;

                widgetControllable.HideWidget();
            }

            base.Close(true);
        }

        #region Subscribe

        private void SubscribeNavigationType(UINavigator.NavigationType navigationType)
        {
            switch (navigationType)
            {
                case UINavigator.NavigationType.None:
                    leaveBattleButton.Hide();
                    backButton.Hide();
                    mainButton.Hide();
                    quitButton.Hide();
                    break;
                case UINavigator.NavigationType.Back:
                    leaveBattleButton.Hide();
                    backButton.Show();
                    mainButton.Hide();
                    quitButton.Hide();
                    break;
                case UINavigator.NavigationType.Main:
                    leaveBattleButton.Hide();
                    backButton.Hide();
                    mainButton.Show();
                    quitButton.Hide();
                    break;
                case UINavigator.NavigationType.Battle:
                    leaveBattleButton.Show();
                    backButton.Hide();
                    mainButton.Hide();
                    quitButton.Hide();
                    break;
                case UINavigator.NavigationType.Quit:
                    leaveBattleButton.Hide();
                    backButton.Hide();
                    mainButton.Hide();
                    quitButton.Show();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(navigationType), navigationType, null);
            }
        }

        private void SubscribeNavigationButtonClick(Unit unit)
        {
            SharedModel.NavigationAction?.Invoke(this);
        }

        private void SubscribeAvatarMailBox(MailBox mailBox)
        {
            if (mailBox is null)
                return;

            mailButton.SharedModel.HasNotification.Value = mailBox.Any(i => i.New);
            Find<Mail>().UpdateList();
        }

        private void SubscribeAvatarQuestList(QuestList questList)
        {
            if (questList is null)
                return;

            questButton.SharedModel.HasNotification.Value = questList.Any(i => i.Complete && !i.Receive);
        }

        #endregion
    }
}
