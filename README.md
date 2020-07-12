# UniGameObjectProcessor

すべてのシーンやプレハブに存在するすべてのゲームオブジェクトに対して処理を行うクラス

## 使用例

```cs
using Kogane;
using UnityEditor;
using UnityEngine;

public static class Example
{
    /// <summary>
    /// すべてのシーンに存在するゲームオブジェクトに BoxCollider をアタッチします
    /// </summary>
    [MenuItem( "Tools/Add BoxCollider to Scene" )]
    private static void AddBoxColliderToScene()
    {
        // Assets/@Project フォルダに存在するシーンのみを対象にする
        bool PathFilter( string scenePath )
        {
            return scenePath.StartsWith( "Assets/@Project/" );
        }

        // ゲームオブジェクトを処理するコールバック
        GameObjectProcessResult OnProcess( GameObject gameObject )
        {
            // プレハブのインスタンスの場合は何もしない
            if ( PrefabUtility.GetPrefabAssetType( gameObject ) != PrefabAssetType.NotAPrefab )
            {
                // NOT_CHANGE を返すとシーンは保存されない
                return GameObjectProcessResult.NOT_CHANGE;
            }

            // BoxCollider がすでにアタッチされている場合は何もしない
            if ( gameObject.GetComponent<BoxCollider>() != null )
            {
                // NOT_CHANGE を返すとシーンは保存されない
                return GameObjectProcessResult.NOT_CHANGE;
            }

            // BoxCollider をアタッチする
            gameObject.AddComponent<BoxCollider>();

            // CHANGE を返すとシーンは保存される
            return GameObjectProcessResult.CHANGE;
        }

        // すべてのシーンに存在するすべてのゲームオブジェクトに対して処理を行う
        GameObjectProcessor.ProcessAllScenes
        (
            scenePathFilter: PathFilter,
            onProcess: OnProcess
        );
    }

    /// <summary>
    /// すべてのプレハブに存在するゲームオブジェクトに BoxCollider をアタッチします
    /// </summary>
    [MenuItem( "Tools/Add BoxCollider to Prefab" )]
    private static void AddBoxColliderToPrefab()
    {
        // Assets/@Project フォルダに存在するプレハブのみを対象にする
        bool PathFilter( string scenePath )
        {
            return scenePath.StartsWith( "Assets/@Project/" );
        }

        // ゲームオブジェクトを処理するコールバック
        GameObjectProcessResult OnProcess( GameObject gameObject )
        {
            // BoxCollider がすでにアタッチされている場合は何もしない
            if ( gameObject.GetComponent<BoxCollider>() != null )
            {
                // NOT_CHANGE を返すとプレハブは保存されない
                return GameObjectProcessResult.NOT_CHANGE;
            }

            // BoxCollider をアタッチする
            gameObject.AddComponent<BoxCollider>();

            // CHANGE を返すとプレハブは保存される
            return GameObjectProcessResult.CHANGE;
        }

        // すべてのプレハブに存在するすべてのゲームオブジェクトに対して処理を行う
        GameObjectProcessor.ProcessAllPrefabs
        (
            prefabPathFilter: PathFilter,
            onProcess: OnProcess
        );
    }
}
```
