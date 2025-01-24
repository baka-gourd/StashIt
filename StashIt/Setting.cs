using System.Collections.Generic;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;

namespace StashIt
{
    [FileLocation(nameof(StashIt))]
    public class Setting : ModSetting
    {
        public string Version => StashIt.Version;
        public Setting(IMod mod) : base(mod)
        {
            
        }

        public override void SetDefaults()
        {
        }
    }

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Stash It" },
                { m_Setting.GetOptionLabelLocaleID(nameof(m_Setting.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(m_Setting.Version)), "Version" },
            };
        }

        public void Unload()
        {

        }
    }
}
