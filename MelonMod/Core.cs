using HarmonyLib;
using Il2CppBonsai.Utility;
using Il2CppBulwarkStudios.Codex.Common;
using Il2CppBulwarkStudios.Codex.Mission;
using Il2CppBulwarkStudios.Codex.Skirmish;
using Il2CppBulwarkStudios.Codex.Story;
using Il2CppBulwarkStudios.Codex.Warmap;
using Il2CppBulwarkStudios.Codex.World;
using Il2CppBulwarkStudios.Core.ScalableSystem;
using Il2CppRewired.Utils.Classes.Data;
using MelonLoader;
using MelonLoader.Preferences;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.Rendering.HighDefinition.DLSSPass;

[assembly: MelonInfo(typeof(Mechanicus2Mod.Core), "jw11-modder.Mechanicus2Mod", "1.0.0", "jw11-modder", null)]
[assembly: MelonGame("BulwarkStudios", "Mechanicus2")]

namespace Mechanicus2Mod
{
    public class Core : MelonMod
    {
        public static MelonMod Instance { get; private set; }
        private static MelonPreferences_Category MultiplierFloatCategory;
        private static MelonPreferences_Category MultiplierIntCategory;
        private static MelonPreferences_Category ToggleCategory;

        private static MelonPreferences_Entry<bool> configNoPlayerDamage;
        private static MelonPreferences_Entry<bool> configNoPlayerCooldown;
        private static MelonPreferences_Entry<bool> configInfiniteCog;
        private static MelonPreferences_Entry<bool> configNoAwakeing;
        private static MelonPreferences_Entry<bool> configMissionNoVigilance;

        private static MelonPreferences_Entry<float> configPlayerDamageMultiplier;
        //private static MelonPreferences_Entry<float> configPlayerDominionMultiplier;
        private static MelonPreferences_Entry<float> configPlayerMovementMultiplier;
        private static MelonPreferences_Entry<float> configMissionRewardMultiplier;

        private static MelonPreferences_Entry<int> configPlayerCognitionMultiplier;
        private static MelonPreferences_Entry<int> configStartCognition;

        private static MelonPreferences_Entry<KeyCode> configMenuToggle;

        private static MelonPreferences_Category ModConfCategory;
        private static List<MelonPreferences_Category> CustomCategoryList = new List<MelonPreferences_Category>();

        public static bool showCheatsPopup = false;

        private static GUIStyle JModStyleT = new();
        private static GUIStyle JModStyleH = new();
        private static GUIStyle JModStyleP = new();
        private static GUIStyle JModStylePV = new();
        private static GUIStyle JModStyleB = new();
        private static GUIStyle JModStyleBlank = new();

        private static Color JModColor = new(0.0f, 0.85f, 0.85f);

        private static Rect jModWindowRect;
        private static Rect _screenRect;

        private static CursorLockMode lastLockMode;
        private static bool lastVisibleState;

        public static GameObject CanvasRoot { get; private set; }

        private static EventSystem jModEventSys;
        private static EventSystem lastEventSys;
        private static BaseInputModule lastInputModule;

        public void LogHandler(string log, LogType level)
        {
            switch (level)
            {
                case LogType.Log:
                    base.LoggerInstance.Msg(log);
                    return;
                case LogType.Warning:
                case LogType.Assert:
                    base.LoggerInstance.Warning(log);
                    return;
                case LogType.Exception:
                case LogType.Error:
                    base.LoggerInstance.Error(log);
                    return;
            }
        }
        public static void Log(object message)
            => Log(message, LogType.Log);

        public static void LogWarning(object message)
            => Log(message, LogType.Warning);

        public static void LogError(object message)
            => Log(message, LogType.Error);

