using System;
using UnhollowerBaseLib;
using HarmonyLib;
using UnityEngine;

namespace LazyCrafter
{
    public class PluginComponent : MonoBehaviour
    {
        public PluginComponent (IntPtr ptr) : base(ptr)
        {
        }

        [HarmonyPostfix]
        public static void Update(InputProxy __instance)
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                BepInExLoader.log.LogMessage("Try crafting");
                //CharacterManager.increasePlayerInventorySizeByResearch(8);

                var en = CharacterManager.singleton.list_charactersInWorld.System_Collections_IEnumerable_GetEnumerator().Current.Cast<Il2CppSystem.Collections.Generic.LinkedListNode<Character>>();
                for (; en != null && en.item != null; en = en.Next)
                {
                    var character = en.item;
                    if(character.sessionOnly_isClientCharacter)
                    {
                        var item = character.clientData.getItemTemplateFromCurrentHotkeybarSlot();
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

        private static System.Collections.Generic.Dictionary<string, ulong> recipeIndex = new System.Collections.Generic.Dictionary<string, ulong>();
        [HarmonyPostfix]
        public static void onLoadRecipe(CraftingRecipe __instance)
        {
            //BepInExLoader.log.LogMessage(string.Format("onLoadRecipe: {0}, {1}, {2}, {3}", __instance.name, __instance.identifier, __instance.id, __instance.output_data[0].identifier));
            recipeIndex[__instance.output_data[0].identifier] = __instance.id;
        }
    }
}