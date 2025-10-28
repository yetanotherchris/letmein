using System;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Letmein.Api.Controllers
{
	// This attribute can be used to decorate Store() if 4mb of text is too small.
	// Thanks to http://stackoverflow.com/q/38357108/21574
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequestFormSizeLimitAttribute : Attribute, IAuthorizationFilter, IOrderedFilter
	{
		private readonly FormOptions _formOptions;
		public int Order { get; set; }

		public RequestFormSizeLimitAttribute(int valueCountLimit)
		{
			_formOptions = new FormOptions
			{
				ValueCountLimit = valueCountLimit
			};
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			IFeatureCollection features = context.HttpContext.Features;
			var formFeature = features.Get<IFormFeature>();

			if (formFeature?.Form == null)
			{
				features.Set<IFormFeature>(new FormFeature(context.HttpContext.Request, _formOptions));
			}
		}
	}
}