        internal static void Log(object message, LogType logType)
        {
            string log = message?.ToString() ?? "";

            switch (logType)
            {
                case LogType.Log:
                case LogType.Assert:
                    Instance.LoggerInstance.Msg(log); break;

                case LogType.Warning:
                    Instance.LoggerInstance.Warning(log); break;

                case LogType.Error:
                case LogType.Exception:
                    Instance.LoggerInstance.Error(log); break;
            }
        }
        public override void OnInitializeMelon()
        {
            Instance = this;
            MultiplierFloatCategory = MelonPreferences.CreateCategory("FloatMultipliers");
            MultiplierIntCategory = MelonPreferences.CreateCategory("IntMultipliers");
            ToggleCategory = MelonPreferences.CreateCategory("Toggles");

            configNoPlayerDamage = ToggleCategory.CreateEntry<bool>("configNoPlayerDamage", false, "Prayer of Certainty of Steel (Disable damage to player units)");
            configNoPlayerCooldown = ToggleCategory.CreateEntry<bool>("configNoPlayerCooldown", false, "Cant of Machine Spirit Awakening (No skills cooldown for player units)");
            configInfiniteCog = ToggleCategory.CreateEntry<bool>("configInfiniteCog", false, "Seal of Omnissiah's Presence (Cognition doesn't decrease)");
            configNoAwakeing = ToggleCategory.CreateEntry<bool>("configNoAwakeing", false, "Rite of Noosphere Calming (No awakening for Necrons)");
            configMissionNoVigilance = ToggleCategory.CreateEntry<bool>("configMissionNoVigilance", false, "Incantation of Quiet Cog Workings (No vigilance increase for Necrons)");

            configPlayerDamageMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerDamageMultiplier", 1f, "Psalm of Righteous Fury (Player units damage multiplier)", validator: new ValueRange<float>(1f, 10f));            
            //configPlayerDominionMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerDominionMultiplier", 1f, "Memory of C'tan Hatred (Dominion gain multiplier)", validator: new ValueRange<float>(1f, 10f));
            configPlayerMovementMultiplier = MultiplierFloatCategory.CreateEntry<float>("configPlayerMovementMultiplier", 1f, "Blessing of the Motive Force (Player units movement multiplier)", validator: new ValueRange<float>(1f, 5f));
            configMissionRewardMultiplier = MultiplierFloatCategory.CreateEntry<float>("configMissionRewardMultiplier", 1f, "Liturgy of the Harvest (Mission reward multiplier)", validator: new ValueRange<float>(1f, 20f));

            configPlayerCognitionMultiplier = MultiplierIntCategory.CreateEntry<int>("configPlayerCognitionMultiplier", 1, "Litany of Knowledge Acquisition (Cognition gain multiplier)", validator: new ValueRange<int>(1, 10));
            configStartCognition = MultiplierIntCategory.CreateEntry<int>("configStartCognition", 0, "Sermon on the Burden of Knowlede (Starting cognition value)", validator: new ValueRange<int>(0, 10));

            JModStyleH.alignment = TextAnchor.MiddleCenter;
            JModStyleH.fontSize = 20;
            JModStyleH.fontStyle = FontStyle.Bold;
            JModStyleH.normal.textColor = JModColor;

            JModStyleP.fontSize = 16;
            JModStyleP.normal.textColor = JModColor;

            JModStylePV.fontSize = 16;
            JModStylePV.fontStyle = FontStyle.Bold;
            JModStylePV.normal.textColor = JModColor;
            JModStylePV.alignment = TextAnchor.MiddleCenter;

            ModConfCategory = MelonPreferences.CreateCategory("JModConfiguration");
            configMenuToggle = ModConfCategory.CreateEntry("ToggleKey", KeyCode.F7, "Main Menu Toggle Key");
            CustomCategoryList.Clear();
            foreach (var category in MelonPreferences.Categories)
            {
                switch (category.Identifier)
                {
                    case "FloatMultipliers":
                        {
                            MultiplierFloatCategory = category;
                            Log("Float Multipliers loaded!");
                            break;
                        }
                    case "IntMultipliers":
                        {
                            MultiplierIntCategory = category;
                            Log("Int Multipliers loaded!");
                            break;
                        }
                    case "Toggles":
                        {
                            ToggleCategory = category;
                            Log("Toggles loaded!");
                            break;
                        }
                    case "JModConfiguration":
                        {
                            break;
                        }
                    default:
                        {
                            CustomCategoryList.Add(category);
                            Log("Custom category: " + category.DisplayName + " loaded!");
                            break;
                        }

                }
            }

            Log("Menu key: " + configMenuToggle.Value.ToString());

            CanvasRoot = new GameObject("JModCanvas");
            UnityEngine.Object.DontDestroyOnLoad(CanvasRoot);
            CanvasRoot.hideFlags |= HideFlags.HideAndDontSave;
            CanvasRoot.layer = 5;
            CanvasRoot.transform.position = new Vector3(0f, 0f, 1f);

            CanvasRoot.SetActive(false);

            jModEventSys = CanvasRoot.AddComponent<EventSystem>();
            jModEventSys.enabled = false;

            CanvasRoot.SetActive(true);

            Log("Machine Spirit blessing... acquired.");
        }


        // Il2CppBulwarkStudios.Codex.Common.WorldInstance FactionData PlayerFaction -> SkirmishTeamData skirmishPlayerTeam -> bool IsPlayerTeamOrAlly() || bool IsPlayerEnemy()

