#region using
using Sitecore;
using Sitecore.Collections;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Diagnostics.PerformanceCounters;
using Sitecore.Pipelines;
using Sitecore.Pipelines.RenderLayout;
using Sitecore.SecurityModel;
using Sitecore.SecurityModel.License;
using Sitecore.Web;
using Sitecore.Web.UI.WebControls;
using Sitecore.Layouts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using Sc = Sitecore;
#endregion

namespace SmartPlaceholderSettings.Layouts
{
    public static class PageContextExtension
    {
        private const string SmartPlaceholderTemplateId = "{E79C79E1-0516-4AE0-BBB5-825959D3DE17}";
        private const string PlaceholderKeyFieldId = "{7256BDAB-1FD2-49DD-B205-CB4873D2917C}";
        private const string DYNAMICPLACEHOLDERREGEX = @"(.+)_[\d\w]{8}\-([\d\w]{4}\-){3}[\d\w]{12}";
        private const string Index_Name = "sitecore_master_index";

        public static Item[] GetPlacehoderItemsSearch(this Sc.Layouts.PageContext pageContext, string placeholderKey, Database database, string layoutDefinition)
        {
            Assert.ArgumentNotNull((object)placeholderKey, "placeholderKey");
            Assert.ArgumentNotNull((object)database, "database");
            Assert.ArgumentNotNull((object)layoutDefinition, "layoutDefinition");

            Item[] result = null;

            try
            {
                var regex = new Regex(DYNAMICPLACEHOLDERREGEX);
                var match = regex.Match(placeholderKey);

                if (match.Success && match.Groups.Count > 0)
                {
                    placeholderKey = match.Groups[1].Value;
                    placeholderKey = placeholderKey.Substring(placeholderKey.LastIndexOf('/') + 1);
                }

                if(placeholderKey.Contains('/'))
                    placeholderKey = placeholderKey.Substring(placeholderKey.LastIndexOf('/') + 1);

                using (var context = Sc.ContentSearch.ContentSearchManager.GetIndex(Index_Name).CreateSearchContext())
                {
                    var query = context.GetQueryable<Sc.ContentSearch.SearchTypes.SearchResultItem>()
                        .Where(
                            p => p.TemplateId == new Sc.Data.ID(SmartPlaceholderTemplateId)
                        );

                    result = query.ToList()
                        .Select(p => p.GetItem())
                        .Where(
                            p => p.Fields[new Sc.Data.ID(PlaceholderKeyFieldId)].Value.Equals(placeholderKey)
                        ).ToArray();
                }
            }
            catch(Exception ex)
            {
                Log.Error("Smart Placeholder Settings: PageContextExtension - GetPlaceholderItemsSearch", ex, typeof(Sc.Layouts.PageContext));
            }

            return result;
        }
    }

}
