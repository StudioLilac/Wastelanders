using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Systems.Persistence.Tests
{
    public class SaveLoadSystemIntegrationTests
    {
        private string testDirectory;
        private SaveLoadSystem saveLoadSystem;
        private GameObject systemGameObject;

        [SetUp]
        public void Setup()
        {
            testDirectory = Path.Combine(Application.temporaryCachePath, "IntegrationTests");
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }
            Directory.CreateDirectory(testDirectory);
            systemGameObject = new GameObject("SaveLoadSystem_Test");
            saveLoadSystem = systemGameObject.AddComponent<SaveLoadSystem>();
        }

        [TearDown]
        public void Teardown()
        {
            if (Directory.Exists(testDirectory))
            {
                Directory.Delete(testDirectory, true);
            }

            if (systemGameObject != null)
            {
                Object.DestroyImmediate(systemGameObject);
            }
        }

        [UnityTest]
        public IEnumerator FirstTimePlayer_GeneratesCleanSaves_AndReportsOkStatus()
        {
            var testSerializer = new JSonSerializer();
            var sandboxService = new FileDataService(testSerializer, testDirectory);

            saveLoadSystem.InitializeForTesting(sandboxService);

            yield return null;

            var status = new GetSaveSystemStatus().Query();
            Assert.IsTrue(status is SaveStatus.Ok, "Expected SaveSystemStatus to be Ok.");

            string expectedGameDataPath = Path.Combine(testDirectory, "Wastelanders Save File.json");
            string expectedPrefsPath = Path.Combine(testDirectory, "Wastelanders User Preferences File.json");

            Assert.IsTrue(File.Exists(expectedGameDataPath), "GameData file was not generated.");
            Assert.IsTrue(File.Exists(expectedPrefsPath), "Preferences file was not generated.");

            string rawGameData = File.ReadAllText(expectedGameDataPath);
            Assert.IsTrue(rawGameData.Contains($"\"SaveVersion\": {Versioning.CURRENT_GAMEDATA_VERSION}"),
                "GameData did not contain the correct starting SaveVersion.");

            string rawPrefsData = File.ReadAllText(expectedPrefsPath);
            Assert.IsTrue(rawPrefsData.Contains($"\"SaveVersion\": {Versioning.CURRENT_PREFERENCES_VERSION}"),
                "Preferences did not contain the correct starting SaveVersion.");
        }
    }
}