using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sitecore.Rules.Actions;

using SmartPlaceholderSettings.Rules;

namespace SmartPlaceholderSettings.Actions
{
    public class AllowControlsAction<T> : RuleAction<T> where T : PlaceholderSettingsRuleContext
    {
        public override void Apply(T ruleContext)
        {
            ruleContext.AllowSelectedControls = true;
        }
    }
}
