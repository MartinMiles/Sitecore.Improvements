using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Resources;
using Sitecore.Rules;
using Sitecore.SecurityModel;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;

namespace SitecoreImprovements.DeviceEditorShortcuts
{
    public class DeviceEditorForm : Sitecore.Shell.Applications.Layouts.DeviceEditor.DeviceEditorForm
    {
        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            
            base.OnLoad(e);
            
            if (Context.ClientPage.IsEvent)
            {
                return;
            }

            DeviceID = WebUtil.GetQueryString("de");
            DeviceDefinition device = GetLayoutDefinition().GetDevice(DeviceID);
            
            if (device.Layout != null)
            {
                Layout.Value = device.Layout;
            }

            if (!Settings.Analytics.Enabled)
            {
                Personalize.Visible = false;
                Test.Visible = false;
            }
            else
            {
                Test.Visible = Policy.IsAllowed("Page Editor/Extended features/Testing");
                Personalize.Visible = Policy.IsAllowed("Page Editor/Extended features/Personalization");
            }

            Refresh();

            SelectedIndex = -1;
        }

        private void Refresh()
        {
            Renderings.Controls.Clear();
            Placeholders.Controls.Clear();
            
            Controls = new ArrayList();
            DeviceDefinition device = GetLayoutDefinition().GetDevice(DeviceID);

            if (device.Renderings == null)
            {
                SheerResponse.SetOuterHtml("Renderings", (System.Web.UI.Control)Renderings);
                SheerResponse.SetOuterHtml("Placeholders", (System.Web.UI.Control)Placeholders);
                SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
            }
            else
            {
                int selectedIndex = SelectedIndex;

                RenderRenderings(device, selectedIndex, 0);
                RenderPlaceholders(device);
                UpdateRenderingsCommandsState();
                UpdatePlaceholdersCommandsState();

                SheerResponse.SetOuterHtml("Renderings", (System.Web.UI.Control)Renderings);
                SheerResponse.SetOuterHtml("Placeholders", (System.Web.UI.Control)Placeholders);
                SheerResponse.Eval("if (!scForm.browser.isIE) { scForm.browser.initializeFixsizeElements(); }");
            }
        }

        private void UpdateRenderingsCommandsState()
        {
            if (SelectedIndex < 0)
            {
                ChangeButtonsState(true);
            }
            else
            {
                ArrayList renderings = GetLayoutDefinition().GetDevice(DeviceID).Renderings;
                if (renderings == null)
                {
                    ChangeButtonsState(true);
                }
                else
                {
                    RenderingDefinition definition = renderings[SelectedIndex] as RenderingDefinition;
                    if (definition == null)
                    {
                        ChangeButtonsState(true);
                    }
                    else
                    {
                        ChangeButtonsState(false);
                        Personalize.Disabled = !string.IsNullOrEmpty(definition.MultiVariateTest);
                        Test.Disabled = HasRenderingRules(definition);
                    }
                }
            }
        }

        private static bool HasRenderingRules(RenderingDefinition definition)
        {
            if (definition.Rules == null)
            {
                return false;
            }

            IEnumerable<XElement> rules = new RulesDefinition(definition.Rules.ToString()).GetRules();
            if (rules == null)
            {
                return false;
            }

            foreach (XContainer xcontainer in rules)
            {
                XElement xelement = Enumerable.FirstOrDefault(xcontainer.Descendants("actions"));
                if (xelement != null && Enumerable.Any(xelement.Descendants()))
                {
                    return true;
                }
            }
            return false;
        }

        private void ChangeButtonsState(bool disable)
        {
            Personalize.Disabled = disable;
            btnEdit.Disabled = disable;
            btnChange.Disabled = disable;
            btnRemove.Disabled = disable;
            MoveUp.Disabled = disable;
            MoveDown.Disabled = disable;
            Test.Disabled = disable;
        }

        private void UpdatePlaceholdersCommandsState()
        {
            phEdit.Disabled = string.IsNullOrEmpty(UniqueId);
            phRemove.Disabled = string.IsNullOrEmpty(UniqueId);
        }

