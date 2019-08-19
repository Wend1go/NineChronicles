using System;
using System.Collections.Generic;

namespace Nekoyume.Game.Item
{
    [Serializable]
    public class SetItem : Equipment
    {
        public SetItem(Data.Table.Item data, Guid id, Skill skill = null)
            : base(data, id, skill)
        {
        }

        public static Dictionary<int, int> WeaponMap =>
            new Dictionary<int, int>
            {
                [308001] = 301001,
                [308002] = 301002,
                [308003] = 301003,
            };
    }
}
