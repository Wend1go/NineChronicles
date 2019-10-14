using System;
using System.Collections.Generic;

namespace Nekoyume.TableData
{
    [Serializable]
    public class LevelSheet : Sheet<int, LevelSheet.Row>
    {
        [Serializable]
        public class Row : SheetRow<int>
        {
            public override int Key => Level;
            public int Level { get; private set; }
            public long Exp { get; private set; }
            public long ExpNeed { get; private set; }

            public override void Set(IReadOnlyList<string> fields)
            {
                Level = int.TryParse(fields[0], out var level) ? level : 0;
                Exp = long.TryParse(fields[1], out var exp) ? exp : 0L;
                ExpNeed = long.TryParse(fields[2], out var expNeed) ? expNeed : 0L;
            }
        }
        
        public LevelSheet() : base(nameof(LevelSheet))
        {
        }

        public int GetLevel(long exp)
        {
            foreach (var row in OrderedList)
            {
                if (row.Exp + row.ExpNeed > exp)
                {
                    return row.Key;
                }
            }

            return 0;
        }
    }
}
