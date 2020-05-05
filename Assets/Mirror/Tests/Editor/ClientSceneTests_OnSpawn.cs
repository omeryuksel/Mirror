using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Mirror.Tests.ClientSceneTests
{
    public class ClientSceneTests_OnSpawn : ClientSceneTestsBase
    {
        [TearDown]
        public override void TearDown()
        {
            NetworkIdentity.spawned.Clear();
            base.TearDown();
        }

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


        [Test]
        public void FindOrSpawnObject_FindExistingObject()
        {
            const uint netId = 1000;
            GameObject go = new GameObject();
            NetworkIdentity existing = go.AddComponent<NetworkIdentity>();
            existing.netId = netId;
            NetworkIdentity.spawned.Add(netId, existing);

            bool success = ClientScene.FindOrSpawnObject(new SpawnMessage
            {
                netId = netId
            }, out NetworkIdentity found);

            Assert.IsTrue(success);
            Assert.That(found, Is.EqualTo(existing));


            // cleanup
            GameObject.DestroyImmediate(found.gameObject);
        }

        [Test]
        public void FindOrSpawnObject_GivesErrorWhenNoExistingAndAssetIdAndSceneIdAreBothEmpty()
        {
            const uint netId = 1001;
            LogAssert.Expect(LogType.Error, $"OnSpawn message with netId '{netId}' has no AssetId or sceneId");
            ClientScene.OnSpawn(new SpawnMessage
            {
                assetId = new Guid(),
                sceneId = 0,
                netId = netId
            });
        }

        [Test]
        public void FindOrSpawnObject_SpawnsFromPrefabDictionary()
        {
            const uint netId = 1002;

            prefabs.Add(validPrefabGuid, validPrefab);

            bool success = ClientScene.FindOrSpawnObject(new SpawnMessage
            {
                netId = netId,
                assetId = validPrefabGuid

            }, out NetworkIdentity found);

            Assert.IsTrue(success);
            Assert.That(found.name, Is.EqualTo(validPrefab.name + "(Clone)"));

            // cleanup
            GameObject.DestroyImmediate(found.gameObject);
        }

        [Test]
        public void FindOrSpawnObject_SpawnsByCallingHandlerFromDictionary()
        {
            Assert.Ignore();
        }

        [Test]
        public void FindOrSpawnObject_SpawnsFromSpawnableObjectsDictionary()
        {
            Assert.Ignore();
        }

        [Test]
        public void FindOrSpawnObject_SpawnsUsingSceneIdInsteadOfAssetId()
        {
            Assert.Ignore();
        }
    }
}
