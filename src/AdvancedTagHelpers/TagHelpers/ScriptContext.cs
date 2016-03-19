using System.Collections.Generic;

namespace AdvancedTagHelpers.TagHelpers
{
    public class ScriptContext
    {
        public string Src { get; set; }

        public IList<ScriptContext> Dependencies { get; set; }
    }
}