        private void RenderPlaceholders(DeviceDefinition deviceDefinition)
        {
            Assert.ArgumentNotNull(deviceDefinition, "deviceDefinition");
            ArrayList placeholders = deviceDefinition.Placeholders;

            if (placeholders == null)
            {
                return;
            }

            foreach (PlaceholderDefinition placeholderDefinition in placeholders)
            {
                Item obj = null;
                string metaDataItemId = placeholderDefinition.MetaDataItemId;

                if (!string.IsNullOrEmpty(metaDataItemId))
                {
                    obj = Client.ContentDatabase.GetItem(metaDataItemId);
                }

                XmlControl xmlControl = Resource.GetWebControl("DeviceRendering") as XmlControl;
                Assert.IsNotNull(xmlControl, typeof(XmlControl));
                
                Placeholders.Controls.Add((System.Web.UI.Control)xmlControl);
                ID id = ID.Parse(placeholderDefinition.UniqueId);

                if (placeholderDefinition.UniqueId == UniqueId)
                {
                    xmlControl["Background"] = "#D0EBF6";
                }

                string str = "ph_" + id.ToShortID();
                xmlControl["ID"] = str;
                xmlControl["Header"] = placeholderDefinition.Key;
                xmlControl["Click"] = ("OnPlaceholderClick(\"" + placeholderDefinition.UniqueId + "\")");
                xmlControl["DblClick"] = "device:editplaceholder";
                xmlControl["Icon"] = obj == null ? "Imaging/24x24/layer_blend.png" : obj.Appearance.Icon;
            }
        }

        private void RenderRenderings(DeviceDefinition deviceDefinition, int selectedIndex, int index)
        {
            Assert.ArgumentNotNull(deviceDefinition, "deviceDefinition");
            ArrayList renderings = deviceDefinition.Renderings;
            
            if (renderings == null)
            {
                return;
            }

            foreach (RenderingDefinition renderingDefinition in renderings)
            {
                if (renderingDefinition.ItemID != null)
                {
                    Item obj = Client.ContentDatabase.GetItem(renderingDefinition.ItemID);
                    
                    XmlControl xmlControl = Resource.GetWebControl("DeviceRendering") as XmlControl;
                    Assert.IsNotNull(xmlControl, typeof(XmlControl));
                    
                    HtmlGenericControl htmlGenericControl1 = new HtmlGenericControl("div");
                    htmlGenericControl1.Style.Add("padding", "0");
                    htmlGenericControl1.Style.Add("margin", "0");
                    htmlGenericControl1.Style.Add("border", "0");
                    htmlGenericControl1.Style.Add("position", "relative");
                    htmlGenericControl1.Controls.Add(xmlControl);

                    string uniqueId = Control.GetUniqueID("R");
                    Renderings.Controls.Add((System.Web.UI.Control)htmlGenericControl1);
                    htmlGenericControl1.ID = Control.GetUniqueID("C");
                    xmlControl["Click"] = ("OnRenderingClick(\"" + index + "\")");
                    xmlControl["DblClick"] = "device:edit";
                    
                    if (index == selectedIndex)
                    {
                        xmlControl["Background"] = "#D0EBF6";
                    }

                    Controls.Add(uniqueId);
                    
                    if (obj != null)
                    {
                        xmlControl["ID"] = uniqueId;
                        xmlControl["Icon"] = obj.Appearance.Icon;
                        xmlControl["Header"] = obj.DisplayName;
                        xmlControl["HeaderID"] = obj.ID;
                        xmlControl["Placeholder"] = WebUtil.SafeEncode(renderingDefinition.Placeholder);

                        var dataSourceItem = Client.ContentDatabase.GetItem(renderingDefinition.Datasource);
                        if (dataSourceItem != null)
                        {
                            xmlControl["DataSourcePath"] = WebUtil.SafeEncode(dataSourceItem.Paths.FullPath);
                            xmlControl["DataSourceID"] = WebUtil.SafeEncode(renderingDefinition.Datasource);
                        }
                    }
                    else
                    {
                        xmlControl["ID"] = uniqueId;
                        xmlControl["Icon"] = "Applications/24x24/forbidden.png";
                        xmlControl["Header"] = "Unknown rendering";
                        xmlControl["Placeholder"] = string.Empty;
                    }

                    if (Settings.Analytics.Enabled && renderingDefinition.Rules != null && !renderingDefinition.Rules.IsEmpty)
                    {
                        int num = Enumerable.Count(renderingDefinition.Rules.Elements("rule"));
                        
                        if (num > 1)
                        {
                            HtmlGenericControl htmlGenericControl2 = new HtmlGenericControl("span");
                            
                            if (num > 9)
                            {
                                htmlGenericControl2.Attributes["class"] = "scConditionContainer scLongConditionContainer";
                            }
                            else
                            {
                                htmlGenericControl2.Attributes["class"] = "scConditionContainer";
                            }

                            htmlGenericControl2.InnerText = num.ToString();
                            htmlGenericControl1.Controls.Add(htmlGenericControl2);
                        }
                    }

                    ++index;
                }
            }
        }

        private static LayoutDefinition GetLayoutDefinition()
        {
            string sessionString = WebUtil.GetSessionString(GetSessionHandle());
            Assert.IsNotNull(sessionString, "layout definition");
            
            return LayoutDefinition.Parse(sessionString);
        }

        private static string GetSessionHandle()
        {
            return "SC_DEVICEEDITOR";
        }
    }
}
