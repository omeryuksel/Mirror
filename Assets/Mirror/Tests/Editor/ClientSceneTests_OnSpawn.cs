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


        [Test]
        public void FindOrSpawnObject_FindExistingObject()
        {
            const uint netId = 1000;
            GameObject go = new GameObject();
            NetworkIdentity existing = go.AddComponent<NetworkIdentity>();
            existing.netId = netId;
            NetworkIdentity.spawned.Add(netId, existing);

            SpawnMessage msg = new SpawnMessage
            {
                netId = netId
            };
            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity found);

            Assert.IsTrue(success);
            Assert.That(found, Is.EqualTo(existing));


            // cleanup
            GameObject.DestroyImmediate(found.gameObject);
        }

        [Test]
        public void FindOrSpawnObject_GivesErrorWhenNoExistingAndAssetIdAndSceneIdAreBothEmpty()
        {
            const uint netId = 1001;
            SpawnMessage msg = new SpawnMessage
            {
                assetId = new Guid(),
                sceneId = 0,
                netId = netId
            };

            LogAssert.Expect(LogType.Error, $"OnSpawn message with netId '{netId}' has no AssetId or sceneId");
            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity found);

        }

        [Test]
        public void FindOrSpawnObject_SpawnsFromPrefabDictionary()
        {
            const uint netId = 1002;
            SpawnMessage msg = new SpawnMessage
            {
                netId = netId,
                assetId = validPrefabGuid

            };

            prefabs.Add(validPrefabGuid, validPrefab);

            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);

            Assert.IsTrue(success);
            Assert.IsNotNull(networkIdentity);
            Assert.That(networkIdentity.name, Is.EqualTo(validPrefab.name + "(Clone)"));

            // cleanup
            GameObject.DestroyImmediate(networkIdentity.gameObject);
        }

        [Test]
        public void FindOrSpawnObject_ErrorWhenPrefabInNullInDictionary()
        {
            const uint netId = 1002;
            SpawnMessage msg = new SpawnMessage
            {
                netId = netId,
                assetId = validPrefabGuid
            };

            // could happen if the prefab is destroyed or unloaded
            prefabs.Add(validPrefabGuid, null);

            LogAssert.Expect(LogType.Error, $"Prefab in dictionary was null for assetId '{msg.assetId}'. If you delete or unload the prefab make sure to unregister it from ClientScene too.");
            LogAssert.Expect(LogType.Error, $"Failed to spawn server object, did you forget to add it to the NetworkManager? assetId={msg.assetId} netId={msg.netId}");
            LogAssert.Expect(LogType.Error, $"Could not spawn assetId={msg.assetId} scene={msg.sceneId} netId={msg.netId}");
            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);


            Assert.IsFalse(success);
            Assert.IsNull(networkIdentity);
        }

        [Test]
        public void FindOrSpawnObject_SpawnsFromPrefabIfBothPrefabAndHandlerExists()
        {
            const uint netId = 1003;
            int handlerCalled = 0;
            SpawnMessage msg = new SpawnMessage
            {
                netId = netId,
                assetId = validPrefabGuid
            };

            prefabs.Add(validPrefabGuid, validPrefab);
            spawnHandlers.Add(validPrefabGuid, (x) =>
            {
                handlerCalled++;
                GameObject go = new GameObject("testObj", typeof(NetworkIdentity));
                _createdObjects.Add(go);
                return go;
            });


            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);

            Assert.IsTrue(success);
            Assert.IsNotNull(networkIdentity);
            Assert.That(networkIdentity.name, Is.EqualTo(validPrefab.name + "(Clone)"));
            Assert.That(handlerCalled, Is.EqualTo(0), "Handler should not have been called");
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

            LogAssert.Expect(LogType.Error, $"Spawn Handler returned null, Handler assetId '{msg.assetId}'");
            LogAssert.Expect(LogType.Error, $"Could not spawn assetId={msg.assetId} scene={msg.sceneId} netId={msg.netId}");
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
            LogAssert.Expect(LogType.Error, $"Could not spawn assetId={msg.assetId} scene={msg.sceneId} netId={msg.netId}");
            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);

            Assert.IsFalse(success);
            Assert.IsNull(networkIdentity);
        }

        NetworkIdentity CreateSceneObject(ulong sceneId)
        {
            GameObject runtimeObject = new GameObject("Runtime GameObject");
            NetworkIdentity networkIdentity = runtimeObject.AddComponent<NetworkIdentity>();
            // set sceneId to zero as it is set in onvalidate (does not set id at runtime)
            networkIdentity.sceneId = sceneId;

            _createdObjects.Add(runtimeObject);
            spawnableObjects.Add(sceneId, networkIdentity);

            return networkIdentity;
        }

        [Test]
        public void FindOrSpawnObject_UsesSceneIdToSpawnFromSpawnableObjectsDictionary()
        {
            const uint netId = 1003;
            const int sceneId = 100020;
            SpawnMessage msg = new SpawnMessage
            {
                netId = netId,
                sceneId = sceneId
            };

            NetworkIdentity sceneObject = CreateSceneObject(sceneId);


            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);

            Assert.IsTrue(success);
            Assert.IsNotNull(networkIdentity);
            Assert.That(networkIdentity, Is.EqualTo(sceneObject));
        }

        [Test]
        public void FindOrSpawnObject_SpawnsUsingSceneIdInsteadOfAssetId()
        {
            const uint netId = 1003;
            const int sceneId = 100020;
            SpawnMessage msg = new SpawnMessage
            {
                netId = netId,
                sceneId = sceneId,
                assetId = validPrefabGuid
            };

            prefabs.Add(validPrefabGuid, validPrefab);
            NetworkIdentity sceneObject = CreateSceneObject(sceneId);

            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);

            Assert.IsTrue(success);
            Assert.IsNotNull(networkIdentity);
            Assert.That(networkIdentity, Is.EqualTo(sceneObject));
        }

        [Test]
        public void FindOrSpawnObject_GivesErrorWhenSceneIdIsNotInSpawnableObjectsDictionary()
        {
            const uint netId = 1004;
            const int sceneId = 100021;
            SpawnMessage msg = new SpawnMessage
            {
                netId = netId,
                sceneId = sceneId,
            };


            LogAssert.Expect(LogType.Error, $"Spawn scene object not found for {msg.sceneId.ToString("X")} SpawnableObjects.Count={spawnableObjects.Count}");
            LogAssert.Expect(LogType.Error, $"Could not spawn assetId={msg.assetId} scene={msg.sceneId} netId={msg.netId}");
            bool success = ClientScene.FindOrSpawnObject(msg, out NetworkIdentity networkIdentity);

            Assert.IsFalse(success);
            Assert.IsNull(networkIdentity);
        }


        [Test]
        public void ApplyPayload()
        {
            Assert.Ignore();
            // Applies Payload Correctly
            // Applies Payload to existing object (if one exists)
        }
    }
}
