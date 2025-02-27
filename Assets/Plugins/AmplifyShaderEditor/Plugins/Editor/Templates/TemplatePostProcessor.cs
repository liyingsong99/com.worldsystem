


using UnityEditor;
using UnityEngine;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;

namespace AmplifyShaderEditor
{
	public sealed class TemplatePostProcessor : AssetPostprocessor
	{
		public static TemplatesManager DummyManager;
		public static void Destroy()
		{
			if( DummyManager != null )
			{
				DummyManager.Destroy();
				ScriptableObject.DestroyImmediate( DummyManager );
				DummyManager = null;
			}
		}

		static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
		{
			ASEPackageManagerHelper.RequestInfo();
			ASEPackageManagerHelper.Update();

			bool containsShaders = false;
			for( int i = 0; i < importedAssets.Length; i++ )
			{
				if( importedAssets[ i ].EndsWith( ".shader" ) )
				{
					containsShaders = true;
					break;
				}
			}

			
			if( !containsShaders )
				return;

			TemplatesManager templatesManager;
			bool firstTimeDummyFlag = false;
			if( UIUtils.CurrentWindow == null )
			{
				if( DummyManager == null )
				{
					DummyManager = ScriptableObject.CreateInstance<TemplatesManager>();
					DummyManager.hideFlags = HideFlags.HideAndDontSave;
					firstTimeDummyFlag = true;
				}
				templatesManager = DummyManager;
			}
			else
			{
				Destroy();
				templatesManager = UIUtils.CurrentWindow.TemplatesManagerInstance;
			}

			if( templatesManager == null )
			{
				return;
			}

			if( !templatesManager.Initialized )
			{
				templatesManager.Init();
			}

			bool refreshMenuItems = false;
			for( int i = 0; i < importedAssets.Length; i++ )
			{
				if( TemplateHelperFunctions.CheckIfTemplate( importedAssets[ i ] ) )
				{
					string guid = AssetDatabase.AssetPathToGUID( importedAssets[ i ] );
					TemplateDataParent templateData = templatesManager.GetTemplate( guid );
					if( templateData != null )
					{
						refreshMenuItems = templateData.Reload() || refreshMenuItems || firstTimeDummyFlag;
						int windowCount = IOUtils.AllOpenedWindows.Count;
						AmplifyShaderEditorWindow currWindow = UIUtils.CurrentWindow;
						for( int windowIdx = 0; windowIdx < windowCount; windowIdx++ )
						{
							if( IOUtils.AllOpenedWindows[ windowIdx ].OutsideGraph.CurrentCanvasMode == NodeAvailability.TemplateShader )
							{
								if( IOUtils.AllOpenedWindows[ windowIdx ].OutsideGraph.MultiPassMasterNodes.NodesList[ 0 ].CurrentTemplate == templateData )
								{
									UIUtils.CurrentWindow = IOUtils.AllOpenedWindows[ windowIdx ];
									IOUtils.AllOpenedWindows[ windowIdx ].OutsideGraph.ForceMultiPassMasterNodesRefresh();
								}
							}
						}
						UIUtils.CurrentWindow = currWindow;
					}
					else
					{
						refreshMenuItems = true;
						string name = TemplatesManager.OfficialTemplates.ContainsKey( guid ) ? TemplatesManager.OfficialTemplates[ guid ] : string.Empty;
						TemplateMultiPass mp = TemplateMultiPass.CreateInstance<TemplateMultiPass>();
						mp.Init( name, guid, AssetDatabase.GUIDToAssetPath( guid ), true );
						templatesManager.AddTemplate( mp );
					}
				}
			}

			if( deletedAssets.Length > 0 )
			{
				if( deletedAssets[ 0 ].IndexOf( Constants.InvalidPostProcessDatapath ) < 0 )
				{
					for( int i = 0; i < deletedAssets.Length; i++ )
					{
						string guid = AssetDatabase.AssetPathToGUID( deletedAssets[ i ] );
						TemplateDataParent templateData = templatesManager.GetTemplate( guid );
						if( templateData != null )
						{
							
							int windowCount = IOUtils.AllOpenedWindows.Count;
							for( int windowIdx = 0; windowIdx < windowCount; windowIdx++ )
							{
								TemplateMasterNode masterNode = IOUtils.AllOpenedWindows[ windowIdx ].CurrentGraph.CurrentMasterNode as TemplateMasterNode;
								if( masterNode != null && masterNode.CurrentTemplate.GUID.Equals( templateData.GUID ) )
								{
									IOUtils.AllOpenedWindows[ windowIdx ].Close();
								}
							}

							templatesManager.RemoveTemplate( templateData );
							refreshMenuItems = true;
						}
					}
				}
			}

			
			
			
			
			
			
			
			

			
			
			
			
			
			
			
			

			if( refreshMenuItems )
			{
				
				refreshMenuItems = false;
				templatesManager.CreateTemplateMenuItems();

				AmplifyShaderEditorWindow currWindow = UIUtils.CurrentWindow;

				int windowCount = IOUtils.AllOpenedWindows.Count;
				for( int windowIdx = 0; windowIdx < windowCount; windowIdx++ )
				{
					UIUtils.CurrentWindow = IOUtils.AllOpenedWindows[ windowIdx ];
					IOUtils.AllOpenedWindows[ windowIdx ].CurrentGraph.ForceCategoryRefresh();
				}
				UIUtils.CurrentWindow = currWindow;
			}

			
			templatesManager.ReimportMenuItems();

			
			Destroy();
		}
	}
}
