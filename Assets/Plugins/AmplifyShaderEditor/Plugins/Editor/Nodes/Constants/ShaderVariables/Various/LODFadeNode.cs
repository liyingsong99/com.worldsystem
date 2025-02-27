


using System;
using UnityEngine;
using UnityEditor;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( 
#if !WB_LANGUAGE_CHINESE
"LOD Fade"
#else
"LOD褪色"
#endif
,            /*<!C>*/
#if !WB_LANGUAGE_CHINESE
"Miscellaneous"
#else
"其他"
#endif
/*<C!>*/, 
#if !WB_LANGUAGE_CHINESE
"LODFadeNode"
#else
"LODFade节点"
#endif
)]
	public sealed class LODFadeNode : ConstVecShaderVariable
	{
		[SerializeField]
		private bool m_legacyBehavior = false;

		private const string LegacyVarName = "legacyFadeVal";
		private const string LegacyVarValue = "(( unity_LODFade.x < 0 ) ? ( 1 + unity_LODFade.x ) : ( unity_LODFade.x ))";

		private const string LegacyVarLabel = 
#if !WB_LANGUAGE_CHINESE
"Legacy Behavior"
#else
"遗留行为"
#endif
;
		private const string LegacyVarInfo = "Prior to Unity 2019 values given by unity_LODFade.x/Fade[0...1] port were always positive and complemented each other between LOD Groups.\n" +
												"Now fade-out is represented with positive values and fade-in with negative ones.\n"+
												"Toggling Legacy Behavior on internally checks for negative values and calculate complement result.";


		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			ChangeOutputName( 1, 
#if !WB_LANGUAGE_CHINESE
"Fade[0...1]"
#else
"褪色[0…1]"
#endif
);
			ChangeOutputName( 2, 
#if !WB_LANGUAGE_CHINESE
"Fade[16Lvl]"
#else
"褪色[16级]"
#endif
);
			ChangeOutputName( 3, 
#if !WB_LANGUAGE_CHINESE
"Unused"
#else
"未使用"
#endif
);
			ChangeOutputName( 4, 
#if !WB_LANGUAGE_CHINESE
"Unused"
#else
"未使用"
#endif
);
			m_value = "unity_LODFade";
			m_previewShaderGUID = "fcd4d93f57ffc51458d4ade10df2fdb4";
			m_autoWrapProperties = true;
		}

		public override string GenerateShaderForOutput( int outputId , ref MasterNodeDataCollector dataCollector , bool ignoreLocalvar )
		{
			string result = base.GenerateShaderForOutput( outputId , ref dataCollector , ignoreLocalvar );
			if( m_legacyBehavior && outputId == 1)
			{
				if( m_outputPorts[ 1 ].IsLocalValue( dataCollector.PortCategory ) )
					return m_outputPorts[ 1 ].LocalValue( dataCollector.PortCategory );

				dataCollector.AddLocalVariable( UniqueId , PrecisionType.Float , WirePortDataType.FLOAT , LegacyVarName , LegacyVarValue );
				m_outputPorts[ 1 ].SetLocalValue( LegacyVarName , dataCollector.PortCategory );

				return m_outputPorts[ 1 ].LocalValue( dataCollector.PortCategory );
			}
			else
			{
				return result;
			}
		}

		public override void DrawProperties()
		{
			base.DrawProperties();
			m_legacyBehavior = EditorGUILayoutToggle( LegacyVarLabel , m_legacyBehavior );
			EditorGUILayout.HelpBox( LegacyVarInfo , MessageType.Info );
		}

		public override void RefreshExternalReferences()
		{
			base.RefreshExternalReferences();
			if( !m_outputPorts[ 0 ].IsConnected )
			{
				m_outputPorts[ 0 ].Visible = false;
				m_sizeIsDirty = true;
			}

			if( !m_outputPorts[ 3 ].IsConnected )
			{
				m_outputPorts[ 3 ].Visible = false;
				m_sizeIsDirty = true;
			}

			if( !m_outputPorts[ 4 ].IsConnected )
			{
				m_outputPorts[ 4 ].Visible = false;
				m_sizeIsDirty = true;
			}
		}

		public override void ReadFromString( ref string[] nodeParams )
		{
			base.ReadFromString( ref nodeParams );
			if( UIUtils.CurrentShaderVersion() > 18902 )
			{
				m_legacyBehavior = Convert.ToBoolean( GetCurrentParam( ref nodeParams ) );
			}
		}

		public override void WriteToString( ref string nodeInfo , ref string connectionsInfo )
		{
			base.WriteToString( ref nodeInfo , ref connectionsInfo );
			IOUtils.AddFieldValueToString( ref nodeInfo , m_legacyBehavior );
		}
	}
}
