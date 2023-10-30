using BepInEx;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using System;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;
using ViendStanceChangeSkill.Modules;

namespace ViendStanceChangeSkill
{
    [BepInDependency("com.RiskyLives.RiskyMod", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Moffein.ViendStanceChangeSkill", "ViendStanceChangeSkill", "1.0.0")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [R2API.Utils.R2APISubmoduleDependency(nameof(RecalculateStatsAPI))]
    public class ViendStanceChangeSkillPlugin : BaseUnityPlugin
    {
        private static bool compatRiskyModLoaded = false;
        public static float corruptDamageMult = 0.75f;
        public static PluginInfo pluginInfo;

        private void Awake()
        {
            pluginInfo = this.Info;
            compatRiskyModLoaded = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.RiskyLives.RiskyMod");

            //Config();
            Setup();

            Tokens.LoadLanguage();
            new Modules.Content().Initialize();
        }

        /*private void Config()
        {

        }*/

        private void Setup()
        {
            Content.entityStates.Add(typeof(EntityStates.VoidSurvivor.JokerMode.EnterJokerMode));
            Content.entityStates.Add(typeof(EntityStates.VoidSurvivor.JokerMode.ExitJokerMode));

            AddStanceChangeMachine();
            CreateSkillDefs();
            DisableCorruptMeter();
            CorruptStatMod();

            On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.OnExit += CorruptMode_OnExit;
        }

        private void CorruptMode_OnExit(On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.orig_OnExit orig, EntityStates.VoidSurvivor.CorruptMode.CorruptMode self)
        {
            orig(self);
            if (self.isAuthority)
            {
                if (self.characterBody)
                {
                    ViendSkillDataComponent vc = self.characterBody.GetComponent<ViendSkillDataComponent>();
                    if (vc)
                    {
                        if (self.skillLocator)
                        {
                            if (self.skillLocator.primary)
                            {
                                self.skillLocator.primary.stock = vc.s1Stock;
                                self.skillLocator.primary.rechargeStopwatch = vc.s1RechargeStopwatch;
                            }
                            if (self.skillLocator.secondary)
                            {
                                self.skillLocator.secondary.stock = vc.s2Stock;
                                self.skillLocator.secondary.rechargeStopwatch = vc.s2RechargeStopwatch;
                            }
                            if (self.skillLocator.utility)
                            {
                                self.skillLocator.utility.stock = vc.s3Stock;
                                self.skillLocator.utility.rechargeStopwatch = vc.s3RechargeStopwatch;
                            }
                            if (self.skillLocator.special)
                            {
                                self.skillLocator.special.stock = vc.s4Stock;
                                self.skillLocator.special.rechargeStopwatch = vc.s4RechargeStopwatch;
                            }
                        }

                        Destroy(vc);
                    }
                }
            }
        }

        private void AddStanceChangeMachine()
        {
            GameObject viendBodyObject = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab").WaitForCompletion();
            EntityStateMachine stanceStateMachine = viendBodyObject.AddComponent<EntityStateMachine>();
            stanceStateMachine.customName = "Stance";
            stanceStateMachine.initialStateType = new SerializableEntityStateType(typeof(EntityStates.BaseState));
            stanceStateMachine.mainStateType = new SerializableEntityStateType(typeof(EntityStates.BaseState));

            NetworkStateMachine nsm = viendBodyObject.GetComponent<NetworkStateMachine>();
            nsm.stateMachines = nsm.stateMachines.Append(stanceStateMachine).ToArray();
        }

        private void CreateSkillDefs()
        {
            SkillFamily specialSkillFamily = Addressables.LoadAssetAsync<SkillFamily>("RoR2/DLC1/VoidSurvivor/VoidSurvivorSpecialFamily.asset").WaitForCompletion();

            SkillDef crushHealth = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/CrushHealth.asset").WaitForCompletion();
            SkillDef crushCorruption = Addressables.LoadAssetAsync<SkillDef>("RoR2/DLC1/VoidSurvivor/CrushCorruption.asset").WaitForCompletion();

            SkillDef enterStanceChange = ScriptableObject.CreateInstance<SkillDef>();
            enterStanceChange.activationState = new SerializableEntityStateType(typeof(EntityStates.VoidSurvivor.JokerMode.EnterJokerMode));
            enterStanceChange.activationStateMachineName = "Stance";
            enterStanceChange.baseMaxStock = 1;
            enterStanceChange.baseRechargeInterval = 12f;
            enterStanceChange.beginSkillCooldownOnSkillEnd = true;
            enterStanceChange.canceledFromSprinting = false;
            enterStanceChange.cancelSprintingOnActivation = false;
            enterStanceChange.forceSprintDuringState = false;
            enterStanceChange.fullRestockOnAssign = true;
            enterStanceChange.icon = crushHealth.icon;
            enterStanceChange.interruptPriority = InterruptPriority.Any;
            enterStanceChange.isCombatSkill = false;
            enterStanceChange.keywordTokens = new string[] { };
            enterStanceChange.mustKeyPress = true;
            enterStanceChange.rechargeStock = 1;
            enterStanceChange.requiredStock = 1;
            enterStanceChange.stockToConsume = 1;
            enterStanceChange.resetCooldownTimerOnUse = false;
            enterStanceChange.skillDescriptionToken = "VIENDJOKERMODE_ENTER_SKILL_DESCRIPTION";
            enterStanceChange.skillNameToken = "VIENDJOKERMODE_ENTER_SKILL_NAME";
            enterStanceChange.skillName = "ViendStanceChange";
            (enterStanceChange as ScriptableObject).name = enterStanceChange.skillName;
            Content.skillDefs.Add(enterStanceChange);
            Content.EnterStanceChange = enterStanceChange;

            SkillDef exitStanceChange = ScriptableObject.CreateInstance<SkillDef>();
            exitStanceChange.activationState = new SerializableEntityStateType(typeof(EntityStates.VoidSurvivor.JokerMode.ExitJokerMode));
            exitStanceChange.activationStateMachineName = "Stance";
            exitStanceChange.baseMaxStock = 1;
            exitStanceChange.baseRechargeInterval = 12f;
            exitStanceChange.beginSkillCooldownOnSkillEnd = true;
            exitStanceChange.canceledFromSprinting = false;
            exitStanceChange.cancelSprintingOnActivation = false;
            exitStanceChange.forceSprintDuringState = false;
            exitStanceChange.fullRestockOnAssign = true;
            exitStanceChange.icon = crushCorruption.icon;
            exitStanceChange.interruptPriority = InterruptPriority.Skill;
            exitStanceChange.isCombatSkill = false;
            exitStanceChange.keywordTokens = new string[] { };
            exitStanceChange.mustKeyPress = true;
            exitStanceChange.rechargeStock = 1;
            exitStanceChange.requiredStock = 1;
            exitStanceChange.stockToConsume = 1;
            exitStanceChange.resetCooldownTimerOnUse = false;
            exitStanceChange.skillDescriptionToken = "Society.";
            exitStanceChange.skillNameToken = "VIENDJOKERMODE_EXIT_SKILL_DESCRIPTION";
            exitStanceChange.skillName = "VIENDJOKERMODE_EXIT_SKILL_NAME";
            (exitStanceChange as ScriptableObject).name = exitStanceChange.skillName;
            Content.skillDefs.Add(exitStanceChange);
            Content.ExitStanceChange = exitStanceChange;
            EntityStates.VoidSurvivor.JokerMode.EnterJokerMode.exitSkillDef = exitStanceChange;

            Array.Resize(ref specialSkillFamily.variants, specialSkillFamily.variants.Length + 1);
            specialSkillFamily.variants[specialSkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = enterStanceChange,
                unlockableDef = null,
                viewableNode = new ViewablesCatalog.Node(enterStanceChange.skillName, false, null)
            };
        }

        private void DisableCorruptMeter()
        {
            On.EntityStates.VoidSurvivor.CorruptMode.UncorruptedMode.FixedUpdate += UncorruptedMode_FixedUpdate;
            On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.FixedUpdate += CorruptMode_FixedUpdate;
            On.RoR2.VoidSurvivorController.UpdateUI += DisableCorruptUI;
            On.RoR2.VoidSurvivorController.FixedUpdate += DisableCorruptAnimator;
        }

        private void DisableCorruptAnimator(On.RoR2.VoidSurvivorController.orig_FixedUpdate orig, VoidSurvivorController self)
        {
            orig(self);
            if (self.characterAnimator && HasStanceChange(self.characterBody))
            {
                float fractionOverride = self.isCorrupted ? 1f : 0f;
                self.characterAnimator.SetFloat("corruptionFraction", fractionOverride);
            }
        }

        private void DisableCorruptUI(On.RoR2.VoidSurvivorController.orig_UpdateUI orig, VoidSurvivorController self)
        {
            if (!HasStanceChange(self.characterBody))
            {
                orig(self);
            }
            else
            {
                if (self.overlayController != null)
                {
                    self.overlayController.onInstanceAdded -= self.OnOverlayInstanceAdded;
                    self.overlayController.onInstanceRemove -= self.OnOverlayInstanceRemoved;
                    self.fillUiList.Clear();
                    RoR2.HudOverlay.HudOverlayManager.RemoveOverlay(self.overlayController);
                    self.overlayController = null;
                }
            }
        }

        private void CorruptMode_FixedUpdate(On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.orig_FixedUpdate orig, EntityStates.VoidSurvivor.CorruptMode.CorruptMode self)
        {
            if (!HasStanceChange(self.skillLocator)) orig(self);
        }

        private void UncorruptedMode_FixedUpdate(On.EntityStates.VoidSurvivor.CorruptMode.UncorruptedMode.orig_FixedUpdate orig, EntityStates.VoidSurvivor.CorruptMode.UncorruptedMode self)
        {
            if (!HasStanceChange(self.skillLocator)) orig(self);
        }

        private void CorruptStatMod()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.EntityStates.VoidSurvivor.Weapon.FireCorruptHandBeam.OnEnter += FireCorruptHandBeam_OnEnter;
            On.EntityStates.VoidSurvivor.Weapon.FireCorruptDisks.FireProjectiles += FireCorruptDisks_FireProjectiles;
        }

        private void FireCorruptDisks_FireProjectiles(On.EntityStates.VoidSurvivor.Weapon.FireCorruptDisks.orig_FireProjectiles orig, EntityStates.VoidSurvivor.Weapon.FireCorruptDisks self)
        {
            float origDamage = self.damageStat;
            if (HasStanceChange(self.skillLocator))
            {
                self.damageStat *= ViendStanceChangeSkillPlugin.corruptDamageMult;
            }

            orig(self);

            self.damageStat = origDamage;
        }

        private void FireCorruptDisks_OnEnter(On.EntityStates.VoidSurvivor.Weapon.FireCorruptDisks.orig_OnEnter orig, EntityStates.VoidSurvivor.Weapon.FireCorruptDisks self)
        {
            throw new NotImplementedException();
        }

        private void FireCorruptHandBeam_OnEnter(On.EntityStates.VoidSurvivor.Weapon.FireCorruptHandBeam.orig_OnEnter orig, EntityStates.VoidSurvivor.Weapon.FireCorruptHandBeam self)
        {
            orig(self);
            if (HasStanceChange(self.skillLocator))
            {
                self.damageStat *= ViendStanceChangeSkillPlugin.corruptDamageMult;
            }
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(DLC1Content.Buffs.VoidSurvivorCorruptMode) && HasStanceChange(sender))
            {
                if (ViendStanceChangeSkillPlugin.RiskyModViendArmorStripEnabled()) args.armorAdd -= 100f;
                args.armorAdd -= 25f;
            }
        }

        public static bool RiskyModViendArmorStripEnabled()
        {
            if (compatRiskyModLoaded)
            {
                return RiskyModViendArmorStripEnabledInternal();
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool RiskyModViendArmorStripEnabledInternal()
        {
            return RiskyMod.Survivors.DLC1.VoidFiend.VoidFiendCore.removeCorruptArmor;
        }

        public static bool HasStanceChange(CharacterBody body)
        {
            return body && HasStanceChange(body.skillLocator);
        }

        public static bool HasStanceChange(SkillLocator skillLocator)
        {
            return skillLocator && skillLocator.special && (skillLocator.special.baseSkill == Content.EnterStanceChange);
        }
    }
}
