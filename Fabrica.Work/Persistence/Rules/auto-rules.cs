// ReSharper disable CommentTypo
// ReSharper disable EmptyConstructor
// ReSharper disable StringLiteralTypo
// ReSharper disable UnusedMember.Global


/**************************************************************
AUTO GENERATED BY Fabrica Application Studio
DO NOT MODIFY THIS FILE
**************************************************************/


using Fabrica.Rules;
using Fabrica.Rules.Validators;
using Fabrica.Work.Persistence.Entities;

namespace Fabrica.Work.Persistence.Rules
{

    public sealed class AutoWorkTopicRules : RuleBuilder<WorkTopic>
    {


		public AutoWorkTopicRules()
		{

			AddValidation("UidIsRequired")
				.Assert( m => m.Uid ).Required()
				.Otherwise( "Uid is required" );


			AddValidation("EnvironmentIsRequired")
				.Assert( m => m.Environment ).Required()
				.Otherwise( "Environment is required" );


			AddValidation("TenantUidIsRequired")
				.Assert( m => m.TenantUid ).Required()
				.Otherwise( "TenantUid is required" );


			AddValidation("TopicIsRequired")
				.Assert( m => m.Topic ).Required()
				.Otherwise( "Topic is required" );


			AddValidation("DescriptionIsRequired")
				.Assert( m => m.Description ).Required()
				.Otherwise( "Description is required" );

		}

	}

}