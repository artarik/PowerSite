﻿using System;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using PowerSite.DataModel;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace PowerSite.Builtin.Renderers
{
	[Export(typeof(IRenderer))]
	[ExportMetadata("Extension", "cshtml")]
	public class RazorRenderer : IRenderer
	{
		public string Render(string template, object data)
		{
			string result = null;

			var config = new TemplateServiceConfiguration() { Resolver = new RazorRenderer.TemplateResolver() };

			config.Namespaces.Add("System.IO");
			config.Namespaces.Add("RazorEngine.Text");

			using (var service = new TemplateService(config))
			{
				Razor.SetTemplateService(service);

				var cacheName = template.GetHashCode().ToString(CultureInfo.InvariantCulture);

				try
				{
					result = Razor.Parse(template, data, cacheName);
				}
				catch (TemplateCompilationException e)
				{
					foreach (var error in e.Errors)
					{
						Console.Error.WriteLine(error);
					}
				}
			}

			return result;
		}

		private class TemplateResolver : ITemplateResolver
		{
			public string Resolve(string name)
			{
				var id = Path.GetFileNameWithoutExtension(name) ?? "default";
			
				var extension = (Path.GetExtension(name) ?? ".cshtml").TrimStart('.');

				var layout = RenderingState.Current.Layouts[id];
			
				if (!layout.Extension.Equals(String.IsNullOrEmpty(extension) ? "cshtml" : extension, StringComparison.OrdinalIgnoreCase))
				{
					Console.WriteLine("Resolved wrong. Layout {0} extention {1}", name, extension);
				}
				return layout.RawContent;
			}
		}
	}
}