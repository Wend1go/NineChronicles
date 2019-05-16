using System;
using System.Linq;
using Nekoyume.Action;
using UniRx;
using Unity.Mathematics;

namespace Nekoyume.UI.Model
{
    public class ShopItems : IDisposable
    {
        public readonly ReactiveCollection<ShopItem> buyItems = new ReactiveCollection<ShopItem>();
        public readonly ReactiveCollection<ShopItem> sellItems = new ReactiveCollection<ShopItem>();
        
        public readonly Subject<ShopItems> onClickRefresh = new Subject<ShopItems>();

        public ShopItems(Game.Shop shop)
        {
            if (ReferenceEquals(shop, null))
            {
                throw new ArgumentNullException();
            }
            
            ResetBuyItems(shop);
            ResetSellItems(shop);
        }
        
        public void Dispose()
        {
            buyItems.DisposeAll();
            sellItems.DisposeAll();
            
            onClickRefresh.Dispose();
        }

        public void ResetBuyItems(Game.Shop shop)
        {
            var index = UnityEngine.Random.Range(0, shop.items.Count);
            var loop = math.min(shop.items.Count, 16);

            for (var i = 0; i < loop; i++)
            {
                var keyValuePair = shop.items.ElementAt(index);
                if (keyValuePair.Value.Count == 0)
                {
                    i--;
                    continue;
                }

                var item = keyValuePair.Value.ElementAt(0);
                
                buyItems.Add(new ShopItem(item));

                index++;
                if (index == shop.items.Count)
                {
                    index = 0;
                }
            }
        }

        public void ResetSellItems(Game.Shop shop)
        {
            var key = ActionManager.instance.agentAddress.ToByteArray();
            if (!shop.items.ContainsKey(key))
            {
                return;
            }

            var items = shop.items[key];
            foreach (var item in items)
            {
                sellItems.Add(new ShopItem(item));
            }
        }
    }
}
