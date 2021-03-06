﻿using System;
using System.Linq;
using System.Reflection;
using Sitecore.Web.UI;
using System.Collections.Specialized;
using System.Web.UI;
using Blade.Views;
using System.Web.Compilation;

namespace Blade.Razor
{
	/// <summary>
	/// This is a Sitecore Rendering Type class that tells Sitecore how to handle a Razor view as a Rendering when it comes across one
	/// </summary>
	/// <remarks>
	/// Must be registered in the renderingControls section of the Sitecore configuration. A template that derives from an existing Rendering template
	/// must also be created in Sitecore with a name matching the entry in the renderingControls config.
	/// </remarks>
	public class SitecoreRazorRenderingType : RenderingType
	{
		public override Control GetControl(NameValueCollection parameters, bool assert)
		{
			var viewPath = parameters["viewPath"];

			var control = GetControl(viewPath);

			Sitecore.Diagnostics.Assert.IsNotNull(control, "Resolved Razor control was null");

			((WebControl)control).Parameters = parameters["properties"];
			
			return control;
		}

		/// <summary>
		/// Gets a Control that will render a given Razor view path
		/// </summary>
		public static Control GetControl(string viewPath)
		{
			Sitecore.Diagnostics.Assert.IsNotNullOrEmpty(viewPath, "ViewPath cannot be empty. The Rendering item in Sitecore needs to have a view path set.");

			Type viewType = BuildManager.GetCompiledType(viewPath);
			PropertyInfo typedModelProperty = viewType.GetProperties().FirstOrDefault(x => x.PropertyType != typeof (object) && x.Name == "Model");
			Type viewModelType = typedModelProperty != null ? typedModelProperty.PropertyType : typeof (object);

			var renderingType = typeof(RazorViewShim<>).MakeGenericType(viewModelType);

			var shim = (IRazorViewShim)Activator.CreateInstance(renderingType);

			shim.ViewPath = viewPath;

			return shim as Control;
		}
	}
}
