using System;
using System.Activities;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using Microsoft.TeamFoundation.Build.Workflow.Services;
using Microsoft.TeamFoundation.Client;

namespace QueueNewBuilds
{
	[BuildActivity(HostEnvironmentOption.Agent)]
	public sealed class QueueNewBuild : CodeActivity
	{
		// The Team Project that the build definition belongs to.
		[RequiredArgument]
		public InArgument<IBuildDetail> BuildDetail { get; set; }

		[RequiredArgument]
		public InArgument<String[]> TfsProjectAndBuildDefinition { get; set; }


		protected override void Execute(CodeActivityContext context)
		{
			String[] dirty = context.GetValue(this.TfsProjectAndBuildDefinition);
			IBuildDetail buildDetail = context.GetValue(this.BuildDetail);

			var pds = Parse(dirty);
			//var workspace = buildDetail.BuildDefinition.Workspace;
			IBuildServer buildServer = buildDetail.BuildServer;

			foreach (var pd in pds)
			{
				try
				{
					string message = string.Format("Queue new build \"{0}\"-\"{1}\"", pd.TfsProject, pd.BuildDefinition);
					context.TrackBuildMessage(message);

					IBuildDefinition buildDef = buildServer.GetBuildDefinition(pd.TfsProject, pd.BuildDefinition);
					buildServer.QueueBuild(buildDef);
				}
				catch (Exception ex)
				{
					string message = string.Format("Queue new build error \"{0}\"-\"{1}\", Exception : \"{2}\"",
							pd.TfsProject, pd.BuildDefinition, ex.Message);
					context.TrackBuildWarning(message);
				}
			}
		}

		private IEnumerable<ProjectDefinition> Parse(string[] dirty)
		{
			if (dirty == null)
				yield break;

			foreach (var item in dirty)
			{
				var t = item.Split(';');
				if (t.Length == 2)
				{
					ProjectDefinition pd = new ProjectDefinition();
					pd.TfsProject = t[0].Trim();
					pd.BuildDefinition = t[1].Trim();
					yield return pd;
				}
			}
		}

		class ProjectDefinition
		{
			public string TfsProject { get; set; }
			public string BuildDefinition { get; set; }
		}
	}
}
