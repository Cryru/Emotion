// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Emotion.Engine;
using Emotion.Graphics;
using Emotion.Graphics.Batching;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using Emotion.Sound;
using Emotion.Tests.Interoperability;
using Emotion.Tests.Scenes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Emotion.Tests.Tests
{
    /// <summary>
    /// Tests connected with sound and the SoundManager module.
    /// </summary>
    [TestClass]
    public class Sound
    {
        /// <summary>
        /// Test whether loading and playing of sound works.
        /// </summary>
        [TestMethod]
        public void LoadAndPlaySound()
        {
            // Create a holder for the sound file.
            SoundFile sound = null;

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    sound = Context.AssetLoader.Get<SoundFile>("Sounds/kids.wav");
                    Context.SoundManager.Play(sound, "testLayer");
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            long memoryUsage = Process.GetCurrentProcess().WorkingSet64;

            // Set instant sound thread.
            Context.Flags.SoundThreadFrequency = 1;

            // Assert that the sound file was loaded.
            Assert.AreEqual(25f, sound.Duration);
            // Check whether the layer was created.
            Assert.AreEqual(1, Context.SoundManager.Layers.Length);

            // Get the layer playing on.
            SoundLayer layer = Context.SoundManager.GetLayer("testLayer");

            // Layer name should be correct.
            Assert.AreEqual("testLayer", layer.Name);

            // Nothing should be playing.
            Assert.AreEqual(null, layer.CurrentlyPlayingFile);

            // Wait until it starts playing. Should be a couple of updates on the AL thread.
            Stopwatch watch = Stopwatch.StartNew();
            while (layer.Status == SoundStatus.Initial || layer.CurrentlyPlayingFile == null)
            {
                // Timeout.
                if(watch.ElapsedMilliseconds > 1000) break;
            }

            // The currently playing file should be the one which was played.
            Assert.AreEqual(sound, layer.CurrentlyPlayingFile);
            // It should also be at the top of the playlist.
            Assert.AreEqual(sound, layer.PlayList[0]);
            // Total layer duration should be the same as the one file playing on it.
            Assert.AreEqual(sound.Duration, layer.TotalDuration);

            // Playback location should be somewhere at the start.
            float oldPlayback = layer.PlaybackLocation;
            Assert.IsTrue(oldPlayback < 1);

            // Wait for 5 seconds.
            Task.Delay(5000).Wait();

            // The current duration should be around 5 seconds in.
            Assert.IsTrue(layer.PlaybackLocation - (oldPlayback + 5) < 1);

            // Stop playing.
            layer.StopPlayingAll();

            // Cleanup sound.
            Context.AssetLoader.Destroy("Sounds/kids.wav");

            // Wait half a second for cleanup.
            Task.Delay(500).Wait();

            // Should be cleaned up.
            Assert.AreEqual(-1, sound.ALBuffer);

            // Memory usage should have fallen.
            Assert.IsTrue(memoryUsage > Process.GetCurrentProcess().WorkingSet64);

            // Remove the layer.
            Context.SoundManager.RemoveLayer("testLayer");
            Assert.AreEqual(0, Context.SoundManager.Layers.Length);

            // Cleanup.
            Helpers.UnloadScene();

            // Restore sound thread frequency.
            Context.Flags.SoundThreadFrequency = 200;
        }

        /// <summary>
        /// Test whether looping of a sound works.
        /// </summary>
        [TestMethod]
        public void LoopSound()
        {
            // Create a holder for the sound file.
            SoundFile sound = null;

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    sound = Context.AssetLoader.Get<SoundFile>("Sounds/noice.wav");
                    Context.SoundManager.Play(sound, "testLayer");
                    Context.SoundManager.GetLayer("testLayer").Looping = true;
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Set instant sound thread.
            Context.Flags.SoundThreadFrequency = 1;

            // Assert that the sound file was loaded.
            Assert.IsTrue(sound.Duration - 3f < 1f);

            // Get the layer playing on.
            SoundLayer layer = Context.SoundManager.GetLayer("testLayer");

            // Nothing should be playing.
            Assert.AreEqual(null, layer.CurrentlyPlayingFile);

            // Wait until it starts playing. Should be a couple of updates on the AL thread.
            Stopwatch watch = Stopwatch.StartNew();
            while (layer.Status == SoundStatus.Initial || layer.CurrentlyPlayingFile == null)
            {
                // Timeout.
                if(watch.ElapsedMilliseconds > 1000) break;
            }

            // The currently playing file should be the one which was played.
            Assert.AreEqual(sound, layer.CurrentlyPlayingFile);
            // It should also be at the top of the playlist.
            Assert.AreEqual(sound, layer.PlayList[0]);
            // Total layer duration should be the same as the one file playing on it.
            Assert.AreEqual(sound.Duration, layer.TotalDuration);

            // Playback location should be somewhere at the start.
            float oldPlayback = layer.PlaybackLocation;
            Assert.IsTrue(oldPlayback < 1);

            // Wait for 5 seconds.
            Task.Delay(5000).Wait();

            // Should still be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);
            // Duration should be about at the end of the second loop.
            Assert.IsTrue(layer.PlaybackLocation - 3f < 1f);

            // Stop playing.
            layer.StopPlayingAll();

            // Cleanup sound.
            Context.AssetLoader.Destroy(sound.Name);

            // Wait half a second for cleanup.
            Task.Delay(500).Wait();

            // Should be cleaned up.
            Assert.AreEqual(-1, sound.ALBuffer);

            // Remove the layer.
            Context.SoundManager.RemoveLayer("testLayer");
            Assert.AreEqual(0, Context.SoundManager.Layers.Length);

            // Cleanup.
            Helpers.UnloadScene();

            // Restore sound thread frequency.
            Context.Flags.SoundThreadFrequency = 200;
        }

        /// <summary>
        /// Test whether queuing of sounds works.
        /// </summary>
        [TestMethod]
        public void SoundQueue()
        {
            List<SoundFile> soundFiles = new List<SoundFile>();
            List<Task> asyncTasks = new List<Task>();

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    soundFiles.Add(Context.AssetLoader.Get<SoundFile>("Sounds/noice.wav"));
                    soundFiles.Add(Context.AssetLoader.Get<SoundFile>("Sounds/kids.wav"));

                    asyncTasks.Add(Context.SoundManager.Play(soundFiles[0], "testLayer"));
                    asyncTasks.Add(Context.SoundManager.PlayQueue(soundFiles[1], "testLayer"));
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Set instant sound thread.
            Context.Flags.SoundThreadFrequency = 1;

            // Get the layer playing on.
            SoundLayer layer = Context.SoundManager.GetLayer("testLayer");

            // Wait until it starts playing. Should be a couple of updates on the AL thread.
            Stopwatch watch = Stopwatch.StartNew();
            while (layer.Status == SoundStatus.Initial || layer.CurrentlyPlayingFile == null)
            {
                // Timeout.
                if(watch.ElapsedMilliseconds > 1000) break;
            }

            // Wait for everything to queue.
            Task.WaitAll(asyncTasks.ToArray());
            asyncTasks.Clear();

            // The currently playing file should be the first.
            Assert.AreEqual(soundFiles[0], layer.CurrentlyPlayingFile);
            // There should be two files in the playlist.
            Assert.AreEqual(2, layer.PlayList.Count);
            // It should also be at the top of the playlist.
            Assert.AreEqual(soundFiles[0], layer.PlayList[0]);
            // The second one in the playlist should be the second file.
            Assert.AreEqual(soundFiles[1], layer.PlayList[1]);

            // Total layer duration should be the sum of both files.
            Assert.AreEqual(soundFiles[0].Duration + soundFiles[1].Duration, layer.TotalDuration);

            // Playback location should be somewhere at the start.
            float oldPlayback = layer.PlaybackLocation;
            Assert.IsTrue(oldPlayback < 1);

            // Wait for 1 second.
            Task.Delay(1000).Wait();

            // Should still be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);
            // Check playback.
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            // Should still be on the first file.
            Assert.AreEqual(soundFiles[0], layer.CurrentlyPlayingFile);

            // Wait for 3 more seconds.
            Task.Delay(3000).Wait();

            // Should still be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);
            // Playback should be in the beginning of the second file.
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            // Should now be on the second file.
            Assert.AreEqual(soundFiles[1], layer.CurrentlyPlayingFile);
            // Playlist should be one file.
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(soundFiles[1], layer.PlayList[0]);
            // Duration should be only the second file.
            Assert.AreEqual(soundFiles[1].Duration, layer.TotalDuration);

            // Stop playing.
            layer.StopPlayingAll();

            // Cleanup sound.
            foreach (SoundFile sf in soundFiles)
            {
                Context.AssetLoader.Destroy(sf.Name);
            }

            // Wait half a second for cleanup.
            Task.Delay(500).Wait();

            // Should be cleaned up.
            foreach (SoundFile sf in soundFiles)
            {
                Assert.AreEqual(-1, sf.ALBuffer);
            }

            soundFiles.Clear();

            // Remove the layer.
            Context.SoundManager.RemoveLayer("testLayer");
            Assert.AreEqual(0, Context.SoundManager.Layers.Length);

            // Cleanup.
            Helpers.UnloadScene();

            // Restore sound thread frequency.
            Context.Flags.SoundThreadFrequency = 200;
        }

        /// <summary>
        /// Test whether queuing of files with a different channel count produces the expected negative result, as it is an unsupported operation.
        /// </summary>
        [TestMethod]
        public void SoundQueueChannelMix()
        {
            List<SoundFile> soundFiles = new List<SoundFile>();

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    soundFiles.Add(Context.AssetLoader.Get<SoundFile>("Sounds/noice.wav"));
                    soundFiles.Add(Context.AssetLoader.Get<SoundFile>("Sounds/noiceStereo.wav"));

                    Context.SoundManager.Play(soundFiles[0], "testLayer");
                    Context.SoundManager.PlayQueue(soundFiles[1], "testLayer");
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Set instant sound thread.
            Context.Flags.SoundThreadFrequency = 1;

            // Get the layer playing on.
            SoundLayer layer = Context.SoundManager.GetLayer("testLayer");

            // Wait until it starts playing. Should be a couple of updates on the AL thread.
            Stopwatch watch = Stopwatch.StartNew();
            while (layer.Status == SoundStatus.Initial || layer.CurrentlyPlayingFile == null)
            {
                // Timeout.
                if(watch.ElapsedMilliseconds > 1000) break;
            }

            // Only the first queued track should play. As the other one could not be queued.
            Assert.AreEqual(soundFiles[0], layer.CurrentlyPlayingFile);
            // Only one track in the playlist.
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(soundFiles[0], layer.PlayList[0]);

            // Duration shouldn't include the second track.
            Assert.AreEqual(soundFiles[0].Duration, layer.TotalDuration);

            // Stop playing.
            layer.StopPlayingAll();

            // Cleanup sound.
            foreach (SoundFile sf in soundFiles)
            {
                Context.AssetLoader.Destroy(sf.Name);
            }

            // Wait half a second for cleanup.
            Task.Delay(500).Wait();

            // Should be cleaned up.
            foreach (SoundFile sf in soundFiles)
            {
                Assert.AreEqual(-1, sf.ALBuffer);
            }

            soundFiles.Clear();

            // Remove the layer.
            Context.SoundManager.RemoveLayer("testLayer");
            Assert.AreEqual(0, Context.SoundManager.Layers.Length);

            // Cleanup.
            Helpers.UnloadScene();

            // Restore sound thread frequency.
            Context.Flags.SoundThreadFrequency = 200;
        }

        /// <summary>
        /// Test whether queuing of sounds works after it has stopped playing once.
        /// </summary>
        [TestMethod]
        public void SoundQueueAfterFinish()
        {
            SoundFile file = null;

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    file = Context.AssetLoader.Get<SoundFile>("Sounds/noice.wav");

                    Context.SoundManager.Play(file, "testLayer");
                    Context.SoundManager.PlayQueue(file, "testLayer");
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Set instant sound thread.
            Context.Flags.SoundThreadFrequency = 1;

            // Get the layer playing on.
            SoundLayer layer = Context.SoundManager.GetLayer("testLayer");

            // Wait until it starts playing. Should be a couple of updates on the AL thread.
            Stopwatch watch = Stopwatch.StartNew();
            while (layer.Status == SoundStatus.Initial || layer.CurrentlyPlayingFile == null)
            {
                // Timeout.
                if(watch.ElapsedMilliseconds > 1000) break;
            }

            // Wait for it to finish.
            Task.Delay(6500).Wait();

            // Should no longer be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Stopped);
            // Current file should be none.
            Assert.AreEqual(null, layer.CurrentlyPlayingFile);
            // Playlist should be empty.
            Assert.AreEqual(0, layer.PlayList.Count);

            SoundFile newFile = Context.AssetLoader.Get<SoundFile>("Sounds/kids.wav");

            // Queue a new sound.
            Context.SoundManager.PlayQueue(newFile, "testLayer");

            // Wait until it starts playing.
            watch.Restart();
            while (layer.Status == SoundStatus.Initial || layer.CurrentlyPlayingFile == null)
            {
                // Timeout.
                if(watch.ElapsedMilliseconds > 1000) break;
            }

            // Stop playing.
            layer.StopPlayingAll();

            // Play again.
            Context.SoundManager.Play(newFile, "testLayer");

            // Wait until it starts playing.
            watch.Restart();
            while (layer.Status == SoundStatus.Initial || layer.CurrentlyPlayingFile == null)
            {
                // Timeout.
                if(watch.ElapsedMilliseconds > 1000) break;
            }

            // Stop playing.
            layer.StopPlayingAll();

            // Cleanup sound.
            Context.AssetLoader.Destroy(file.Name);
            Context.AssetLoader.Destroy(newFile.Name);

            // Wait half a second for cleanup.
            Task.Delay(500).Wait();

            // Should be cleaned up.
            Assert.AreEqual(-1, file.ALBuffer);

            // Remove the layer.
            Context.SoundManager.RemoveLayer("testLayer");
            Assert.AreEqual(0, Context.SoundManager.Layers.Length);

            // Cleanup.
            Helpers.UnloadScene();

            // Restore sound thread frequency.
            Context.Flags.SoundThreadFrequency = 200;
        }

        /// <summary>
        /// Test whether looping of a queue works.
        /// </summary>
        [TestMethod]
        public void LoopQueue()
        {
            // Create a holder for the sound files.
            List<SoundFile> soundFiles = new List<SoundFile>();
            List<Task> asyncTasks = new List<Task>();

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    soundFiles.Add(Context.AssetLoader.Get<SoundFile>("Sounds/noice.wav"));
                    soundFiles.Add(Context.AssetLoader.Get<SoundFile>("Sounds/sadMeme.wav"));
                    asyncTasks.Add(Context.SoundManager.PlayQueue(soundFiles[0], "testLayer"));
                    asyncTasks.Add(Context.SoundManager.PlayQueue(soundFiles[1], "testLayer"));
                    Context.SoundManager.GetLayer("testLayer").Looping = true;
                    Context.SoundManager.GetLayer("testLayer").LoopLastOnly = false;
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Set instant sound thread.
            Context.Flags.SoundThreadFrequency = 1;

            // Get the layer playing on.
            SoundLayer layer = Context.SoundManager.GetLayer("testLayer");

            // Wait until it starts playing. Should be a couple of updates on the AL thread.
            Stopwatch watch = Stopwatch.StartNew();
            while (layer.Status == SoundStatus.Initial || layer.CurrentlyPlayingFile == null)
            {
                // Timeout.
                if(watch.ElapsedMilliseconds > 1000) break;
            }
            
            // Wait for everything to queue.
            Task.WaitAll(asyncTasks.ToArray());
            asyncTasks.Clear();

            // Assert that the duration is correct.
            Assert.AreEqual(soundFiles[0].Duration + soundFiles[1].Duration, layer.TotalDuration);

            // The currently playing file should be the first.
            Assert.AreEqual(soundFiles[0], layer.CurrentlyPlayingFile);
            // It should also be at the top of the playlist.
            Assert.AreEqual(soundFiles[0], layer.PlayList[0]);

            // Play for 4 seconds, which should bring it to the second track.
            Task.Delay(4000).Wait();

            // Second file should be playing.
            Assert.AreEqual(soundFiles[1], layer.CurrentlyPlayingFile);
            // But the top of the playlist should still be the first file.
            Assert.AreEqual(soundFiles[0], layer.PlayList[0]);
            // And the duration shouldn't have changed.
            Assert.AreEqual(soundFiles[0].Duration + soundFiles[1].Duration, layer.TotalDuration);

            // Play for 4 more seconds and a half. This should be within the last track.
            Task.Delay(4500).Wait();

            // First file should be playing now as it has looped.
            Assert.AreEqual(soundFiles[0], layer.CurrentlyPlayingFile);
            // But the top of the playlist should still be the first file.
            Assert.AreEqual(soundFiles[0], layer.PlayList[0]);
            // And the duration shouldn't have changed.
            Assert.AreEqual(soundFiles[0].Duration + soundFiles[1].Duration, layer.TotalDuration);

            // Should still be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Stop playing.
            layer.StopPlayingAll();

            // Cleanup sound.
            foreach (SoundFile sf in soundFiles)
            {
                Context.AssetLoader.Destroy(sf.Name);
            }

            // Wait half a second for cleanup.
            Task.Delay(500).Wait();

            // Should be cleaned up.
            foreach (SoundFile sf in soundFiles)
            {
                Assert.AreEqual(-1, sf.ALBuffer);
            }

            soundFiles.Clear();

            // Remove the layer.
            Context.SoundManager.RemoveLayer("testLayer");
            Assert.AreEqual(0, Context.SoundManager.Layers.Length);

            // Cleanup.
            Helpers.UnloadScene();

            // Restore sound thread frequency.
            Context.Flags.SoundThreadFrequency = 200;
        }
    }
}