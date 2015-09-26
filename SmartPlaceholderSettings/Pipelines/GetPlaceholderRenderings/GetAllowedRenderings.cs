using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using Sc = Sitecore;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Pipelines.GetPlaceholderRenderings;
using Sitecore.Rules;

using SmartPlaceholderSettings.Layouts;
using SmartPlaceholderSettings.Rules;

namespace SmartPlaceholderSettings.Pipelines.GetPlaceholderRenderings
{
    public class GetAllowedRenderings : Sc.Pipelines.GetPlaceholderRenderings.GetAllowedRenderings
    {
        private const string Allowed_Controls_Rules_Field = "AllowedControlsRules";

        protected Item[] _placeholderItems;
        protected List<Item> disAllowList;
        protected bool allowedControlsSpecified;

        public new void Process(GetPlaceholderRenderingsArgs args)
        {
            Assert.IsNotNull((object)args, "args");
            try
            {
                this.Setup(args);
                this.GetPlaceholderItems(args);

                if (_placeholderItems != null && _placeholderItems.Length > 0)
                {
                    args.HasPlaceholderSettings = true;
                    
                    foreach (Item placeholderItem in _placeholderItems)
                    {
                        ProcessIndividualPlaceholder(placeholderItem, args);
                    }

                    RemoveDisallowedRenderings(args);
                }
            }
            catch(Exception ex)
            {
                Log.Error("Smart Placeholder Settings: GetAllowedRenderings - Process", ex, this);
            }
        }

        private void RemoveDisallowedRenderings(GetPlaceholderRenderingsArgs args)
        {
            // Loop through and remove disallowed renderings
            if (disAllowList.Count > 0)
            {
                foreach (Item rendering in disAllowList)
                {
                    args.PlaceholderRenderings.RemoveAll(r => r.ID == rendering.ID);
                }
                string infoMessage = "\n\tContextItem: " + GetContextItem(args.PlaceholderKey).Paths.Path +
                        "\n\tKey: " + args.PlaceholderKey +
                        "\n\tRenderings: " + disAllowList.Select(i => i.Paths.Path).Aggregate((i, j) => i + "; " + j);
                Log.Info("Smart Placeholder Settings: Renderings Removed" + infoMessage, this);
            }
        }

        private void ProcessIndividualPlaceholder(Item placeholderItem, GetPlaceholderRenderingsArgs args)
        {
            PlaceholderSettingsRuleContext context = this.EvaluatePlaceholderRules(placeholderItem, args.PlaceholderKey);
            List<Item> smallList = new List<Item>();

            if (context.AllowSelectedControls)
            {
                // Add any additional renderings to the args.PlaceholderRenderings list
                smallList = this.GetRenderings(placeholderItem, out allowedControlsSpecified);
                if (smallList.Count > 0)
                {
                    string infoMessage = "\n\tContextItem: " + GetContextItem(args.PlaceholderKey).Paths.Path +
                        "\n\tSmart Placeholder Item: " + placeholderItem.Paths.Path +
                        "\n\tKey: " + args.PlaceholderKey +
                        "\n\tRenderings: " + smallList.Select(i => i.Paths.Path).Aggregate((i, j) => i + "; " + j);
                    Log.Info("Smart Placeholder Settings: Renderings Added" + infoMessage, this);

                    args.PlaceholderRenderings.AddRange((IEnumerable<Item>)smallList);
                    if (allowedControlsSpecified)
                        args.Options.ShowTree = false;
                }
            }
            else if (context.DisallowSelectedControls)
            {
                // Create list of disallowed renderings to remove at the end.
                smallList = this.GetRenderings(placeholderItem, out allowedControlsSpecified);
                if (smallList.Count > 0)
                {
                    disAllowList.AddRange((IEnumerable<Item>)smallList);
                }
            }
        }
        private void Setup(GetPlaceholderRenderingsArgs args)
        {
            if (args.PlaceholderRenderings == null)
                args.PlaceholderRenderings = new List<Item>();

            _placeholderItems = null;
            disAllowList = new List<Item>();
        }
        private void GetPlaceholderItems(GetPlaceholderRenderingsArgs args)
        {
            if (ID.IsNullOrEmpty(args.DeviceId))
            {
                _placeholderItems = Client.Page.GetPlacehoderItemsSearch(args.PlaceholderKey, args.ContentDatabase, args.LayoutDefinition);
            }
            else
            {
                using (new DeviceSwitcher(args.DeviceId, args.ContentDatabase))
                    _placeholderItems = Client.Page.GetPlacehoderItemsSearch(args.PlaceholderKey, args.ContentDatabase, args.LayoutDefinition);
            }
        }

        private PlaceholderSettingsRuleContext EvaluatePlaceholderRules(Item placeholder, string placeholderKey)
        {
            PlaceholderSettingsRuleContext context = new PlaceholderSettingsRuleContext();

            context.Item = this.GetContextItem(placeholderKey);

            foreach (
                Rule<PlaceholderSettingsRuleContext> rule in
                RuleFactory.GetRules<PlaceholderSettingsRuleContext>(new[] { placeholder }, Allowed_Controls_Rules_Field).Rules)
            {
                if (rule.Condition != null)
                {
                    var passed = rule.Evaluate(context);
                    if (passed)
                        rule.Execute(context);
                }
            }

            return context;
        }

        private Item GetContextItem(string placeholderKey)
        {
            HttpContext httpContext = HttpContext.Current;
            Item contextItem = Sc.Context.Item;

            if (contextItem == null)
            {
                string itemId = GetContextItemId();
                Sc.Data.Database master = Sc.Configuration.Factory.GetDatabase("master");
                contextItem = master.GetItem(new Sc.Data.ID(itemId));
            }

            return contextItem;
        }
        private string GetContextItemId()
        {
            string result = string.Empty;
            if (Sc.Context.Request.QueryString["sc_itemid"] != null)
            {
                // Initial Page Load
                result = Sc.Context.Request.GetQueryString("sc_itemid");
            }
            else
            {
                // This is an additional request where the original item id is passed as part of another querystring attribute
                var valueList = HttpUtility.ParseQueryString(Sc.Context.Request.GetQueryString("url"));
                //valueList[0] = valueList[0].Replace("/?", ""); first query string parameter is getting /? added and jacking stuff up
                //valueList[0] = valueList[0].Replace("/?", "");
                if (valueList["sc_itemid"] != null)
                    result = valueList["sc_itemid"];
                else
                    result = valueList["/?sc_itemid"];
            }

            return result;
        }
    }
}
