using System;
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
