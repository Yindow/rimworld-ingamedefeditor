﻿using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;

namespace InGameDefEditor
{
    class DatabaseUtil
    {
        private static readonly MethodInfo removeDefMI = typeof(DefDatabase<Def>).GetMethod("Remove", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo shuffleableBackstoryListFI = typeof(BackstoryDatabase).GetField("shuffleableBackstoryList", BindingFlags.Static | BindingFlags.NonPublic);

        public static void Add(Backstory b)
        {
            BackstoryDatabase.AddBackstory(b);
        }

        public static void Add<D>(D d) where D : Def
        {
            DefDatabase<D>.Add(d);
        }

        public static bool Add(object o)
        {
            switch (o)
            {
                case Backstory b:
                    Add(b);
                    return true;
                case Def d:
                    Add(d);
                    return true;
                case string s:
                    if (TryGetFromString(s, out object obj))
                        return Add(obj);
                    break;
            }
            return false;
        }

        public static bool TryGetFromString(string s, out object o)
        {
            if (TryGetFromDB<ThingDef>(s, out o))
                return true;
            if (TryGetFromDB<BiomeDef>(s, out o))
                return true;
            if (TryGetFromDB<DifficultyDef>(s, out o))
                return true;
            if (TryGetFromDB<HediffDef>(s, out o))
                return true;
            if (TryGetFromDB<RecipeDef>(s, out o))
                return true;
            if (TryGetFromDB<StorytellerDef>(s, out o))
                return true;
            if (TryGetFromDB<ThoughtDef>(s, out o))
                return true;
            if (TryGetFromDB<TraitDef>(s, out o))
                return true;
            if (BackstoryDatabase.TryGetWithIdentifier(s, out Backstory b, false))
            {
                o = b;
                return true;
            }

            return false;
        }

        public static bool TryGetDefTypeFor(object o, out DefType dt)
        {
            switch (o)
            {
                case string s:
                    if (TryGetFromString(s, out object obj))
                        return TryGetDefTypeFor(obj, out dt);
                    dt = DefType.Apparel;
                    return false;
                case Backstory _:
                    dt = DefType.Backstory;
                    return true;
                case BiomeDef _:
                    dt = DefType.Biome;
                    return true;
                case DifficultyDef _:
                    dt = DefType.Difficulty;
                    return true;
                case HediffDef _:
                    dt = DefType.Hediff;
                    return true;
                case RecipeDef _:
                    dt = DefType.Recipe;
                    return true;
                case StorytellerDef _:
                    dt = DefType.StoryTeller;
                    return true;
                case ThoughtDef _:
                    dt = DefType.Thought;
                    return true;
                case TraitDef _:
                    dt = DefType.Trait;
                    return true;
                case ThingDef d:
                    if (d.IsApparel)
                    {
                        dt = DefType.Apparel;
                        return true;
                    }
                    else if (d.IsWeapon)
                    {
                        dt = DefType.Weapon;
                        return true;
                    }
                    else if (d.IsIngestible)
                    {
                        dt = DefType.Ingestible;
                        return true;
                    }
                    else if (d.mineable)
                    {
                        dt = DefType.Mineable;
                        return true;
                    }
                    else if (d.building != null)
                    {
                        dt = DefType.Building;
                        return true;
                    }
                    else if (
                        d.defName.StartsWith("Arrow_") ||
                        d.defName.StartsWith("Bullet_") ||
                        d.defName.StartsWith("Proj_"))
                    {
                        dt = DefType.Projectile;
                        return true;
                    }
                    Log.Warning($"Cannot get DefType for {d.defName}");
                    break;
            }
            dt = DefType.Apparel;
            return false;
        }

        private static bool TryGetFromDB<D>(string s, out object o) where D : Def, new()
        {
            o = DefDatabase<D>.GetNamed(s, false);
            return o != null;
        }

        public static void Remove(Backstory b)
        {
            BackstoryDatabase.allBackstories.Remove(b.identifier);
            if (shuffleableBackstoryListFI.GetValue(null) is Dictionary<Pair<BackstorySlot, BackstoryCategoryFilter>, List<Backstory>> dic)
            {
                Log.Message("Cleared BackstoryDatabase.shuffleableBackstoryList");
                dic.Clear();
            }
            else
            {
                Log.Warning("Failed to clear BackstoryDatabase.shuffleableBackstoryList");
            }
        }

        public static void Remove<D>(D d) where D : Def
        {
            removeDefMI.Invoke(null, new object[] { d });
        }

        public static bool Remove(object o)
        {
            switch (o)
            {
                case Backstory b:
                    Remove(b);
                    return true;
                case Def d:
                    Remove(d);
                    return true;
                case string s:
                    if (TryGetFromString(s, out object obj))
                        return Remove(obj);
                    break;
            }
            return false;
        }

        public static bool RemoveDef(string defName)
        {
            Def d = DefDatabase<Def>.GetNamed(defName, false);
            if (d != null)
            {
                Remove(d);
                return true;
            }
            return false;
        }

        public static bool RemoveBackstory(string identifier)
        {
            if (BackstoryDatabase.TryGetWithIdentifier(identifier, out Backstory b, false))
            {
                Remove(b);
                return true;
            }
            return false;
        }
    }
}
