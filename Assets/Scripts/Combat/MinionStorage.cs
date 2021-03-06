using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameJam
{
    public class MinionSlot
    {
        public Minion minion;
        public int amountStored;

        // runtime data
        public int level;
        public float remainingLifetime;
    }

    public class MinionStorage : MonoBehaviour
    {
        [Header("Storage")]
        [Tooltip("Maximum amount of minions stored inside")]
        [SerializeField] private int maxCapacity = 20;
        [Tooltip("Allows to store no more than this number of unique minions")]
        [SerializeField] private int numberOfSlots = 5;

        [Header("Settings")]
        [SerializeField] private float spawnRadius = 10f;
        
        private Entity entity;

        List<MinionSlot> minionSlots = new List<MinionSlot>();
        private int selectedIndex;

        private void Awake()
        {
            entity = GetComponentInParent<Entity>();

            minionSlots = new List<MinionSlot>(maxCapacity);
        }

        // minion storage ======================================
        public int GetTotalStoredAmount()
        {
            int storedAmount = 0;
            minionSlots.ForEach(x => storedAmount += x.amountStored);

            return storedAmount;
}

        public bool IsFull() => GetTotalStoredAmount() >= maxCapacity;

        public MinionSlot GetFirstEmptySlotIndex()
        {
            return minionSlots.Where(x => x.amountStored < 1).FirstOrDefault();
        }

        public void Absorb(Minion minion)
        {
            // can't absorb if full
            if (IsFull()) { return; }

            // new minion data
            MinionSlot newMinionData = new MinionSlot()
            {
                minion = minion,
                level = minion.Level.Current,
            };

            // already an existing one in the list, increase quantity
            MinionSlot existingMinion = minionSlots.Where(x => x == newMinionData).FirstOrDefault();
            if (existingMinion != null)
            {
                existingMinion.amountStored += 1;
            }
            // otherwise add new data
            else
            {
                //TODO add to first empty slot
                var firstEmptySlot = GetFirstEmptySlotIndex();
                firstEmptySlot = newMinionData;
            }
        }
        public void Release(int index)
        {
            // check valid index
            if (index < 0 || index > minionSlots.Count - 1) { return; }


            // remove 1 amount of minion at index
            MinionSlot storedMinionAtIndex = minionSlots[index];

            if (storedMinionAtIndex.amountStored > 0)
            {
                minionSlots[index].amountStored -= 1;

                // nothing more stored at this index, clear
                if (storedMinionAtIndex.amountStored < 1)
                {
                    minionSlots[index] = new MinionSlot();
                }
            }

            // spawn & setup minion
            Vector3 randomSpawnPos = Random.insideUnitSphere * spawnRadius;
            randomSpawnPos.y = 0f;

            Minion minionInstance = Instantiate(storedMinionAtIndex.minion, randomSpawnPos, transform.rotation);
            minionInstance.Setup(entity, storedMinionAtIndex.level);
        }

        public void Empty()
        {
            minionSlots = new List<MinionSlot>();
        }
    }
}