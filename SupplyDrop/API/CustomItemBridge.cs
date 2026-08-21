using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Exiled.API.Features;
using UnityEngine;
using CustomItem = Exiled.CustomItems.API.Features.CustomItem;

namespace SupplyDrop.API
{
    public static class CustomItemBridge
    {
        private const string AssemblyName = "Exiled.CustomItems";

        private static bool probed;
        private static bool available;

        public static bool Available
        {
            get
            {
                if (!probed)
                {
                    probed = true;
                    available = IsAssemblyLoaded() && Probe();
                }

                return available;
            }
        }

        public static void Reset()
        {
            probed = false;
            available = false;
        }

        public static bool Exists(string reference)
        {
            if (!Available || string.IsNullOrEmpty(reference))
                return false;

            try
            {
                return ExistsCore(reference);
            }
            catch (Exception e)
            {
                Log.Error($"CustomItemBridge.Exists({reference}): {e}");
                return false;
            }
        }

        public static bool Spawn(string reference, Vector3 position)
        {
            if (!Available || string.IsNullOrEmpty(reference))
                return false;

            try
            {
                return SpawnCore(reference, position);
            }
            catch (Exception e)
            {
                Log.Error($"CustomItemBridge.Spawn({reference}): {e}");
                return false;
            }
        }

        public static int Collect(List<string> destination, List<ItemType> baseItems)
        {
            if (destination is null)
                return 0;

            if (!Available)
                return 0;

            try
            {
                return CollectCore(destination, baseItems);
            }
            catch (Exception e)
            {
                Log.Error($"CustomItemBridge.Collect: {e}");
                return 0;
            }
        }

        private static bool IsAssemblyLoaded()
        {
            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(assembly.GetName().Name, AssemblyName, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool Probe()
        {
            try
            {
                return CustomItem.Registered is not null;
            }
            catch (Exception e)
            {
                Log.Warn($"Exiled.CustomItems present mais inutilisable, les objets personnalises sont desactives : {e.Message}");
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int CollectCore(List<string> destination, List<ItemType> baseItems)
        {
            int added = 0;

            foreach (CustomItem item in CustomItem.Registered)
            {
                if (item is null || string.IsNullOrEmpty(item.Name))
                    continue;

                if (baseItems is not null && baseItems.Count > 0 && !baseItems.Contains(item.Type))
                    continue;

                destination.Add(item.Name);
                added++;
            }

            return added;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool ExistsCore(string reference) => ResolveCore(reference) is not null;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool SpawnCore(string reference, Vector3 position)
        {
            CustomItem item = ResolveCore(reference);

            return item is not null && item.Spawn(position) is not null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static CustomItem ResolveCore(string reference)
        {
            if (uint.TryParse(reference, out uint id) && CustomItem.TryGet(id, out CustomItem byId) && byId is not null)
                return byId;

            return CustomItem.TryGet(reference, out CustomItem byName) ? byName : null;
        }
    }
}
