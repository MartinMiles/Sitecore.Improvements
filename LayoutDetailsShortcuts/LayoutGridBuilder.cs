using System.Collections;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Reflection;
using Sitecore.Resources;
using Sitecore.Web.UI;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XmlControls;

namespace SitecoreImprovements.LayoutDetailsShortcuts
{
    public class LayoutGridBuilder : Sitecore.Shell.Web.UI.LayoutGridBuilder
    {
        public void BuildGrid(System.Web.UI.Control parent)
        {
            Assert.ArgumentNotNull(parent, "parent");

            LayoutDefinition layout = null;

            var gridPanel = new GridPanel
            {
                RenderAs = RenderAs.Literal,
                Width = Unit.Parse("100%")
            };

            gridPanel.Attributes["Class"] = Class;
            gridPanel.Attributes["CellSpacing"] = "2";
            gridPanel.Attributes["id"] = ID;
            parent.Controls.Add(gridPanel);

            string @string = StringUtil.GetString(Value);

            if (@string.Length > 0)
            {
                layout = LayoutDefinition.Parse(@string);
            }

            foreach (DeviceItem deviceItem in Client.ContentDatabase.Resources.Devices.GetAll())
            {
                BuildDevice(gridPanel, layout, deviceItem);
            }
        }

        private void BuildDevice(GridPanel grid, LayoutDefinition layout, DeviceItem deviceItem)
        {
            Assert.ArgumentNotNull(grid, "grid");
            Assert.ArgumentNotNull(deviceItem, "deviceItem");

            var xmlControl = string.IsNullOrEmpty(OpenDeviceClick) 
                ? Resource.GetWebControl("LayoutFieldDeviceReadOnly") as XmlControl 
                : Resource.GetWebControl("LayoutFieldDevice") as XmlControl;

            Assert.IsNotNull(xmlControl, typeof(XmlControl));
            
            grid.Controls.Add(xmlControl);
            
            string str1 = StringUtil.GetString(OpenDeviceClick).Replace("$Device", deviceItem.ID.ToString());
            string str2 = StringUtil.GetString(CopyToClick).Replace("$Device", deviceItem.ID.ToString());

            ReflectionUtil.SetProperty(xmlControl, "DeviceName", deviceItem.DisplayName);
            ReflectionUtil.SetProperty(xmlControl, "DeviceIcon", deviceItem.InnerItem.Appearance.Icon);
            ReflectionUtil.SetProperty(xmlControl, "DblClick", str1);
            ReflectionUtil.SetProperty(xmlControl, "Copy", str2);
            
            string str3 = string.Format("<span style=\"color:#999999\">{0}</span>", Translate.Text("[No layout specified]"));

            int index = 0;
            int num = 0;
            var parent1 = xmlControl["ControlsPane"] as System.Web.UI.Control;
            var parent2 = xmlControl["PlaceholdersPane"] as System.Web.UI.Control;
            
            if (layout != null)
            {
                DeviceDefinition device = layout.GetDevice(deviceItem.ID.ToString());
                string layout1 = device.Layout;
                
                if (!string.IsNullOrEmpty(layout1))
                {
                    Item obj = Client.ContentDatabase.GetItem(layout1);
                    if (obj != null)
                    {
                        str3 = Images.GetImage(obj.Appearance.Icon, 16, 16, "absmiddle", "0px 4px 0px 0px", obj.ID.ToString()) + obj.DisplayName;
                    }
                }

                ArrayList renderings = device.Renderings;
                if (renderings != null && renderings.Count > 0)
                {
                    Border border = new Border();
                    Context.ClientPage.AddControl(parent1, border);
                    foreach (RenderingDefinition renderingDefinition in renderings)
                    {
                        int conditionsCount = 0;
                        
                        if (renderingDefinition.Rules != null && !renderingDefinition.Rules.IsEmpty)
                        {
                            conditionsCount = Enumerable.Count(renderingDefinition.Rules.Elements("rule"));
                        }

                        BuildRendering(border, device, renderingDefinition, index, conditionsCount);
                        ++index;
                    }
                }

                ArrayList placeholders = device.Placeholders;
                if (placeholders != null && placeholders.Count > 0)
                {
                    Border border = new Border();
                    Context.ClientPage.AddControl(parent2, border);

                    foreach (PlaceholderDefinition placeholderDefinition in placeholders)
                    {
                        BuildPlaceholder(border, device, placeholderDefinition);
                        ++num;
                    }
                }
            }

            ReflectionUtil.SetProperty(xmlControl, "LayoutName", str3);

            if (index == 0)
            {
                var noRenderingsLabel = string.Format("<span style=\"color:#999999\">{0}</span>", Translate.Text("[No renderings specified.]"));
                Context.ClientPage.AddControl(parent1, new LiteralControl(noRenderingsLabel));
            }

            if (num != 0)
            {
                return;
            }

            var noPlaceholdersLabel = string.Format("<span style=\"color:#999999\">{0}</span>", Translate.Text("[No placeholder settings were specified]"));
            Context.ClientPage.AddControl(parent2, new LiteralControl(noPlaceholdersLabel));
        }

