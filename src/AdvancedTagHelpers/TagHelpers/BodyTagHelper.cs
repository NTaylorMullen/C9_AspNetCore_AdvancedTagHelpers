using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdvancedTagHelpers.TagHelpers
{
    public class BodyTagHelper : TagHelper
    {
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            var scriptContexts = new List<ScriptContext>();

            ViewContext.ViewData[typeof(BodyTagHelper).FullName] = scriptContexts;
            await output.GetChildContentAsync();

            var renderedScripts = new HashSet<string>();
            foreach (var scriptContext in scriptContexts)
            {
                RenderScriptContext(scriptContext, output.PostContent, renderedScripts);
            }
        }

        private void RenderScriptContext(ScriptContext context, TagHelperContent content, HashSet<string> renderedScripts)
        {
            // Render dependency scripts depth first.
            foreach (var contextDependency in context.Dependencies)
            {
                RenderScriptContext(contextDependency, content, renderedScripts);
            }

            // Only need to render the dependency if it hasn't been rendered already.
            if (renderedScripts.Add(context.Src))
            {
                content.AppendHtml($"<script src=\"{context.Src}\"></script>{Environment.NewLine}");
            }
        }
    }
}