using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ViendStanceChangeSkill.Modules
{
    public class Content : IContentPackProvider
    {
        public static SkillDef EnterStanceChange, ExitStanceChange;
        public static BuffDef StanceChangeBuff;

        internal ContentPack contentPack = new ContentPack();
        public string identifier => "com.Moffein.ViendStanceChangeSkil";

        public static List<SkillDef> skillDefs = new List<SkillDef>();
        public static List<Type> entityStates = new List<Type>();
        public static List<BuffDef> buffDefs = new List<BuffDef>();

        public void Initialize()
        {
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;
            contentPack.skillDefs.Add(skillDefs.ToArray());
            contentPack.entityStateTypes.Add(entityStates.ToArray());
            contentPack.buffDefs.Add(buffDefs.ToArray());

            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }
}
