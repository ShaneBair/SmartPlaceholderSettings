using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sitecore.Data.Items;
using Sitecore.Rules;

namespace SmartPlaceholderSettings.Rules
{
    public class PlaceholderSettingsRuleContext : RuleContext
    {
        public bool AllowSelectedControls { get; set; }
        public bool DisallowSelectedControls { get; set; }

        public PlaceholderSettingsRuleContext()
            : base()
        {
            AllowSelectedControls = false;
            DisallowSelectedControls = false;
        }
    }
}
