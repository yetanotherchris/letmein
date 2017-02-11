using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Configuration;

namespace Letmein.Web.Views.TagHelpers
{
	[HtmlTargetElement("configtext", TagStructure = TagStructure.WithoutEndTag, Attributes = "key")]
	public class ConfigTextTagHelper : TagHelper
    {
	    private readonly IConfigurationRoot _configuration;

	    public ConfigTextTagHelper(IConfigurationRoot configuration)
	    {
		    _configuration = configuration;
	    }

	    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	    {
		    string variableKey = context.AllAttributes["key"].Value.ToString();
			output.Content.AppendHtml(_configuration[variableKey]);
		    output.TagName = "";
		    return base.ProcessAsync(context, output);
	    }
    }
}
