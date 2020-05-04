using NUnit.Framework;

namespace Mirror.Tests
{
    public class ClientSceneTests_OnSpawn : ClientSceneTestsBase
    {
        // Applies Payload Correctly
        // Applies Payload to existing object (if one exists
        // Spawns Prefab from prefab Dictionary
        // Spawns Prefab from Handler
        // Spawns Scene object from spawnableObjects Dictionary
        // gives errors when...
        //   guid and sceneId is empty
        //   cant find prefab/handler with assetId
        //   cant find object with sceneId
        //   failed to spawn prefab
        [Test] [Ignore("Not Implemented")] public void DoesLotsOfStuffCorrectly() { }
    }
}
