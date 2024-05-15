using C3.ModKit;
using HarmonyLib;
using System.Collections.Generic;
using Unfoundry;

namespace LazyCrafter
{
    [UnfoundryMod(GUID)]
    public class Plugin : UnfoundryPlugin
    {
        public const string
            MODNAME = "LazyCrafter",
            AUTHOR = "erkle64",
            GUID = AUTHOR + "." + MODNAME,
            VERSION = "1.2.0";

        public static LogSource log;

        public static TypedConfigEntry<int> autoCraftAmount;
        public static TypedConfigEntry<int> autoCraftAltAmount;
        public static TypedConfigEntry<bool> autoCraftOnlyWhenEmpty;

        private static readonly Dictionary<ulong, ulong> _recipeIndex = new Dictionary<ulong, ulong>();

        public Plugin()
        {
            log = new LogSource(MODNAME);

            new Config(GUID)
                .Group("General")
                    .Entry(out autoCraftAmount, "autoCraftAmount", 5, "How many items to craft.")
                    .Entry(out autoCraftAltAmount, "autoCraftAltAmount", 50, "How many items to craft if alt is held.")
                    .Entry(out autoCraftOnlyWhenEmpty, "autoCraftOnlyWhenEmpty", false, "Don't craft if the player already has the item.")
                .EndGroup()
                .Load()
                .Save();
        }

        public override void Load(Mod mod)
        {
            log.Log($"Loading {MODNAME}");
        }

        [HarmonyPatch]
        public static class Patch
        {
            [HarmonyPatch(typeof(Character.ClientData), nameof(Character.ClientData.userPressedBarSlotCallback))]
            [HarmonyPrefix]
            public static bool CharacterClientDataUserPressedBarSlotCallback(Character.ClientData __instance, uint slotIdx)
            {
                var itemTemplate = __instance.getItemTemplateForHotkeybarSlot(__instance.hotkeyBar_currentBarIdx, slotIdx);
                var clientCharacter = GameRoot.getClientCharacter();

                if (itemTemplate != null && _recipeIndex.TryGetValue(itemTemplate.id, out var recipeId))
                {
                    if (!InputHelpers.IsShiftHeld) return true;
                    if (autoCraftOnlyWhenEmpty.Get() && InventoryManager.inventoryManager_countByItemTemplateByPtr(clientCharacter.inventoryPtr, itemTemplate.id, IOBool.iotrue) > 0U) return true;

                    HotkeyBar.triggerClickAnimationForSlot(slotIdx);

                    var amount = InputHelpers.IsAltHeld ? autoCraftAltAmount.Get() : autoCraftAmount.Get();

                    InfoMessageSystem.addSingleTextInfoMessage($"Crafting {amount}x {itemTemplate.name}");

                    GameRoot.addLockstepEvent(new Character.CharacterCraftingEvent(clientCharacter.usernameHash, recipeId, amount));

                    return false;
                }

                return true;
            }

            [HarmonyPatch(typeof(CraftingRecipe), nameof(CraftingRecipe.onLoad))]
            [HarmonyPostfix]
            public static void CrafintRecipeOnLoad(CraftingRecipe __instance)
            {
                if (__instance.output_data.Length > 0 && __instance.output[0].itemTemplate != null)
                {
                    _recipeIndex[__instance.output[0].itemTemplate.id] = __instance.id;
                }
            }
        }
    }
}


