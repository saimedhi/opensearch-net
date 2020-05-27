// ███╗   ██╗ ██████╗ ████████╗██╗ ██████╗███████╗
// ████╗  ██║██╔═══██╗╚══██╔══╝██║██╔════╝██╔════╝
// ██╔██╗ ██║██║   ██║   ██║   ██║██║     █████╗  
// ██║╚██╗██║██║   ██║   ██║   ██║██║     ██╔══╝  
// ██║ ╚████║╚██████╔╝   ██║   ██║╚██████╗███████╗
// ╚═╝  ╚═══╝ ╚═════╝    ╚═╝   ╚═╝ ╚═════╝╚══════╝
// -----------------------------------------------
//  
// This file is automatically generated 
// Please do not edit these files manually
// Run the following in the root of the repos:
//
// 		*NIX 		:	./build.sh codegen
// 		Windows 	:	build.bat codegen
//
// -----------------------------------------------
// ReSharper disable RedundantUsingDirective
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Elasticsearch.Net.Specification.RollupApi
{
	///<summary>Request options for DeleteJob <para>https://www.elastic.co/guide/en/elasticsearch/reference/master/rollup-delete-job.html</para></summary>
	public class DeleteRollupJobRequestParameters : RequestParameters<DeleteRollupJobRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.DELETE;
		public override bool SupportsBody => false;
	}

	///<summary>Request options for GetJob <para>https://www.elastic.co/guide/en/elasticsearch/reference/master/rollup-get-job.html</para></summary>
	public class GetRollupJobRequestParameters : RequestParameters<GetRollupJobRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
	}

	///<summary>Request options for GetCapabilities <para>https://www.elastic.co/guide/en/elasticsearch/reference/master/rollup-get-rollup-caps.html</para></summary>
	public class GetRollupCapabilitiesRequestParameters : RequestParameters<GetRollupCapabilitiesRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
	}

	///<summary>Request options for GetIndexCapabilities <para>https://www.elastic.co/guide/en/elasticsearch/reference/master/rollup-get-rollup-index-caps.html</para></summary>
	public class GetRollupIndexCapabilitiesRequestParameters : RequestParameters<GetRollupIndexCapabilitiesRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.GET;
		public override bool SupportsBody => false;
	}

	///<summary>Request options for CreateJob <para>https://www.elastic.co/guide/en/elasticsearch/reference/master/rollup-put-job.html</para></summary>
	public class CreateRollupJobRequestParameters : RequestParameters<CreateRollupJobRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.PUT;
		public override bool SupportsBody => true;
	}

	///<summary>Request options for Search <para>https://www.elastic.co/guide/en/elasticsearch/reference/master/rollup-search.html</para></summary>
	public class RollupSearchRequestParameters : RequestParameters<RollupSearchRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => true;
		///<summary>Indicates whether hits.total should be rendered as an integer or an object in the rest search response</summary>
		public bool? TotalHitsAsInteger
		{
			get => Q<bool? >("rest_total_hits_as_int");
			set => Q("rest_total_hits_as_int", value);
		}

		///<summary>Specify whether aggregation and suggester names should be prefixed by their respective types in the response</summary>
		public bool? TypedKeys
		{
			get => Q<bool? >("typed_keys");
			set => Q("typed_keys", value);
		}
	}

	///<summary>Request options for StartJob <para>https://www.elastic.co/guide/en/elasticsearch/reference/master/rollup-start-job.html</para></summary>
	public class StartRollupJobRequestParameters : RequestParameters<StartRollupJobRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => false;
	}

	///<summary>Request options for StopJob <para>https://www.elastic.co/guide/en/elasticsearch/reference/master/rollup-stop-job.html</para></summary>
	public class StopRollupJobRequestParameters : RequestParameters<StopRollupJobRequestParameters>
	{
		public override HttpMethod DefaultHttpMethod => HttpMethod.POST;
		public override bool SupportsBody => false;
		///<summary>Block for (at maximum) the specified duration while waiting for the job to stop. Defaults to 30s.</summary>
		public TimeSpan Timeout
		{
			get => Q<TimeSpan>("timeout");
			set => Q("timeout", value);
		}

		///<summary>True if the API should block until the job has fully stopped, false if should be executed async. Defaults to false.</summary>
		public bool? WaitForCompletion
		{
			get => Q<bool? >("wait_for_completion");
			set => Q("wait_for_completion", value);
		}
	}
}