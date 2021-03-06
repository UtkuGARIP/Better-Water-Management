﻿using Harmony;
using UnityEngine;

namespace BetterWaterManagement
{
    [HarmonyPatch(typeof(ConditionTableManager), "GetDisplayNameWithCondition")]
    public class ConditionTableManager_GetDisplayNameWithCondition
    {
        public static void Postfix(GearItem gearItem, ref string __result)
        {
            LiquidItem liquidItem = gearItem.m_LiquidItem;
            if (!liquidItem || liquidItem.m_LiquidType != GearLiquidTypeEnum.Water)
            {
                return;
            }

            if (Water.IsEmpty(liquidItem))
            {
                __result += " - " + Localization.Get("GAMEPLAY_Empty");
            }
            else if (liquidItem.m_LiquidQuality == LiquidQuality.Potable)
            {
                __result += " - " + Localization.Get("GAMEPLAY_WaterPotable");
            }
            else
            {
                __result += " - " + Localization.Get("GAMEPLAY_WaterUnsafe");
            }
        }
    }

    [HarmonyPatch(typeof(GearItem), "GetItemWeightKG")]
    public class GearItem_GetItemWeightKG
    {
        public static void Postfix(GearItem __instance, ref float __result)
        {
            if (__instance.m_WaterSupply != null)
            {
                __result -= __instance.m_WaterSupply.m_VolumeInLiters;
            }
        }
    }

    [HarmonyPatch(typeof(GearItem), "ManualStart")]
    public class GearItem_ManualStart
    {
        public static void Postfix(GearItem __instance)
        {
            LiquidItem liquidItem = __instance.m_LiquidItem;
            if (liquidItem && liquidItem.m_LiquidType == GearLiquidTypeEnum.Water)
            {
                WaterUtils.UpdateWaterBottle(__instance);
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), "AddGear")]
    public class Inventory_AddGear
    {
        private const GearLiquidTypeEnum ModWater = (GearLiquidTypeEnum)1000;

        public static void Postfix(GameObject go)
        {
            LiquidItem liquidItem = go.GetComponent<LiquidItem>();
            if (liquidItem && liquidItem.m_LiquidType == ModWater)
            {
                liquidItem.m_LiquidType = GearLiquidTypeEnum.Water;
                Water.AdjustWaterSupplyToWater();
            }
        }

        public static void Prefix(Inventory __instance, GameObject go)
        {
            LiquidItem liquidItem = go.GetComponent<LiquidItem>();
            if (liquidItem && liquidItem.m_LiquidType == GearLiquidTypeEnum.Water)
            {
                liquidItem.m_LiquidType = ModWater;
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), "AddToPotableWaterSupply")]
    public class Inventory_AddToPotableWaterSupply
    {
        public static void Prefix(float volumeLiters)
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(Inventory), "AddToWaterSupply")]
    public class Inventory_AddToWaterSupply
    {
        public static void Prefix(float numLiters, LiquidQuality quality)
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(Inventory), "RemoveGear")]
    public class Inventory_RemoveGear
    {
        public static void Postfix(GameObject go)
        {
            LiquidItem liquidItem = go.GetComponent<LiquidItem>();
            if (liquidItem && liquidItem.m_LiquidType == GearLiquidTypeEnum.Water)
            {
                Water.AdjustWaterSupplyToWater();
            }
        }
    }

    [HarmonyPatch(typeof(Panel_Container), "IgnoreWaterSupplyItem")]
    public class Panel_Container_IgnoreWaterSupplyItem
    {
        public static bool Prefix(WaterSupply ws, ref bool __result)
        {
            __result = ws != null && ws.GetComponent<LiquidItem>() == null;
            return false;
        }
    }

    [HarmonyPatch(typeof(Panel_Inventory), "IgnoreWaterSupplyItem")]
    public class Panel_Inventory_IgnoreWaterSupplyItem
    {
        public static bool Prefix(WaterSupply ws, ref bool __result)
        {
            __result = ws != null && ws.GetComponent<LiquidItem>() == null;
            return false;
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "OnIncrease")]
    public class Panel_PickWater_OnIncrease
    {
        public static void Postfix(Panel_PickWater __instance)
        {
            PickWater.ClampAmount(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "OnTakeWaterComplete")]
    public class Panel_PickWater_OnTakeWaterComplete
    {
        public static void Postfix()
        {
            Water.AdjustWaterToWaterSupply();
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "SetWaterSourceForTaking")]
    public class Panel_PickWater_SetWaterSourceForTaking
    {
        public static void Postfix(Panel_PickWater __instance)
        {
            PickWater.ClampAmount(__instance);
        }
    }

    [HarmonyPatch(typeof(Panel_PickWater), "Start")]
    public class Panel_PickWater_Start
    {
        public static void Postfix(Panel_PickWater __instance)
        {
            PickWater.Prepare(__instance);
        }
    }

    [HarmonyPatch(typeof(Utils), "GetInventoryIconTexture")]
    internal class Utils_GetInventoryIconTexture
    {
        public static bool Prefix(GearItem gi, ref Texture2D __result)
        {
            LiquidItem liquidItem = gi.m_LiquidItem;
            if (!liquidItem || liquidItem.m_LiquidType != GearLiquidTypeEnum.Water)
            {
                return true;
            }

            string textureName = gi.name.Replace("GEAR_", "ico_GearItem__") + WaterUtils.GetWaterSuffix(liquidItem);
            __result = Utils.GetInventoryIconTextureFromName(textureName);

            return __result == null;
        }
    }
}