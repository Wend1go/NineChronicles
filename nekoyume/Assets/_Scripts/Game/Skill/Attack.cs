using UnityEngine;


namespace Nekoyume.Game.Skill
{
    public class Attack : Skill
    {
        override public bool Use()
        {
            if (IsCooltime())
                return false;

            _cooltime = (float)_data.Cooltime;

            // TODO: Calc damage
            int damage = Mathf.FloorToInt(1.0f * ((float)_data.Power * 0.01f));
            float range = (float)_data.Range / (float)Game.PixelPerUnit;
            float size = (float)_data.Size / (float)Game.PixelPerUnit;

            GameObject target = GetNearestTarget(Tag.Enemy);
            var objectPool = GetComponentInParent<Util.ObjectPool>();
            var damager = objectPool.Get<Trigger.Damager>(target.transform.position);
            damager.Set(Tag.Enemy, damage, size, _data.TargetCount);

            return true;
        }
    }
}
