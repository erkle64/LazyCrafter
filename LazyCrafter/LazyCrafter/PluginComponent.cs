using System;
using UnhollowerBaseLib;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace LazyCrafter
{
    public class PluginComponent : MonoBehaviour
    {
        private static Dictionary<string, ulong> recipeIndex = new System.Collections.Generic.Dictionary<string, ulong>();
        private static Dictionary<string, ulong> itemIndex = new System.Collections.Generic.Dictionary<string, ulong>();

        public PluginComponent (IntPtr ptr) : base(ptr)
        {
        }

        [HarmonyPostfix]
        public static void Update(InputProxy __instance)
        {
            // I was curious. Looks like it is not functional at all
            //if (Input.GetKeyDown(KeyCode.Backslash) && Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt))
            //{
            //    BepInExLoader.log.LogMessage("Cheating...");
            //    for (var en = CharacterManager.singleton.list_charactersInWorld.System_Collections_IEnumerable_GetEnumerator().Current.Cast<Il2CppSystem.Collections.Generic.LinkedListNode<Character>>(); en != null && en.item != null; en = en.Next)
            //    {
            //        var character = en.item;
            //        if (character.sessionOnly_isClientCharacter)
            //        {
            //            InventoryManager.inventoryManagerPtr_tryAddItemAtAnyPosition(character.inventoryPtr, itemIndex["_base_blueprint_tool"], 1, IOBool.iotrue);
            //            BepInExLoader.log.LogMessage("Cheated!!!");
            //            break;
            //        }
            //    }
            //}

            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                BepInExLoader.log.LogMessage("Try crafting");
                for (var en = CharacterManager.singleton.list_charactersInWorld.System_Collections_IEnumerable_GetEnumerator().Current.Cast<Il2CppSystem.Collections.Generic.LinkedListNode<Character>>(); en != null && en.item != null; en = en.Next)
                {
                    var character = en.item;
                    if(character.sessionOnly_isClientCharacter)
                    {
                        var item = character.clientData.getEquippedItemTemplate();
                        if (item != null)
                        {
                            BepInExLoader.log.LogMessage(string.Format("Item: {0}", item.identifier));
                            if (recipeIndex.ContainsKey(item.identifier))
                            {
                                int amount = 1;
                                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) amount *= 5;
                                if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) amount *= 10;

                                BepInExLoader.log.LogMessage(string.Format("Crafting {0}", amount));
                                GameRoot.addLockstepEvent(new Character.CharacterCraftingEvent(character.usernameHash, recipeIndex[item.identifier], amount));
                            }
                        }
                        break;
                    }
                }
            }
        }

        [HarmonyPostfix]
        public static void onLoadRecipe(CraftingRecipe __instance)
        {
            if (__instance.output_data.Length > 0)
            {
                //BepInExLoader.log.LogMessage(string.Format("onLoadRecipe: {0} {1} {2} {3}", __instance.name, __instance.identifier, __instance.id, __instance.output_data[0].identifier));
                recipeIndex[__instance.output_data[0].identifier] = __instance.id;
            }
        }

        public static void onLoadItemTemplate(ItemTemplate __instance)
        {
            //BepInExLoader.log.LogMessage(string.Format("onLoadItemTemplate: {0} {1} {2} {3} {4}", __instance.name, __instance.identifier, __instance.id, __instance.includeInBuild, __instance.isHiddenItem));
            itemIndex[__instance.identifier] = __instance.id;
        }
    }
}