        private void BuildRendering(Border border, DeviceDefinition deviceDefinition, RenderingDefinition renderingDefinition, int index, int conditionsCount)
        {
            Assert.ArgumentNotNull(border, "border");
            Assert.ArgumentNotNull(deviceDefinition, "deviceDefinition");
            Assert.ArgumentNotNull(renderingDefinition, "renderingDefinition");

            string itemId = renderingDefinition.ItemID;
            if (itemId == null)
            {
                return;
            }

            Item obj = Client.ContentDatabase.GetItem(itemId);
            if (obj == null)
            {
                return;
            }

            string displayName = obj.DisplayName;
            string icon = obj.Appearance.Icon;
            string str1 = string.Empty;
            string str2 = displayName;

            if (str1.Length > 0 && str1 != "content")
            {
                str2 = string.Format("{0} {1} {2}.", str2, Translate.Text("in"), str1);
            }

            if (Settings.Analytics.Enabled && conditionsCount > 1)
            {
                str2 += string.Format("<span class=\"{0}\">{1}</span>", conditionsCount > 9 
                        ? "scConditionContainer scLongConditionContainer" 
                        : "scConditionContainer", 
                    conditionsCount);
            }

            Border border1 = new Border();
            border1.Style.Add("float", "left");

            Border borderX = new Border();

            var renderingIconAnchor = new HtmlGenericControl
            {
                InnerHtml = "<a onclick='window.open(\"/sitecore/shell/Applications/Content%20Editor.aspx?fo=" +
                    itemId + "\",\"myWin\",\"width=800,height=600\"); return false;' taget='_blank' class='linkIcon'>" +
                    Images.GetImage(icon, 16, 16, "absmiddle", "0px 4px 0px 0px") + "</a>",
            };
            renderingIconAnchor.Style.Add("float", "left");

            borderX.Controls.Add(renderingIconAnchor);
            borderX.Controls.Add(border1);

            var clearBoth = new HtmlGenericControl("div");
            clearBoth.Style.Add("clear","both");
            borderX.Controls.Add(clearBoth);

            border.Controls.Add(borderX);

            string str3 = StringUtil.GetString(EditRenderingClick).Replace("$Device", deviceDefinition.ID).Replace("$Index", index.ToString());

            if (!string.IsNullOrEmpty(str3))
            {
                border1.RollOver = true;
                border1.Class = "scRollOver";
                border1.Click = str3;
            }

            var literal = new Sitecore.Web.UI.HtmlControls.Literal(
                string.Format("<div class='scRendering' style='padding:2px;position:relative'>{0}</div>", str2)
                );

            border1.Controls.Add(literal);
        }

        /// <summary>
        /// Copy from Sitecore as as, just to make compile.
        /// </summary>
        private void BuildPlaceholder(Border border, DeviceDefinition deviceDefinition, PlaceholderDefinition placeholderDefinition)
        {
            Assert.ArgumentNotNull(border, "border");
            Assert.ArgumentNotNull(deviceDefinition, "deviceDefinition");
            Assert.ArgumentNotNull(placeholderDefinition, "placeholderDefinition");

            string metaDataItemId = placeholderDefinition.MetaDataItemId;
            
            Border border1 = new Border();
            border.Controls.Add(border1);

            string str1 = StringUtil.GetString(EditPlaceholderClick)
                .Replace("$Device", deviceDefinition.ID)
                .Replace("$UniqueID", placeholderDefinition.UniqueId);

            Assert.IsNotNull(metaDataItemId, "placeholder id");
            
            Item obj = Client.ContentDatabase.GetItem(metaDataItemId);
            
            string str2;
            if (obj != null)
            {
                string displayName = obj.DisplayName;
                str2 = Images.GetImage(obj.Appearance.Icon, 16, 16, "absmiddle", "0px 4px 0px 0px") + displayName;
            }
            else
            {
                str2 = Images.GetImage("Imaging/16x16/layer_blend.png", 16, 16, "absmiddle", "0px 4px 0px 0px") + placeholderDefinition.Key;
            }

            if (!string.IsNullOrEmpty(str1))
            {
                border1.RollOver = true;
                border1.Class = "scRollOver";
                border1.Click = str1;
            }

            var literal = new Sitecore.Web.UI.HtmlControls.Literal(string.Format("<div style=\"padding:2\">{0}</div>", str2));
            border1.Controls.Add(literal);
        }
    }
}
