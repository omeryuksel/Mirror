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
        public void FindOrSpawnObject_SpawnHandlerCalledFromDictionary()
        {
            const uint netId = 1003;
            int handlerCalled = 0;
            SpawnMessage msg = new SpawnMessage
            {
                netId = netId,
                assetId = validPrefabGuid
            };

            GameObject createdInhandler = null;

            spawnHandlers.Add(validPrefabGuid, (x) =>
            {
                handlerCalled++;
                Assert.That(x, Is.EqualTo(msg));
                createdInhandler = new GameObject("testObj", typeof(NetworkIdentity));
                _createdObjects.Add(createdInhandler);
                return createdInhandler;
            });


            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);

            Assert.IsTrue(success);
            Assert.IsNotNull(networkIdentity);
            Assert.That(handlerCalled, Is.EqualTo(1));
            Assert.That(networkIdentity.gameObject, Is.EqualTo(createdInhandler), "Object returned should be the same object created by the spawn handler");
        }
        [Test]
        public void FindOrSpawnObject_ErrorWhenSpawnHanlderReturnsNull()
        {
            const uint netId = 1003;
            SpawnMessage msg = new SpawnMessage
            {
                netId = netId,
                assetId = validPrefabGuid
            };

            spawnHandlers.Add(validPrefabGuid, (x) =>
            {
                return null;
            });


            LogAssert.Expect(LogType.Error, $"Spawn Handler returned null, Handler assetId '{validPrefabGuid}'");
            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);

            Assert.IsFalse(success);
            Assert.IsNull(networkIdentity);
        }
        [Test]
        public void FindOrSpawnObject_ErrorWhenSpawnHanlderReturnsWithoutNetworkIdentity()
        {
            const uint netId = 1003;
            SpawnMessage msg = new SpawnMessage
            {
                netId = netId,
                assetId = validPrefabGuid
            };

            spawnHandlers.Add(validPrefabGuid, (x) =>
            {
                GameObject go = new GameObject("testObj");
                _createdObjects.Add(go);
                return go;
            });

            LogAssert.Expect(LogType.Error, $"Object Spawned by handler did not have a NetworkIdentity, Handler assetId '{validPrefabGuid}'");
            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);

            Assert.IsFalse(success);
            Assert.IsNull(networkIdentity);
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
