// <sitecore_web_root>\Website\sitecore\shell\Applications\Content Manager\Dialogs\LayoutDetails\LayoutDetails.js
function runJs(span, event) {

    var target = event.target;

    if (target.tagName == "IMG") {

        var guid = target.getAttribute('alt');

        if (guid != "") {

            span.parentNode.setAttribute("onclick", "return;");
            var onclick = 'window.open(\"/sitecore/shell/Applications/Content%20Editor.aspx?fo=' + guid + '\",\"myWin\",\"width=800,height=600\"); ';
            eval(onclick);
        }
    }
}