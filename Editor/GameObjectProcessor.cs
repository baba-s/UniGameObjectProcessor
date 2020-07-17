using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Kogane
{
	/// <summary>
	/// リザルトタイプ
	/// </summary>
	public enum GameObjectProcessResult
	{
		/// <summary>
		/// 変更していない（シーンやプレハブを保存しない）
		/// </summary>
		NOT_CHANGE,

		/// <summary>
		/// 変更した（シーンやプレハブを保存する）
		/// </summary>
		CHANGE,
	}

	/// <summary>
	/// シーンやプレハブに存在するすべてのゲームオブジェクトに対して処理を行うクラス
	/// </summary>
	public static class GameObjectProcessor
	{
		//================================================================================
		// 関数(static)
		//================================================================================
		/// <summary>
		/// すべてのシーンに存在するすべてのゲームオブジェクトに対して処理を行います
		/// </summary>
		/// <param name="onProcess">ゲームオブジェクトに対して処理を行うデリゲート</param>
		public static void ProcessAllScenes( Func<GameObject, GameObjectProcessResult> onProcess )
		{
			ProcessAllScenes( null, onProcess );
		}

		/// <summary>
		/// すべてのシーンに存在するすべてのゲームオブジェクトに対して処理を行います
		/// </summary>
		/// <param name="scenePathFilter">処理を行うシーンを絞り込むためのデリゲート</param>
		/// <param name="onProcess">ゲームオブジェクトに対して処理を行うデリゲート</param>
		public static void ProcessAllScenes
		(
			Func<string, bool>                        scenePathFilter,
			Func<GameObject, GameObjectProcessResult> onProcess
		)
		{
			var sceneSetups = EditorSceneManager.GetSceneManagerSetup();

			// FindAssets は Packages フォルダも対象になっているので
			// Assets フォルダ以下のシーンのみを抽出
			var scenePaths = AssetDatabase
					.FindAssets( "t:scene" )
					.Select( x => AssetDatabase.GUIDToAssetPath( x ) )
					.Where( x => x.StartsWith( "Assets/" ) )
					.Where( x => scenePathFilter == null || scenePathFilter( x ) )
					.ToArray()
				;

			try
			{
				for ( var i = 0; i < scenePaths.Length; i++ )
				{
					var scenePath = scenePaths[ i ];
					var scene     = EditorSceneManager.OpenScene( scenePath );

					var gameObjects = scene
							.GetRootGameObjects()
							.SelectMany( x => x.GetComponentsInChildren<Transform>( true ) )
							.Select( x => x.gameObject )
							.ToArray()
						;

					if ( gameObjects.Length <= 0 ) continue;

					var isSave = false;

					foreach ( var gameObject in gameObjects )
					{
						isSave |= onProcess( gameObject ) == GameObjectProcessResult.CHANGE;
					}

					if ( isSave )
					{
						EditorSceneManager.MarkSceneDirty( scene );
						EditorSceneManager.SaveScene( scene );
					}
				}
			}
			finally
			{
				// Untitled なシーンは復元できず、SceneSetup[] の要素数が 0 になる
				// Untitled なシーンを復元しようとすると下記のエラーが発生するので if で確認
				// ArgumentException: Invalid SceneManagerSetup:
				if ( 0 < sceneSetups.Length )
				{
					EditorSceneManager.RestoreSceneManagerSetup( sceneSetups );
				}
			}
		}

		/// <summary>
		/// すべてのプレハブに存在するすべてのゲームオブジェクトに対して処理を行います
		/// </summary>
		/// <param name="onProcess">ゲームオブジェクトに対して処理を行うデリゲート</param>
		public static void ProcessAllPrefabs( Func<GameObject, GameObjectProcessResult> onProcess )
		{
			ProcessAllPrefabs( null, onProcess );
		}

		/// <summary>
		/// すべてのプレハブに存在するすべてのゲームオブジェクトに対して処理を行います
		/// </summary>
		/// <param name="prefabPathFilter">処理を行うプレハブを絞り込むためのデリゲート</param>
		/// <param name="onProcess">ゲームオブジェクトに対して処理を行うデリゲート</param>
		public static void ProcessAllPrefabs
		(
			Func<string, bool>                        prefabPathFilter,
			Func<GameObject, GameObjectProcessResult> onProcess
		)
		{
			// FindAssets は Packages フォルダも対象になっているので
			// Assets フォルダ以下のシーンのみを抽出
			var prefabPaths = AssetDatabase
					.FindAssets( "t:prefab" )
					.Select( x => AssetDatabase.GUIDToAssetPath( x ) )
					.Where( x => x.StartsWith( "Assets/" ) )
					.Where( x => prefabPathFilter == null || prefabPathFilter( x ) )
					.ToArray()
				;

			try
			{
				for ( var i = 0; i < prefabPaths.Length; i++ )
				{
					var prefabPath = prefabPaths[ i ];
					var prefab     = PrefabUtility.LoadPrefabContents( prefabPath );

					var gameObjects = prefab
							.GetComponentsInChildren<Transform>( true )
							.Select( x => x.gameObject )
							.ToArray()
						;

					if ( gameObjects.Length <= 0 ) continue;

					var isSave = false;

					foreach ( var gameObject in gameObjects )
					{
						isSave |= onProcess( gameObject ) == GameObjectProcessResult.CHANGE;
					}

					if ( isSave )
					{
						PrefabUtility.SaveAsPrefabAsset( prefab, prefabPath );
					}

					PrefabUtility.UnloadPrefabContents( prefab );
				}
			}
			finally
			{
				// この処理を呼び出さないと Project ビューの表示が更新されず、
				// 変更も保存されない
				AssetDatabase.SaveAssets();
			}
		}
	}
}