        // Il2CppBulwarkStudios.Codex.World.WorldGlobalConfig WrathLevelData wrathLevelData

        // Il2CppBulwarkStudios.Codex.Common.NecronCourtData List<NecronCourtMemberData> necronCourtMembers

        // configNoPlayerDamage
        [HarmonyPatch(typeof(SkirmishBasicDamageEffect), nameof(SkirmishBasicDamageEffect.GetDamageRange))]
        class GetDamageRangePatch1
        {
            static void Postfix(ref Vector2Int __result, ref ISkirmishEntity targetEntity, SkirmishEffectPipelineExecuteParameters parameters)
            {
                if (!configNoPlayerDamage.Value)
                {
                    return;
                }
                if (targetEntity != null)
                    if (targetEntity.Team.IsPlayerTeamOrAlly())
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y + " owner " + parameters.owner.NameId.ToString());
                        if (!parameters.owner.Team.IsPlayerTeamOrAlly())
                        {
                            __result.x = 0;
                            __result.y = 0;
                        }
                        //Log("!!NO BASIC PLAYER DAMAGE ALLOWED!! to entity: " + targetEntity.NameId.ToString());
                    }
                    else
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y + " owner " + parameters.owner.NameId.ToString());
                        __result.x = Mathf.RoundToInt(__result.x * configPlayerDamageMultiplier.Value);
                        __result.y = Mathf.RoundToInt(__result.y * configPlayerDamageMultiplier.Value);
                        //Log("!!ENEMY BASIC DAMAGE MULTIPLIED!! to entity: " + targetEntity.NameId.ToString());
                    }
            }
        }
        [HarmonyPatch(typeof(SkirmishDamageWithForbiddenKnowledge), nameof(SkirmishDamageWithForbiddenKnowledge.GetDamageRange))]
        class GetDamageRangePatch2
        {
            static void Postfix(ref Vector2Int __result, ref ISkirmishEntity targetEntity)
            {
                if (!configNoPlayerDamage.Value)
                {
                    return;
                }
                if (targetEntity != null)
                    if (targetEntity.Team.IsPlayerTeamOrAlly())
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = 0;
                        __result.y = 0;
                        //Log("!!NO PLAYER FORBIDDEN KNOWLEDGE DAMAGE ALLOWED!! to entity: " + targetEntity.NameId.ToString());
                    }
                    else
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = Mathf.RoundToInt(__result.x * configPlayerDamageMultiplier.Value);
                        __result.y = Mathf.RoundToInt(__result.y * configPlayerDamageMultiplier.Value);
                        //Log("!!ENEMY FORBIDDEN KNOWLEDGE DAMAGE MULTIPLIED!! to entity: " + targetEntity.NameId.ToString());
                    }
            }
        }
        [HarmonyPatch(typeof(SkirmishDamageWithHealthEffect), nameof(SkirmishDamageWithHealthEffect.GetDamageRange))]
        class GetDamageRangePatch3
        {
            static void Postfix(ref Vector2Int __result, ref ISkirmishEntity targetEntity)
            {
                if (!configNoPlayerDamage.Value)
                {
                    return;
                }
                if (targetEntity != null)
                    if (targetEntity.Team.IsPlayerTeamOrAlly())
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = 0;
                        __result.y = 0;
                        //Log("!!NO PLAYER DamageWithHealth DAMAGE ALLOWED!! to entity: " + targetEntity.NameId.ToString());
                    }
                    else
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = Mathf.RoundToInt(__result.x * configPlayerDamageMultiplier.Value);
                        __result.y = Mathf.RoundToInt(__result.y * configPlayerDamageMultiplier.Value);
                        //Log("!!ENEMY DamageWithHealth DAMAGE MULTIPLIED!! to entity: " + targetEntity.NameId.ToString());
                    }
            }
        }


        [HarmonyPatch(typeof(SkirmishDamageWithNetworkEffect), nameof(SkirmishDamageWithNetworkEffect.GetDamageRange))]
        class GetDamageRangePatch4
        {
            static void Postfix(ref Vector2Int __result, ref ISkirmishEntity targetEntity)
            {
                if (!configNoPlayerDamage.Value)
                {
                    return;
                }
                if (targetEntity != null)
                    if (targetEntity.Team.IsPlayerTeamOrAlly())
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = 0;
                        __result.y = 0;
                        //Log("!!NO PLAYER DamageWithNetwork DAMAGE ALLOWED!! to entity: " + targetEntity.NameId.ToString());
                    }
                    else
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = Mathf.RoundToInt(__result.x * configPlayerDamageMultiplier.Value);
                        __result.y = Mathf.RoundToInt(__result.y * configPlayerDamageMultiplier.Value);
                        //Log("!!ENEMY DamageWithNetwork DAMAGE MULTIPLIED!! to entity: " + targetEntity.NameId.ToString());
                    }
            }
        }

        [HarmonyPatch(typeof(SkirmishDamageWithOwnerShieldEffect), nameof(SkirmishDamageWithOwnerShieldEffect.GetDamageRange))]
        class GetDamageRangePatch5
        {
            static void Postfix(ref Vector2Int __result, ref ISkirmishEntity targetEntity)
            {
                if (!configNoPlayerDamage.Value)
                {
                    return;
                }
                if (targetEntity != null)
                    if (targetEntity.Team.IsPlayerTeamOrAlly())
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = 0;
                        __result.y = 0;
                        //Log("!!NO PLAYER DamageWithOwnerShield DAMAGE ALLOWED!! to entity: " + targetEntity.NameId.ToString());
                    }
                    else
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = Mathf.RoundToInt(__result.x * configPlayerDamageMultiplier.Value);
                        __result.y = Mathf.RoundToInt(__result.y * configPlayerDamageMultiplier.Value);
                        //Log("!!ENEMY DamageWithOwnerShield DAMAGE MULTIPLIED!! to entity: " + targetEntity.NameId.ToString());
                    }
            }
        }

        [HarmonyPatch(typeof(SkirmishHealthDependentDamageEffect), nameof(SkirmishHealthDependentDamageEffect.GetDamageRange))]

        class GetDamageRangePatch6
        {
            static void Postfix(ref Vector2Int __result, ref ISkirmishEntity targetEntity)
            {
                if (!configNoPlayerDamage.Value)
                {
                    return;
                }
                if (targetEntity != null)
                    if (targetEntity.Team.IsPlayerTeamOrAlly())
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = 0;
                        __result.y = 0;
                        //Log("!!NO PLAYER HealthDependent DAMAGE ALLOWED!! to entity: " + targetEntity.NameId.ToString());
                    }
                    else
                    {
                        //Log("Original result values: " + __result.x + " " + __result.y);
                        __result.x = Mathf.RoundToInt(__result.x * configPlayerDamageMultiplier.Value);
                        __result.y = Mathf.RoundToInt(__result.y * configPlayerDamageMultiplier.Value);
                        //Log("!!ENEMY HealthDependent DAMAGE MULTIPLIED!! to entity: " + targetEntity.NameId.ToString());
                    }
            }
        }

        [HarmonyPatch(typeof(PerkSkirmishEntityReceiveDamageComponent), nameof(PerkSkirmishEntityReceiveDamageComponent.GetDamageRange))]

        class GetDamageRangePatch7
        {
            static void Postfix(ref Vector2Int __result, ref ISkirmishEntity targetEntity)
            {
                if (!configNoPlayerDamage.Value)
                {
                    return;
                }
                if (targetEntity != null)
                    if (targetEntity.Team.IsPlayerTeamOrAlly())
                    {
                        //Log("Original entity damage received values: " + __result.x + " " + __result.y);
                        __result.x = 0;
                        __result.y = 0;
                        //Log("!!NO PLAYER DAMAGE RECEIVED ALLOWED!! to entity: " + targetEntity.NameId.ToString());
                    }
                    else
                    {
                        //Log("Original entity damage received values: " + __result.x + " " + __result.y);
                        __result.x = Mathf.RoundToInt(__result.x * configPlayerDamageMultiplier.Value);
                        __result.y = Mathf.RoundToInt(__result.y * configPlayerDamageMultiplier.Value);
                        //Log("!!ENEMY DAMAGE RECEIVED MULTIPLIED!! to entity: " + targetEntity.NameId.ToString());
                    }
            }
        }

        // Il2CppBulwarkStudios.Codex.Mission.MissionUnitInstance
        [HarmonyPatch(typeof(MissionUnitInstance), nameof(MissionUnitInstance.add_onHealthChanged))]
        class MissionUnitInstancePatch1
        {
            static void Postfix(ref MissionUnitInstance __instance)
            {
                if (!configNoPlayerDamage.Value)
                {
                    return;
                }
                //Log("Original mission unit health values: " + __instance.HealthPercent + " " + __instance.LostHealthPoint);
                __instance.LostHealthPoint = 0;
            }
        }

        //!!!!
        //Il2CppBulwarkStudios.Codex.World.WorldGlobalConfig WrathLevelData wrathLevelData
        [HarmonyPatch(typeof(WorldGlobalConfig), nameof(WorldGlobalConfig.GetHealthState))]
        class GetHealthStatePatch1
        {
            static void Postfix(ref CodexCommonGlobalConfig.HealthState __result, ref WorldGlobalConfig __instance)
            {
                if (!configNoPlayerDamage.Value)
                {
                    return;
                }
                //Log("Original health values: " + __result.text.ToString() + " " + __result.percent.ToString());
            }
        }

        //configNoPlayerCooldown

        // Il2CppBulwarkStudios.Codex.Skirmish.SkirmishUnitActionData
        [HarmonyPatch(typeof(SkirmishUnitActionData), nameof(SkirmishUnitActionData.GetMaxUseCount))]
        class SkirmishUnitActionDataCooldownPatch1
        {
            static void Postfix(ref int __result, ref SkirmishUnitActionData __instance)
            {
                if (!configNoPlayerCooldown.Value)
                {
                    return;
                }
                if (__instance.Group != null)
                {
                    //Log("Original cooldown values: max use " + __result + " turn cooldown " + __instance.maxTurnCooldown + " group " + __instance.Group.name + " " + __instance.localizedName.value);
                }
                //Log("Skill name " + __instance.localizedName.value);
                __result = 0;
                __instance.maxTurnCooldown = 0;
            }
        }




        //configPlayerMovementMultiplier

        [HarmonyPatch(typeof(SkirmishUnitMovementActionBase<SkirmishUnitMovementActionState, SkirmishUnitMovementActionData>), "GetRange")]
        class SkirmishUnitMovementActionPatch1
        {
            static void Postfix(ref SkirmishUnitMovementActionBase<SkirmishUnitMovementActionState, SkirmishUnitMovementActionData> __instance, ref int __result)
            {
                if (configPlayerMovementMultiplier.Value <= 1)
                {
                    return;
                }
                if (__instance.Unit.Team.IsPlayerTeamOrAlly())
                {
                    //Log("Skirmish unit movement action range: " + __result);
                    __result = Mathf.RoundToInt(__result * configPlayerMovementMultiplier.Value);
                }

            }
        }

        //configNoAwakeing

        [HarmonyPatch(typeof(InvestigationEffectMissionDynasticAwakening), nameof(InvestigationEffectMissionDynasticAwakening.ApplyInvestigationEffect))]
        class MissionDynasticAwakeningPatch1
        {
            static bool Prefix(ref InvestigationEffectMissionDynasticAwakening __instance)
            {
                if (!configNoAwakeing.Value)
                {
                    return true;
                }
                if (__instance.value > 0)
                {
                    Log("Mission Awakening value: " + __instance.value);
                    __instance.value = 0;
                }
                return true;
            }
        }

        // Il2CppBulwarkStudios.Codex.Warmap.GlobalAwakeningInstance
        [HarmonyPatch(typeof(GlobalAwakeningInstance), nameof(GlobalAwakeningInstance.remove_onGlobalAwakeningLevelChanged))]
        class GlobalAwakeningPatch1
        {
            static bool Prefix(ref GlobalAwakeningInstance __instance)
            {
                if (!configNoAwakeing.Value)
                {
                    return true;
                }
                if (__instance.GlobalAwakeningLevel > 0)
                {
                    Log("Global Awakening value: " + __instance.GlobalAwakeningLevel);
                    __instance.GlobalAwakeningLevel = 0;
                }
                return true;
            }
        }

        // Il2CppBulwarkStudios.Codex.Warmap.GlobalAwakeningState
        [HarmonyPatch(typeof(GlobalAwakeningState), nameof(GlobalAwakeningState.GetCumulativeAwakeningLevels))]
        class GlobalAwakeningStatePatch1
        {
            static void Postfix(ref GlobalAwakeningState __instance, ref int __result)
            {
                if (!configNoAwakeing.Value)
                {
                    return;
                }
                if (__result > 0)
                {
                    Log("Global awakening state value: " + __result);
                    __result = 0;
                }
            }
        }

        //configInfiniteCog

        [HarmonyPatch(typeof(SkirmishCognitionSkillCostProvider), nameof(SkirmishCognitionSkillCostProvider.RequiredCognition), MethodType.Getter)]
        class CognitionCostPatch1
        {
            static void Postfix(ref SkirmishCognitionSkillCostProvider __instance, ref int __result)
            {
                if (!configInfiniteCog.Value)
                {
                    return;
                }
                if (__result > 0)
                {
                    //Log("Cognition skill cost provider value: " + __result);
                    __result = 0;
                } 
            }
        }
        [HarmonyPatch(typeof(SkirmishCognitionSkillCostProvider), nameof(SkirmishCognitionSkillCostProvider.ActionCognitionChanged))]
        class CognitionCostPatch2
        {
            static bool Prefix(ref SkirmishCognitionSkillCostProvider __instance)
            {
                if (!configInfiniteCog.Value)
                {
                    return true;
                }
                //Log("Cognition skill scalable cost prefix: " + __instance.scalableRequiredCognition.ToString() + " value " + __instance.scalableRequiredCognition.value + " modValue " + __instance.scalableRequiredCognition.modifiedValue + " value without modifiers " + __instance.scalableRequiredCognition.ValueWithoutModifier + " GetValue " + __instance.scalableRequiredCognition.GetValue());
                __instance.scalableRequiredCognition.SetValueWithoutNotify(0);
                return true;
            }
        }

        //configPlayerCognitionMultiplier
        [HarmonyPatch(typeof(StoryEventChoiceEffectCognitionModifier), nameof(StoryEventChoiceEffectCognitionModifier.ApplyStoryRewardEffect))]
        class EffectCognitionModifierPatch1
        {
            static bool Prefix(ref StoryEventChoiceEffectCognitionModifier __instance)
            {
                if (configPlayerCognitionMultiplier.Value <= 1)
                {
                    return true;
                }
                Log("Original choice effect cognition modifier: " + __instance.value);
                if (__instance.value > 0)
                    __instance.value = __instance.value * configPlayerCognitionMultiplier.Value;
                else
                    __instance.value = 0;
                return true;
            }
        }

        [HarmonyPatch(typeof(SkirmishCognitionEffectPipeline), nameof(SkirmishCognitionEffectPipeline.GetAdditionalCognition))]
        class AdditionalCognitionPatch1
        {
            static void Postfix(ref SkirmishCognitionEffectPipeline __instance, ref int __result)
            {
                if (configPlayerCognitionMultiplier.Value <= 1)
                {
                    return;
                }
                //Log("!! Original additional cognition: " + __result);
                if (__result > 0)
                    __result = __result * configPlayerCognitionMultiplier.Value;
            }
        }

        //configMissionRewardMultiplier

        [HarmonyPatch(typeof(MissionRewardEffectResource), nameof(MissionRewardEffectResource.ApplyReward))]
        class MissionRewardEffectResourcePatch1
        {
            static bool Prefix(ref MissionRewardEffectResource __instance)
            {
                if (configMissionRewardMultiplier.Value <= 1)
                {
                    return true;
                }
                if (__instance.value > 0)
                {
                    Log("Mission reward original value: " + __instance.value);
                    __instance.value = Mathf.RoundToInt(__instance.value * configMissionRewardMultiplier.Value);
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(InvestigationEffectMissionRewardModifier), nameof(InvestigationEffectMissionRewardModifier.ApplyMissionRewardEffect))]
        class InvestigationEffectMissionRewardModifierPatch1
        {
            static bool Prefix(ref InvestigationEffectMissionRewardModifier __instance)
            {
                if (configMissionRewardMultiplier.Value <= 1)
                {
                    return true;
                }
                if (__instance.value > 0)
                {
                    Log("Mission investigation reward original value: " + __instance.value);
                    __instance.value = Mathf.RoundToInt(__instance.value * configMissionRewardMultiplier.Value);
                }
                return true;
            }
        }

        // configMissionNoVigilance
        [HarmonyPatch(typeof(InvestigationEffectMissionVigilanceModifier), nameof(InvestigationEffectMissionVigilanceModifier.ApplyInvestigationEffect))]
        class InvestigationEffectMissionVigilanceModifierPatch1
        {
            static bool Prefix(ref InvestigationEffectMissionVigilanceModifier __instance)
            {
                if (!configMissionNoVigilance.Value)
                {
                    return true;
                }
                if (__instance.value > 0 && __instance.modifier == 0)
                {
                    Log("Mission investigation vigilance original value: " + __instance.value);
                    __instance.value = 0;
                }
                return true;
            }
        }

        // Il2CppBulwarkStudios.Codex.Common.NecronVigilanceInstance
        [HarmonyPatch(typeof(NecronVigilanceInstance), nameof(NecronVigilanceInstance.add_onNecronVigilanceLevelChanged))]
        class NecronVigilanceInstancePatch1
        {
            static bool Prefix(ref NecronVigilanceInstance __instance)
            {
                if (!configMissionNoVigilance.Value)
                {
                    return true;
                }
                if (__instance.VigilanceLevel > 0)
                {
                    Log("Vigilance original value prefix: " + __instance.VigilanceLevel);
                    __instance.VigilanceLevel = 0;
                }
                return true;
            }
            static void Postfix(ref NecronVigilanceInstance __instance)
            {
                if (!configMissionNoVigilance.Value)
                {
                    return;
                }
                if (__instance.VigilanceLevel > 0)
                {
                    Log("Vigilance original value postfix: " + __instance.VigilanceLevel);
                    __instance.VigilanceLevel = 0;
                }
            }
        }

        [HarmonyPatch(typeof(NecronVigilanceInstance), nameof(NecronVigilanceInstance.OnLoaded))]
        class NecronVigilanceInstancePatch2
        {
            static bool Prefix(ref NecronVigilanceInstance __instance)
            {
                if (!configMissionNoVigilance.Value)
                {
                    return true;
                }
                if (__instance.VigilanceLevel > 0)
                {
                    Log("Vigilance original value prefix: " + __instance.VigilanceLevel);
                    __instance.VigilanceLevel = 0;
                }
                return true;
            }
            static void Postfix(ref NecronVigilanceInstance __instance)
            {
                if (!configMissionNoVigilance.Value)
                {
                    return;
                }
                if (__instance.VigilanceLevel > 0)
                {
                    Log("Vigilance original value postfix: " + __instance.VigilanceLevel);
                    __instance.VigilanceLevel = 0;
                }
            }
        }


        // configStartCognition

        [HarmonyPatch(typeof(MissionMissionInstance), nameof(MissionMissionInstance.add_OnProgressionChanged))]
        class MissionMissionInstancePatch3
        {
            static bool Prefix(ref MissionMissionInstance __instance)
            {
                if (configStartCognition.Value <= 0)
                {
                    return true;
                }
                //Log("Mission starting cognition value before progression change : " + __instance.Cognition);
                if (__instance.Cognition < configStartCognition.Value)
                    __instance.Cognition = configStartCognition.Value;
                return true;

            }
            static void Postfix(ref MissionMissionInstance __instance)
            {
                if (configStartCognition.Value <= 0)
                {
                    return;
                }
                //Log("Mission starting cognition value after progression change : " + __instance.Cognition);
                if (__instance.Cognition < configStartCognition.Value)
                    __instance.Cognition = configStartCognition.Value;


            }
        }


        // MAIN MOD CLASSES

        public override void OnUpdate()
        {
            if (Event.current != null)
                if ((Event.current.keyCode == (configMenuToggle.Value)) && (Event.current.type == EventType.KeyDown))
                {
                    SwitchMenu();
                    //Log("Menu switched!");
                }
        }

        public override void OnGUI()
        {
            ShowMenu();
        }


        public static void SwitchMenu()
        {
            if (!showCheatsPopup)
            {
                lastLockMode = UnityEngine.Cursor.lockState;
                lastVisibleState = UnityEngine.Cursor.visible;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
                lastEventSys = EventSystem.current;
                lastInputModule = EventSystem.current.currentInputModule;
                lastEventSys.enabled = false;
                lastInputModule.DeactivateModule();
                jModEventSys.enabled = true;
                jModEventSys.m_CurrentInputModule?.ActivateModule();
            }
            else
            {
                UnityEngine.Cursor.lockState = lastLockMode;
                UnityEngine.Cursor.visible = lastVisibleState;
                Input.ResetInputAxes();
                jModEventSys.enabled = false;
                jModEventSys.currentInputModule?.DeactivateModule();
                lastEventSys.enabled = true;
                lastInputModule.ActivateModule();
                lastEventSys.m_CurrentInputModule = lastInputModule;
                MelonPreferences.Save();
            }
            showCheatsPopup = !showCheatsPopup;
        }

        public static void ShowMenu()
        {
            if (showCheatsPopup)
            {
                JModStyleT = GUI.skin.GetStyle("toggle");
                JModStyleT.fontSize = 16;
                JModStyleT.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
                JModStyleT.onNormal.textColor = JModColor;

                JModStyleB = GUI.skin.GetStyle("box");
                JModStyleB.alignment = TextAnchor.UpperCenter;
                JModStyleB.fontSize = 24;
                JModStyleB.fontStyle = FontStyle.Bold;
                JModStyleB.normal.textColor = JModColor;

                jModWindowRect = new Rect(Screen.width / 2 - 425, Screen.height / 2 - 425, 850, 850);
                _screenRect = new Rect(0, 0, Screen.width, Screen.height);

                GUI.BeginGroup(jModWindowRect);
                for (int i = 0; i < 5; i++)
                    GUI.Box(new Rect(0, 0, 850, 850), "", JModStyleB);

                GUI.Box(new Rect(0, 0, 850, 850), "BLESSINGS OF MARS", JModStyleB);

                var yAxis = 40;
                var xAxis = 20;
                GUI.Label(new Rect(xAxis, yAxis, 810, 20), "Rituals of Omnissiah (Toggle Mod Options)", JModStyleH);
                yAxis += 35;
                ShowBoolMenu(ref xAxis, ref yAxis, ref ToggleCategory);
                yAxis += 10;
                GUI.Label(new Rect(xAxis, yAxis, 810, 20), "Apocrypha Mechanica (Multipliers)", JModStyleH);
                yAxis += 45;
                ShowFloatMenu(ref xAxis, ref yAxis, ref MultiplierFloatCategory);
                ShowIntMenu(ref xAxis, ref yAxis, ref MultiplierIntCategory);
                yAxis += 15;

                if (GUI.Button(new Rect(325, 810, 200, 35), "Save settings and close"))
                {
                    SwitchMenu();
                }


                //Vector2 mousePosition = Mouse.current.position.value;
                Vector2 mousePosition = Input.mousePosition;
                mousePosition.y = Screen.height - mousePosition.y;

                if (GUI.Button(_screenRect, string.Empty, JModStyleBlank) && !jModWindowRect.Contains(mousePosition))
                {

                }

                if (jModWindowRect.Contains(mousePosition) && !((Event.current.keyCode == (configMenuToggle.Value)) && (Event.current.type == EventType.KeyDown)))
                {
                    Event.current.Use();
                }
                GUI.EndGroup();
            }
        }

        public static void ShowBoolMenu(ref int xAxis, ref int yAxis, ref MelonPreferences_Category cat)
        {
            foreach (MelonPreferences_Entry<bool> toggle in cat.Entries)
            {
                toggle.Value = GUI.Toggle(new Rect(xAxis, yAxis, 800, 20), toggle.Value, toggle.DisplayName, JModStyleT);
                    xAxis = 20;
                    yAxis += 35;
            }
        }

        public static void ShowFloatMenu(ref int xAxis, ref int yAxis, ref MelonPreferences_Category cat)
        {
            foreach (MelonPreferences_Entry<float> mult in cat.Entries)
            {
                string multLabel = mult.DisplayName;
                ValueRange<float> range;
                if (mult.Validator != null)
                    range = (ValueRange<float>)mult.Validator;
                else
                    range = new ValueRange<float>(1f, 20f);
                float step;
                if (range.MaxValue < 10)
                    step = 0.1f;
                else
                    step = 0.5f;
                multLabel += " (" + range.MinValue.ToString() + " - " + range.MaxValue.ToString() + ")";
                GUI.Label(new Rect(xAxis, yAxis, 680, 20), multLabel, JModStyleP);

                if (GUI.Button(new Rect(xAxis + 680, yAxis, 40, 20), " - "))
                {
                    if (mult.Value > range.MinValue)
                        mult.Value -= step;
                }
                GUI.Label(new Rect(xAxis + 730, yAxis, 40, 20), mult.Value.ToString("0.0"), JModStylePV);
                if (GUI.Button(new Rect(xAxis + 780, yAxis, 40, 20), " + "))
                {
                    if (mult.Value < range.MaxValue)
                        mult.Value += step;
                }

                yAxis += 35;
            }
        }
        public static void ShowIntMenu(ref int xAxis, ref int yAxis, ref MelonPreferences_Category cat)
        {
            foreach (MelonPreferences_Entry<int> mult in cat.Entries)
            {
                string multLabel = mult.DisplayName;
                ValueRange<int> range;
                if (mult.Validator != null)
                    range = (ValueRange<int>)mult.Validator;
                else
                    range = new ValueRange<int>(1, 20);
                multLabel += " (" + range.MinValue + " - " + range.MaxValue + ")";
                GUI.Label(new Rect(xAxis, yAxis, 680, 20), multLabel, JModStyleP);
                if (GUI.Button(new Rect(xAxis + 680, yAxis, 40, 20), " - "))
                {
                    if (mult.Value > range.MinValue)
                        mult.Value -= 1;
                }
                GUI.Label(new Rect(xAxis + 730, yAxis, 40, 20), mult.Value.ToString(), JModStylePV);
                if (GUI.Button(new Rect(xAxis + 780, yAxis, 40, 20), " + "))
                {
                    if (mult.Value < range.MaxValue)
                        mult.Value += 1;
                }
                yAxis += 35;
            }
        }

    }
}
