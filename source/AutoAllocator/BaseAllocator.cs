using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine.UI;

namespace jshepler.ngu.mods.AutoAllocator
{
    internal abstract class BaseAllocator
    {
        protected Text[] TextComponents;
        private bool[] _isEnabled;

        protected BaseAllocator(int idCount)
        {
            _isEnabled = new bool[idCount];
            TextComponents = new Text[idCount];
        }

        protected BaseAllocator(int idCount, int textComponentsCount)
        {
            _isEnabled = new bool[idCount];
            TextComponents = new Text[textComponentsCount];
        }

        internal abstract void Allocate(int id, long amount);
        internal abstract long CalcCapDelta(int id);
        internal abstract bool IsTargetReached(int id);

        protected virtual Text GetTextComponent(int id)
        {
            return TextComponents[id];
        }

        protected void UpdateText(int id)
        {
            var tc = GetTextComponent(id);
            if(tc) tc.text = _isEnabled[id] ? "++" : "+";
        }

        internal virtual void OnTargetReached(int id)
        {
            this[id] = false;
        }

        internal bool this[int index]
        {
            get => _isEnabled[index];
            set
            {
                _isEnabled[index] = value;
                UpdateText(index);
            }
        }

        internal IEnumerable<int> EnabledIDs => _isEnabled.SelectWhere(b => b, (b, i) => i);
        internal void DisableAll() => EnabledIDs.Do(i => this[i] = false);
        internal void SetEnabled(IEnumerable<int> ids) => _isEnabled.Do((b, i) => this[i] = ids.Contains(i));
    }
}
