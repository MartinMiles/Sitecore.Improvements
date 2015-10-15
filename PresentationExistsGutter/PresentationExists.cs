using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Layouts;
using Sitecore.Shell.Applications.ContentEditor.Gutters;

namespace SitecoreImprovements.Gutters
{
    public class PresentationExists : GutterRenderer
    {
        protected override GutterIconDescriptor GetIconDescriptor(Item item)
        {
            if (item != null)
            {
                var layoutField = item.Fields[Sitecore.FieldIDs.LayoutField];
                var layoutDefinition = LayoutDefinition.Parse(LayoutField.GetFieldValue(layoutField));

                if (layoutDefinition != null && layoutDefinition.Devices.Count > 0)
                {
                    GutterIconDescriptor gutterIconDescriptor = new GutterIconDescriptor
                    {
                        Icon = "Applications/32x32/window_colors.png",
                        Tooltip = Translate.Text("Presentation is set for this item.")
                    };

                    if (item.Access.CanWrite() && !item.Appearance.ReadOnly)
                    {
                        gutterIconDescriptor.Click = string.Format("item:setlayoutdetails(id={0})", item.ID);
                    }
                    return gutterIconDescriptor;
                }
            }

            return null;
        }
    }
}
