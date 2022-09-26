// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable RedundantDefaultMemberInitializer
// ReSharper disable ConvertPropertyToExpressionBody
// ReSharper disable PartialMethodWithSinglePart
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToConstant.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local
// ReSharper disable ConvertToAutoProperty
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global
// ReSharper disable IdentifierTypo


/**************************************************************
AUTO GENERATED BY Fabrica Application Studio
DO NOT MODIFY THIS FILE
**************************************************************/

using System.ComponentModel;
using Fabrica.Models.Serialization;
using Fabrica.Models.Support;
using Fabrica.Persistence.Audit;
using Fabrica.Utilities.Text;
using Newtonsoft.Json;

namespace Fabrica.Work.Persistence.Entities
{

	[JsonObject(MemberSerialization.OptIn)]
	[Audit]	
	[Model( Alias=nameof(WorkTopic) )]
    public partial class WorkTopic : BaseMutableModel<WorkTopic>, IRootModel, IExplorableModel, INotifyPropertyChanged
    {

		public static class Meta
		{
			public static readonly string Id = "Id";
			public static readonly string Uid = "Uid";
			public static readonly string Environment = "Environment";
			public static readonly string TenantUid = "TenantUid";
			public static readonly string Topic = "Topic";
			public static readonly string Description = "Description";
			public static readonly string Synchronous = "Synchronous";
			public static readonly string ClientName = "ClientName";
			public static readonly string Path = "Path";
			public static readonly string FullUrl = "FullUrl";

		}



        public WorkTopic(): this(false)
        {

        }

        public WorkTopic( bool added )
        {

			SuspendTracking( m =>
			{
			});

            if( added )
                Added();

        }

		
		private long _id = 0;			
		[ModelMeta(Scope=PropertyScope.Exclude)]
        public override  long Id
		{ 
			get { return _id; } 
			protected set { _id = value; } 
		} 


		[JsonProperty("Uid")]
		private string _uid = Base62Converter.NewGuid();
		[ModelMeta(Scope=PropertyScope.Immutable)]		
        public override  string Uid
		{ 
			get { return _uid; } 
			set { _uid = value; } 
		} 


		[JsonProperty("Environment")]
		private string _environment = "";
		 		
        public  string Environment 
		{ 
			get { return _environment; } 
			set { _environment = value; } 
		} 

		[JsonProperty("TenantUid")]
		private string _tenantUid = "";
		 		
        public  string TenantUid 
		{ 
			get { return _tenantUid; } 
			set { _tenantUid = value; } 
		} 

		[JsonProperty("Topic")]
		private string _topic = "";
		 		
        public  string Topic 
		{ 
			get { return _topic; } 
			set { _topic = value; } 
		} 

		[JsonProperty("Description")]
		private string _description = "";
		 		
        public  string Description 
		{ 
			get { return _description; } 
			set { _description = value; } 
		} 

		[JsonProperty("Synchronous")]
		private bool _synchronous = false;
		 		
        public  bool Synchronous 
		{ 
			get { return _synchronous; } 
			set { _synchronous = value; } 
		} 

		[JsonProperty("ClientName")]
		private string _clientName = "";
		 		
        public  string ClientName 
		{ 
			get { return _clientName; } 
			set { _clientName = value; } 
		} 

		[JsonProperty("Path")]
		private string _path = "";
		 		
        public  string Path 
		{ 
			get { return _path; } 
			set { _path = value; } 
		} 

		[JsonProperty("FullUrl")]
		private string _fullUrl = "";
		 		
        public  string FullUrl 
		{ 
			get { return _fullUrl; } 
			set { _fullUrl = value; } 
		} 


	}		

	
}