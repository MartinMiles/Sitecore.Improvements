using System;
using Sitecore;
using Sitecore.Data.Events;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Sites;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;

namespace SitecoreImprovements.LayoutDetailsShortcuts
{
    public class LayoutDetailsForm : Sitecore.Shell.Applications.ContentManager.Dialogs.LayoutDetails.LayoutDetailsForm
    {
        protected override void OnLoad(EventArgs e)
        {
            Assert.CanRunApplication("Content Editor/Ribbons/Chunks/Layout");
            Assert.ArgumentNotNull(e, "e");

            base.OnLoad(e);
            Tabs.OnChange += (EventHandler)((sender, args) => Refresh());

            if (!Context.ClientPage.IsEvent)
            {
                Item currentItem = GetCurrentItem();
                Assert.IsNotNull(currentItem, "Item not found");
                Layout = LayoutField.GetFieldValue(currentItem.Fields[FieldIDs.LayoutField]);
                LayoutDelta = currentItem.Fields[FieldIDs.FinalLayoutField].GetValue(false, false);
                ToggleVisibilityOfControlsOnFinalLayoutTab(currentItem);

                Refresh();
            }


            SiteContext site = Context.Site;
            if (site == null)
            {
                return;
            }

            site.Notifications.ItemSaved += ItemSavedNotification;
        }

        private void ItemSavedNotification(object sender, ItemSavedEventArgs args)
        {
            VersionCreated = true;
            ToggleVisibilityOfControlsOnFinalLayoutTab(args.Item);
            SheerResponse.SetDialogValue(GetDialogResult());
        }

        private static Item GetCurrentItem()
        {
            return Client.ContentDatabase.GetItem(
                WebUtil.GetQueryString("id"), 
                Language.Parse(WebUtil.GetQueryString("la")), 
                Sitecore.Data.Version.Parse(WebUtil.GetQueryString("vs")));
        }

        private TabType ActiveTab
        {
            get
            {
                switch (Tabs.Active)
                {
                    case 0: return TabType.Shared;
                    case 1: return TabType.Final;
                    default: return TabType.Unknown;
                }
            }
        }

        private void Refresh()
        {
            RenderLayoutGridBuilder(GetActiveLayout(), ActiveTab == TabType.Final 
                ? (Sitecore.Web.UI.HtmlControls.Control)FinalLayoutPanel 
                : (Sitecore.Web.UI.HtmlControls.Control)LayoutPanel);
        }

        private void RenderLayoutGridBuilder(string layoutValue, Sitecore.Web.UI.HtmlControls.Control renderingContainer)
        {
            string str = renderingContainer.ID + "LayoutGrid";

            LayoutGridBuilder layoutGridBuilder = new LayoutGridBuilder
            {
                ID = str,
                Value = layoutValue,
                EditRenderingClick = "EditRendering(\"$Device\", \"$Index\")",
                EditPlaceholderClick = "EditPlaceholder(\"$Device\", \"$UniqueID\")",
                OpenDeviceClick = "OpenDevice(\"$Device\")",
                CopyToClick = "CopyDevice(\"$Device\")"
            };

            renderingContainer.Controls.Clear();
            layoutGridBuilder.BuildGrid(renderingContainer);

            if (!Context.ClientPage.IsEvent)
            {
                return;
            }

            SheerResponse.SetOuterHtml(renderingContainer.ID, renderingContainer);
            SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
        }

        private enum TabType
        {
            Shared,
            Final,
            Unknown,
        }
    }
}
