using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Quests;

namespace SuperAardvark.AntiSocial
{
    /// <summary>
    /// Thank u so much super-aardvark
    /// This was copy&pasted from https://github.com/super-aardvark/AardvarkMods-SDV/blob/master/AntiSocial/AntiSocialManager.cs
    /// Super cool usage of static properties
    /// </summary>
    public class AntiSocialManager : IAssetLoader
    {
        public const string AssetName = "Data/AntiSocialNPCs";
        public const string OriginModId = "SuperAardvark.AntiSocial";

        private static Mod modInstance;
        private static HarmonyInstance harmonyInstance;
        private static bool adHoc = false;

        public static AntiSocialManager Instance { get; private set; }

        /// <summary>
        /// Checks for the AntiSocial stand-alone mod before running setup.
        /// </summary>
        /// <param name="modInstance">A reference to your Mod class.</param>
        public static void DoSetupIfNecessary(Mod modInstance)
        {
            if (modInstance.ModManifest.UniqueID.Equals(OriginModId))
            {
                modInstance.Monitor.Log("AntiSocial Mod performing stand-alone setup.", LogLevel.Debug);
                adHoc = false;
                DoSetup(modInstance);
            }
            else if (modInstance.Helper.ModRegistry.IsLoaded(OriginModId))
            {
                modInstance.Monitor.Log("AntiSocial Mod loaded.  Skipping ad hoc setup.", LogLevel.Debug);
            }
            else if (AntiSocialManager.modInstance != null)
            {
                modInstance.Monitor.Log("AntiSocial setup was already completed.", LogLevel.Debug);
            }
            else
            {
                modInstance.Monitor.Log($"AntiSocial Mod not loaded.  No problem; performing ad hoc setup for {modInstance.ModManifest.Name}.", LogLevel.Debug);
                adHoc = true;
                DoSetup(modInstance);
            }
        }

        /// <summary>
        /// Sets up AntiSocial.
        /// </summary>
        /// <param name="modInstance"></param>
        private static void DoSetup(Mod modInstance)
        {
            if (Instance != null)
            {
                modInstance.Monitor.Log($"AntiSocial setup already completed by {AntiSocialManager.modInstance.ModManifest.Name} ({AntiSocialManager.modInstance.ModManifest.UniqueID}).", LogLevel.Warn);
                return;
            }

            Instance = new AntiSocialManager();
            AntiSocialManager.modInstance = modInstance;
            modInstance.Helper.Content.AssetLoaders.Add(Instance);

            harmonyInstance = HarmonyInstance.Create(OriginModId);
            harmonyInstance.Patch(original: AccessTools.Method(typeof(NPC), "get_CanSocialize"),
                                  postfix: new HarmonyMethod(typeof(AntiSocialManager), "get_CanSocialize_Postfix"));
            harmonyInstance.Patch(original: AccessTools.Method(typeof(Utility), "getRandomTownNPC", new Type[] { typeof(Random) }),
                                  transpiler: new HarmonyMethod(typeof(AntiSocialManager), "getRandomTownNPC_Transpiler"));
            harmonyInstance.Patch(original: AccessTools.Method(typeof(SocializeQuest), "loadQuestInfo"),
                                  transpiler: new HarmonyMethod(typeof(AntiSocialManager), "loadQuestInfo_Transpiler"));

        }

        public static bool get_CanSocialize_Postfix(
            bool originalReturnValue,
            NPC __instance)
        {
            try
            {
                if (originalReturnValue && Game1.content.Load<Dictionary<string, string>>(AssetName).ContainsKey(__instance.Name))
                {
                    Log($"Overriding CanSocialize for {__instance.Name}", LogLevel.Trace);
                    return false;
                }
                else
                {
                    return originalReturnValue;
                }
            }
            catch (Exception ex)
            {
                Log($"Error in get_CanSocialize postfix patch: {ex}", LogLevel.Error);
                return originalReturnValue;
            }
        }

        public static IEnumerable<CodeInstruction> getRandomTownNPC_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                Log("Patching getRandomTownNPC...", LogLevel.Trace);
                return PatchNPCDispositions(instructions);
            }
            catch (Exception ex)
            {
                Log($"Error in getRandomTownNPC transpiler patch: {ex}", LogLevel.Error);
                return instructions;
            }
        }

        public static IEnumerable<CodeInstruction> loadQuestInfo_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                Log("Patching loadQuestInfo...", LogLevel.Trace);
                return PatchNPCDispositions(instructions);
            }
            catch (Exception ex)
            {
                Log($"Error in loadQuestInfo transpiler patch: {ex}", LogLevel.Error);
                return instructions;
            }
        }

        private static IEnumerable<CodeInstruction> PatchNPCDispositions(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = instructions.ToList();
            for (int i = 0; i < codes.Count; i++)
            {
                CodeInstruction instr = codes[i];
                //Log($"{instr.opcode} : {instr.operand}");
                if (instr.opcode == OpCodes.Callvirt && instr.operand is MethodInfo method && method.Name == "Load")
                {
                    CodeInstruction prevInstr = codes[i - 1];
                    if (prevInstr.opcode == OpCodes.Ldstr && prevInstr.operand.Equals("Data\\NPCDispositions"))
                    {
                        Log($"Adding call to RemoveAntiSocialNPCs at index {i + 1}");
                        codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(AntiSocialManager), "RemoveAntiSocialNPCs")));
                    }
                }
            }
            return codes;
        }

        public static Dictionary<string, string> RemoveAntiSocialNPCs(Dictionary<string, string> dict)
        {
            try
            {
                Dictionary<string, string> antiSocial = Game1.content.Load<Dictionary<string, string>>(AssetName);
                Dictionary<string, string> result = dict.Where(kvp => !antiSocial.ContainsKey(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                Log($"Initially {dict.Count} NPCs, removed {antiSocial.Count} anti-social ones, returning {result.Count}");
                if (result.Count == 0)
                {
                    Log($"No social NPCs found", LogLevel.Warn);
                    result = dict;
                }
                return result;
            }
            catch (Exception ex)
            {
                Log($"Error in RemoveAntiSocialNPCs: {ex}", LogLevel.Error);
                return dict;
            }
        }

        public bool CanLoad<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals(AntiSocialManager.AssetName);
        }

        public T Load<T>(IAssetInfo asset)
        {
            return (T)(object)new Dictionary<string, string>();
        }

        private static void Log(String message, LogLevel level = LogLevel.Trace)
        {
            modInstance?.Monitor.Log((adHoc ? "[AntiSocial] " + message : message), level);
        }
    }
}