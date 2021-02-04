using System;
using System.Collections.Generic;

namespace Nekoyume.UI
{
    [Serializable]
    public enum TutorialActionType
    {
        None = 0,
        TutorialActionHackAndSlash,
        TutorialActionCombinationClick,
        TutorialActionClickFirstRecipeCellView,
        TutorialActionClickCombinationSubmitButton,
        TutorialActionClickBottomMenuWorkShopButton,
        TutorialActionClickBottomMenuMailButton,
        TutorialActionClickBottomMenuCharacterButton,
        TutorialActionClickBottomMenuBackButton,
        TutorialActionClickFirstCombinationMailSubmitButton,
    }

    public class TutorialActionTypeComparer : IEqualityComparer<TutorialActionType>
    {
        public bool Equals(TutorialActionType x, TutorialActionType y)
        {
            return x == y;
        }

        public int GetHashCode(TutorialActionType obj)
        {
            return obj.GetHashCode();
        }
    }
}
