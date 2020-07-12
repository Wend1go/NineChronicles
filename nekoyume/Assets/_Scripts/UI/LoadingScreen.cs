using System.Collections.Generic;
using System.Linq;
using Assets.SimpleLocalization;
using DG.Tweening;
using Nekoyume.UI.Module;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nekoyume.UI
{
    public class LoadingScreen : ScreenWidget
    {
        public LoadingIndicator indicator;
        public TextMeshProUGUI toolTip;

        public string Message { get; internal set; }

        private List<string> _tips;

        #region Mono

        protected override void Awake()
        {
            base.Awake();

            var message = LocalizationManager.Localize("BLOCK_CHAIN_MINING_TX") + "...";
            indicator.UpdateMessage(message);
            _tips = LocalizationManager.LocalizePattern("^UI_TIPS_[0-9]+$").Values.ToList();

            var pos = transform.localPosition;
            pos.z = -5f;
            transform.localPosition = pos;

            if (ReferenceEquals(indicator, null) ||
                ReferenceEquals(toolTip, null))
            {
                throw new SerializeFieldNullException();
            }
        }

        protected override void Update()
        {
            if (!string.IsNullOrEmpty(Message) && indicator.text.text != Message)
            {
                if (indicator.gameObject.activeSelf)
                {
                    indicator.UpdateMessage(Message);
                }
                else
                {
                    indicator.Show(Message);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            toolTip.text = _tips[new System.Random().Next(0, _tips.Count)];
        }

        protected override void OnDisable()
        {
            Message = LocalizationManager.Localize("BLOCK_CHAIN_MINING_TX");
            
            base.OnDisable();
        }

        #endregion
    }
}
