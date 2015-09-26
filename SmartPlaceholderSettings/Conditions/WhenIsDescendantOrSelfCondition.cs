using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Rules;
using Sitecore.Rules.Conditions;

using SmartPlaceholderSettings.Rules;

namespace SmartPlaceholderSettings.Conditions
{
    public class WhenIsDescendantOrSelfCondition<T> : WhenCondition<T> where T : PlaceholderSettingsRuleContext
    {
        private ID itemId;

        public ID ItemId
        {
            get { return this.itemId; }
            set { Assert.ArgumentNotNull((object)value, "value"); this.itemId = value; }
        }

        public WhenIsDescendantOrSelfCondition()
        {
            this.itemId = ID.Null;
        }

        protected override bool Execute(T ruleContext)
        {
            Assert.ArgumentNotNull((object)ruleContext, "ruleContext");

            Item parent = ruleContext.Item;
            if (parent == null)
                return false;
            for (; parent != null; parent = parent.Parent)
            {
                if (parent.ID == this.ItemId)
                    return true;
            }

            return false;
        }
    }
}
