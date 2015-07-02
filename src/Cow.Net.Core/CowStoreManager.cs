using System.Collections.Generic;
using System.Linq;

namespace Cow.Net.Core
{
    public class CowStoreManager
    {
        public List<CowStore> Stores { get; private set; }

        /// <summary>
        /// All main stores for the COW client
        /// </summary>
        public CowStoreManager(IEnumerable<CowStore> cowStores)            
        {
            Stores = new List<CowStore>(cowStores);            
        }

        /// <summary>
        /// Find a store by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>CowStore if found, null if no store was found by given id</returns>
        public CowStore GetStoreById(string id)
        {
            return Stores.Select(store => GetStoreById(id, store)).FirstOrDefault(foundStore => foundStore != null);
        }

        /// <summary>
        /// Retrieve the id's of the configured stores
        /// </summary>
        /// <param name="includeNonLocalStores">if true include all store id's if false exclude non local stores</param>
        /// <returns>List of store id's</returns>
        public List<string> GetStoreIds(bool includeNonLocalStores)
        {
            var ids = new List<string>();
            foreach (var store in Stores)
            {
                ids.AddRange(GetStoreIds(store, includeNonLocalStores));
            }

            return ids.Any() ? ids : null;
        }

        private List<string> GetStoreIds(CowStore cowStore, bool includeNonLocalStores, List<string> ids = null)
        {
            ids = ids ?? new List<string>();

            if (cowStore.SaveToLocalDatabase || (!includeNonLocalStores && cowStore.SaveToLocalDatabase))
            {
                ids.Add(cowStore.Id);
            }

            if (cowStore.SubStores == null)
            {
                return ids;
            }

            foreach (var subStore in cowStore.SubStores)
            {
                GetStoreIds(subStore, includeNonLocalStores, ids);
            }

            return ids;
        }

        private CowStore GetStoreById(string id, CowStore cowStore)
        {
            if (cowStore.Id.Equals(id))
                return cowStore;

            return cowStore.SubStores == null ? null : cowStore.SubStores.Select(subStore => GetStoreById(id, subStore)).FirstOrDefault(store => store != null);
        }
    }
}
