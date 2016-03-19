using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.PlatformAbstractions;

namespace AdvancedTagHelpers.TagHelpers
{
    [HtmlTargetElement("script", Attributes = "src")]
    public class ScriptTrackerTagHelper : TagHelper
    {
        private readonly IFileProvider _wwwroot;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly string _wwwrootFolder;

        public ScriptTrackerTagHelper(
            IApplicationEnvironment appEnv,
            IHostingEnvironment env,
            IUrlHelperFactory urlHelperFactory)
        {
            _wwwroot = env.WebRootFileProvider;
            _wwwrootFolder = env.WebRootPath.Substring(appEnv.ApplicationBasePath.Length + 1);
            _urlHelperFactory = urlHelperFactory;
        }

        [ViewContext]
        public ViewContext ViewContext { get; set; }

        public string Src { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var dependencyContext = GetScriptContext(Src);

            var scriptContexts = (List<ScriptContext>)context.Items[typeof(BodyTagHelper)];
            scriptContexts.Add(dependencyContext);

            // The body will render the script tag.
            output.SuppressOutput();
        }

        private ScriptContext GetScriptContext(string src)
        {
            var dependencies = new List<ScriptContext>();
            var urlHelper = _urlHelperFactory.GetUrlHelper(ViewContext);
            var resolvedSrc = urlHelper.Content(src);
            var fileInfo = _wwwroot.GetFileInfo(resolvedSrc);
            using (var readStream = fileInfo.CreateReadStream())
            using (var reader = new StreamReader(readStream))
            {
                const string startMarker = "/// <reference path=\"";
                const string endMarker = "\" />";

                string line;
                while ((line = reader.ReadLine()) != null && line.StartsWith(startMarker))
                {
                    var dependencySrc = line.Substring(startMarker.Length, line.Length - startMarker.Length - endMarker.Length);
                    var resolvedDependencySrc = dependencySrc.Replace(_wwwrootFolder + "/", string.Empty);
                    var dependenciesContext = GetScriptContext(resolvedDependencySrc);
                    dependencies.Add(dependenciesContext);
                }
            }

            return new ScriptContext
            {
                Src = resolvedSrc,
                Dependencies = dependencies
            };
        }
    }
}
