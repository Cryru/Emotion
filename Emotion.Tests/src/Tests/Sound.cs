// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Engine;
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
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/kids.wav");
                     
            // Record memory usage for checking after cleanup.
            long memoryUsage = Process.GetCurrentProcess().WorkingSet64;

            // File should've loaded correctly.
            Assert.AreEqual(25f, playingFiles[0].Duration);

            // Playback location should be somewhere at the start.
            float oldPlayback = layer.PlaybackLocation;
            Assert.IsTrue(oldPlayback < 1);

            // Wait for 5 seconds.
            Task.Delay(5000).Wait();

            // The current duration should be around 5 seconds in.
            Assert.IsTrue(layer.PlaybackLocation - (oldPlayback + 5) < 1);

            SoundTestCleanup(layer, playingFiles);

            // Memory usage should've fallen after cleanup.
            Assert.IsTrue(memoryUsage > Process.GetCurrentProcess().WorkingSet64);
        }

        /// <summary>
        /// Test whether looping of a sound works. Also tests loading of stereo files as all other tests use mono tracks.
        /// </summary>
        [TestMethod]
        public void LoopSound()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noiceStereo.wav");
            layer.Looping = true;

            // Assert that the sound file was loaded. The duration of the track is around 3 seconds.
            Assert.IsTrue(playingFiles[0].Duration - 3f < 1f);

            // Playback location should be somewhere at the start.
            Assert.IsTrue(layer.PlaybackLocation < 1f);

            // Wait for the duration of the track. This should cause it to loop.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();

            // Should still be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Duration should be about at the end of the second loop.
            Assert.IsTrue(layer.PlaybackLocation < 1f);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether queuing of sounds works.
        /// </summary>
        [TestMethod]
        public void SoundQueue()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav", "Sounds/kids.wav");

            // Playback location should be somewhere at the start.
            Assert.IsTrue(layer.PlaybackLocation < 1f);

            // Wait for 1 second.
            Task.Delay(1000).Wait();

            // Should still be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);
            // Check playback, should be around a second in.
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            // Should still be on the first file.
            Assert.AreEqual(playingFiles[0], layer.CurrentlyPlayingFile);

            // Wait for 2 more seconds for the first track to end.
            Task.Delay(2000).Wait();

            // Should still be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Playback should be in the beginning of the second file.
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.AreEqual(playingFiles[1], layer.CurrentlyPlayingFile);
            // Playlist should be one file.
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(playingFiles[1], layer.PlayList[0]);
            // Duration should be only the second file.
            Assert.AreEqual(playingFiles[1].Duration, layer.TotalDuration);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether queuing of files with a different channel count produces the expected negative result, as it is an
        /// unsupported operation.
        /// </summary>
        [TestMethod]
        public void SoundQueueChannelMix()
        {
            SoundTestStartNoChecks(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav", "Sounds/noiceStereo.wav");

            // Only the first queued track should play. As the other one could not be queued.
            Assert.AreEqual(playingFiles[0], layer.CurrentlyPlayingFile);
            // Only one track in the playlist.
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(playingFiles[0], layer.PlayList[0]);

            // Duration shouldn't include the second track.
            Assert.AreEqual(playingFiles[0].Duration, layer.TotalDuration);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether queuing of sounds works after it has stopped playing once.
        /// </summary>
        [TestMethod]
        public void SoundQueueAfterFinish()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");

            // Wait for it to finish.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // Should no longer be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Stopped);
            // Current file should be none.
            Assert.AreEqual(null, layer.CurrentlyPlayingFile);
            // Playlist should be empty.
            Assert.AreEqual(0, layer.PlayList.Count);

            // Add new file.
            SoundFile newFile = Context.AssetLoader.Get<SoundFile>("Sounds/kids.wav");

            // Queue a new sound.
            Context.SoundManager.QueuePlay(newFile, "testLayer");

            // Loop sound manager once.
            WaitForSoundLoops(1);
            Assert.AreEqual(SoundStatus.Playing, layer.Status);
            Assert.IsTrue(layer.PlaybackLocation < 1f);

            // Stop playing.
            layer.StopPlayingAll();

            // Play again.
            Context.SoundManager.Play(newFile, "testLayer");

            // Wait until it starts playing.
            WaitForSoundLoops(1);
            Assert.AreEqual(SoundStatus.Playing, layer.Status);
            Assert.IsTrue(layer.PlaybackLocation < 1f);

            SoundTestCleanup(layer, playingFiles);
            Context.AssetLoader.Destroy(newFile.Name);
        }

        /// <summary>
        /// Test whether looping of single track works. Additionally adding tracks to a loop.
        /// </summary>
        [TestMethod]
        public void LoopSingleAndAddingToLoop()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");
            layer.Looping = true;
            layer.LoopLastOnly = false;

            // Wait for track to be over, so it loops.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // First file should be playing now as it has looped.
            Assert.AreEqual(playingFiles[0], layer.CurrentlyPlayingFile);
            // And it should still be in the playlist, and as the only thing there.
            Assert.AreEqual(playingFiles[0], layer.PlayList[0]);
            Assert.AreEqual(1, layer.PlayList.Count);
            // And the duration shouldn't have changed.
            Assert.AreEqual(playingFiles[0].Duration, layer.TotalDuration);
            // And playback should be correct.
            Assert.IsTrue(layer.PlaybackLocation < 1f);

            // Play a new track on the looping layer.
            SoundFile newTrack = Context.AssetLoader.Get<SoundFile>("Sounds/money.wav");
            // Wait for it to be queued.
            layer.Play(newTrack).Wait();
            WaitForSoundLoops(1);

            // Check whether all the reporting is correct.
            Assert.AreEqual(newTrack, layer.CurrentlyPlayingFile);
            Assert.AreEqual(newTrack, layer.PlayList[0]);
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(newTrack.Duration, layer.TotalDuration);
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            // Check monitoring.
            Assert.AreEqual(newTrack, layer.CurrentlyPlayingFile);
            Assert.AreEqual(newTrack, layer.PlayList[0]);
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(newTrack.Duration, layer.TotalDuration);
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            SoundFile queueTrack = Context.AssetLoader.Get<SoundFile>("Sounds/sadMeme.wav");

            // Queue new track, and wait for a loop.
            layer.QueuePlay(queueTrack).Wait();
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            // Should be playing second track.
            Assert.AreEqual(queueTrack, layer.CurrentlyPlayingFile);
            Assert.AreEqual(queueTrack, layer.PlayList[1]);
            Assert.AreEqual(2, layer.PlayList.Count);
            Assert.AreEqual(newTrack.Duration + queueTrack.Duration, layer.TotalDuration);
            Assert.IsTrue(layer.PlaybackLocation - newTrack.Duration < 1f);
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(queueTrack.Duration)).Wait();

            Assert.AreEqual(newTrack, layer.CurrentlyPlayingFile);
            Assert.AreEqual(newTrack, layer.PlayList[0]);
            Assert.AreEqual(2, layer.PlayList.Count);
            Assert.AreEqual(newTrack.Duration + queueTrack.Duration, layer.TotalDuration);
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            Assert.AreEqual(queueTrack, layer.CurrentlyPlayingFile);
            Assert.AreEqual(queueTrack, layer.PlayList[1]);
            Assert.AreEqual(2, layer.PlayList.Count);
            Assert.AreEqual(newTrack.Duration + queueTrack.Duration, layer.TotalDuration);
            Assert.IsTrue(layer.PlaybackLocation - newTrack.Duration < 1f);
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            SoundTestCleanup(layer, playingFiles);
            Context.AssetLoader.Destroy(queueTrack.Name);
            Context.AssetLoader.Destroy(newTrack.Name);
        }

        /// <summary>
        /// Test whether looping of single track works, when looping the last only. Additionally adding tracks to a last only loop.
        /// </summary>
        [TestMethod]
        public void LoopSingleLastOnlyAndAddingToLoop()
        {
              SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");
            layer.Looping = true;
            layer.LoopLastOnly = true;

            // Wait for track to be over, so it loops.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // First file should be playing now as it has looped.
            Assert.AreEqual(playingFiles[0], layer.CurrentlyPlayingFile);
            // And it should still be in the playlist, and as the only thing there.
            Assert.AreEqual(playingFiles[0], layer.PlayList[0]);
            Assert.AreEqual(1, layer.PlayList.Count);
            // And the duration shouldn't have changed.
            Assert.AreEqual(playingFiles[0].Duration, layer.TotalDuration);
            // And playback should be correct.
            Assert.IsTrue(layer.PlaybackLocation < 1f);

            // Play a new track on the looping layer.
            SoundFile newTrack = Context.AssetLoader.Get<SoundFile>("Sounds/sadMeme.wav");
            // Wait for it to be queued.
            layer.Play(newTrack).Wait();
            WaitForSoundLoops(1);

            // Check whether all the reporting is correct.
            Assert.AreEqual(newTrack, layer.CurrentlyPlayingFile);
            Assert.AreEqual(newTrack, layer.PlayList[0]);
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(newTrack.Duration, layer.TotalDuration);
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            // Check monitoring.
            Assert.AreEqual(newTrack, layer.CurrentlyPlayingFile);
            Assert.AreEqual(newTrack, layer.PlayList[0]);
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(newTrack.Duration, layer.TotalDuration);
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            SoundFile queueTrack = Context.AssetLoader.Get<SoundFile>("Sounds/money.wav");

            // Queue new track, and wait for a loop.
            layer.QueuePlay(queueTrack).Wait();
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            // Since we are only looping the last track only it should be left in the playlist.
            Assert.AreEqual(queueTrack, layer.CurrentlyPlayingFile);
            Assert.AreEqual(queueTrack, layer.PlayList[0]);
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(queueTrack.Duration, layer.TotalDuration);
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(queueTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            Assert.AreEqual(queueTrack, layer.CurrentlyPlayingFile);
            Assert.AreEqual(queueTrack, layer.PlayList[0]);
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(queueTrack.Duration, layer.TotalDuration);
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            SoundTestCleanup(layer, playingFiles);
            Context.AssetLoader.Destroy(queueTrack.Name);
            Context.AssetLoader.Destroy(newTrack.Name);
        }

        /// <summary>
        /// Test whether looping of a queue works.
        /// </summary>
        [TestMethod]
        public void LoopQueue()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav", "Sounds/sadMeme.wav");
            layer.Looping = true;
            layer.LoopLastOnly = false;

            // Get to the second track.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // Second file should be playing.
            Assert.AreEqual(playingFiles[1], layer.CurrentlyPlayingFile);
            // But the top of the playlist should still be the first file.
            Assert.AreEqual(playingFiles[0], layer.PlayList[0]);
            // And the duration shouldn't have changed.
            Assert.AreEqual(playingFiles[0].Duration + playingFiles[1].Duration, layer.TotalDuration);

            // Finish second track.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[1].Duration)).Wait();
            WaitForSoundLoops(1);

            // First file should be playing now as it has looped.
            Assert.AreEqual(playingFiles[0], layer.CurrentlyPlayingFile);
            // But the top of the playlist should still be the first file.
            Assert.AreEqual(playingFiles[0], layer.PlayList[0]);
            Assert.AreEqual(2, layer.PlayList.Count);
            // And the duration shouldn't have changed.
            Assert.AreEqual(playingFiles[0].Duration + playingFiles[1].Duration, layer.TotalDuration);

            // Should still be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether looping of a queue works, when looping the last only.
        /// </summary>
        [TestMethod]
        public void LoopLastQueue()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav", "Sounds/sadMeme.wav");
            layer.Looping = true;
            layer.LoopLastOnly = true;

            // Get to the second track.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // Second file should be playing.
            Assert.AreEqual(playingFiles[1], layer.CurrentlyPlayingFile);
            // Duration should've changed as the first track is removed from the playlist as it won't loop.
            Assert.AreEqual(playingFiles[1], layer.PlayList[0]);
            Assert.AreEqual(playingFiles[1].Duration, layer.TotalDuration);

            // Finish second track.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[1].Duration)).Wait();
            WaitForSoundLoops(1);

            // Second file should be playing again as we are only looping last.
            Assert.AreEqual(playingFiles[1], layer.CurrentlyPlayingFile);
            Assert.AreEqual(playingFiles[1], layer.PlayList[0]);
            Assert.AreEqual(1, layer.PlayList.Count);
            Assert.AreEqual(playingFiles[1].Duration, layer.TotalDuration);

            // Should still be playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether pausing, resuming, and stopping of sounds. Includes changing of the looping settings on a paused layer.
        /// </summary>
        [TestMethod]
        public void PauseResumeStopSound()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");

            // Playback location should be somewhere at the start.
            float oldPlayback = layer.PlaybackLocation;
            Assert.IsTrue(oldPlayback < 1);

            // Pause.
            layer.Pause().Wait();
            Assert.IsTrue(layer.Status == SoundStatus.Paused);

            // Wait for 1 seconds.
            Task.Delay(1000).Wait();

            // Should still be paused.
            Assert.IsTrue(layer.Status == SoundStatus.Paused);
            // Duration shouldn't be changed.
            Assert.IsTrue(layer.PlaybackLocation - oldPlayback < 1f);

            // Try to pause again.
            layer.Pause().Wait();
            Assert.IsTrue(layer.Status == SoundStatus.Paused);

            // Change looping setting.
            layer.Looping = true;
            WaitForSoundLoops(1);
            Assert.IsTrue(layer.Status == SoundStatus.Paused);
            layer.Looping = false;
            WaitForSoundLoops(1);

            // Resume.
            layer.Resume().Wait();
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Wait for a second.
            Task.Delay(1000).Wait();

            Assert.IsTrue(layer.Status == SoundStatus.Playing);
            Assert.IsTrue(layer.PlaybackLocation - (oldPlayback + 1f) < 1f);

            // Try to resume again.
            layer.Resume().Wait();
            Assert.IsTrue(layer.Status == SoundStatus.Playing);

            // Stop playing.
            layer.StopPlayingAll().Wait();

            // Try to pause stopped.
            layer.Pause().Wait();
            Assert.IsTrue(layer.Status == SoundStatus.Stopped);

            // Try to resume stopped.
            layer.Resume().Wait();
            Assert.IsTrue(layer.Status == SoundStatus.Stopped);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether playing and queueing on a paused layer works as expected.
        /// </summary>
        [TestMethod]
        public void PlayOnPausedLayer()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");

            // Playback location should be somewhere at the start.
            float oldPlayback = layer.PlaybackLocation;
            Assert.IsTrue(oldPlayback < 1);

            // Pause.
            layer.Pause().Wait();
            Assert.IsTrue(layer.Status == SoundStatus.Paused);

            // Wait for 2 seconds.
            Task.Delay(2000).Wait();

            // Should still be paused.
            Assert.IsTrue(layer.Status == SoundStatus.Paused);
            // Duration shouldn't be changed.
            Assert.IsTrue(layer.PlaybackLocation - oldPlayback < 1f);

            // Play a sound.
            layer.Play(playingFiles[0]).Wait();

            // Should've started playing.
            Assert.IsTrue(layer.Status == SoundStatus.Playing);
            Assert.IsTrue(layer.PlaybackLocation < 1);

            // Pause.
            layer.Pause().Wait();
            Assert.IsTrue(layer.Status == SoundStatus.Paused);

            // Queue a sound.
            layer.QueuePlay(playingFiles[0]).Wait();

            // Shouldn't play. But should be queued.
            Assert.IsTrue(layer.Status == SoundStatus.Paused);
            Assert.IsTrue(layer.PlaybackLocation < 1);
            Assert.AreEqual(2, layer.PlayList.Count);

            // Resume.
            layer.Resume().Wait();

            // Wait three seconds.
            Task.Delay(3100).Wait();

            Assert.IsTrue(layer.Status == SoundStatus.Playing);
            Assert.IsTrue(layer.PlaybackLocation < 1);
            Assert.AreEqual(1, layer.PlayList.Count);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether sound is paused when the host is paused.
        /// </summary>
        [TestMethod]
        public void FocusLossPause()
        {
            // Get the host.
            TestHost host = TestInit.TestingHost;

            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");

            // Check playback.
            Assert.IsTrue(layer.PlaybackLocation < 1f);

            // Defocus host.
            host.Focused = false;

            // Check playback.
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Task.Delay(1000).Wait();
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.AreEqual(SoundStatus.FocusLossPause, layer.Status);

            // Resume and check again.
            host.Focused = true;
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Task.Delay(1000).Wait();
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            Assert.AreEqual(SoundStatus.Playing, layer.Status);

            // Pause again.
            host.Focused = false;
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            Task.Delay(1000).Wait();
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            Assert.AreEqual(SoundStatus.FocusLossPause, layer.Status);

            // Resume while it is focus loss paused.
            layer.Resume();
            Assert.IsTrue(layer.PlaybackLocation  - 1f < 1f);
            Assert.AreEqual(SoundStatus.FocusLossPause, layer.Status);

            // Play a track. Which shouldn't have changed it too.
            layer.Play(playingFiles[0]);
            Assert.IsTrue(layer.PlaybackLocation  - 1f < 1f);
            Assert.AreEqual(SoundStatus.FocusLossPause, layer.Status);

            // Queue while focus paused.
            layer.QueuePlay(playingFiles[0]);
            Assert.IsTrue(layer.PlaybackLocation  - 1f < 1f);
            Assert.AreEqual(SoundStatus.FocusLossPause, layer.Status);
            Assert.AreEqual(1, layer.PlayList.Count);

            // Resume. All the things should run now.
            host.Focused = true;
            WaitForSoundLoops(1);
            Assert.AreEqual(2, layer.PlayList.Count);
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.AreEqual(SoundStatus.Playing, layer.Status);

            // Pause.
            layer.Pause().Wait();
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.AreEqual(SoundStatus.Paused, layer.Status);
            // Remove focus.
            host.Focused = false;
            WaitForSoundLoops(1);
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.AreEqual(SoundStatus.Paused, layer.Status);
            // Restore focus.
            host.Focused = true;
            WaitForSoundLoops(1);
            // Should still be paused.
            Assert.IsTrue(layer.PlaybackLocation < 1f);
            Assert.AreEqual(SoundStatus.Paused, layer.Status);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether global and local volume affect the layer as expected.
        /// </summary>
        [TestMethod]
        public void SoundLayerVolume()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/sadMeme.wav");

            float reportedVolumeFirst = layer.ReportedVolume;

            // Change global volume.
            Context.Settings.SoundSettings.Volume = 50;
            WaitForSoundLoops(1);
            float reportedVolumeSecond = layer.ReportedVolume;

            // Reported volume should be lower.
            Assert.IsTrue(reportedVolumeSecond < reportedVolumeFirst);

            // Lower local volume.
            layer.Volume = 50;
            WaitForSoundLoops(1);
            float reportedVolumeThird = layer.ReportedVolume;

            // Reported volume should be lower.
            Assert.IsTrue(reportedVolumeThird < reportedVolumeSecond);

            // Raise global volume.
            Context.Settings.SoundSettings.Volume = 100;
            WaitForSoundLoops(1);
            Assert.IsTrue(layer.ReportedVolume > reportedVolumeThird);

            // Restore both.
            layer.Volume = 100;
            WaitForSoundLoops(1);
            Assert.IsTrue(layer.ReportedVolume == reportedVolumeFirst);

            // Set sound to off.
            Context.Settings.SoundSettings.Sound = false;
            WaitForSoundLoops(1);
            Assert.IsTrue(layer.ReportedVolume == 0f);

            // Restore
            Context.Settings.SoundSettings.Sound = true;

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test fading in.
        /// </summary>
        [TestMethod]
        public void FadeIn()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/money.wav");
            layer.FadeInLength = 2;

            // Should fade in.
            Task.Delay(1000).Wait();
            Assert.IsTrue(layer.ReportedVolume < 1f);

            // Should be at full.
            Task.Delay(1000).Wait();
            Assert.AreEqual(1, layer.ReportedVolume);

            // Loop layer.
            layer.Looping = true;
            // Wait for sound to loop.
            Task.Delay(3000).Wait();
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            // Should fade in again.
            Assert.IsTrue(layer.ReportedVolume < 1f);

            // Fade in on the first loop only.
            layer.FadeInFirstLoopOnly = true;
            // Wait for loop. It should not take effect.
            Task.Delay(5000).Wait();
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            Assert.AreEqual(1f, layer.ReportedVolume);

            // Reset
            layer.StopPlayingAll();
            layer.Play(playingFiles[0]).Wait();
            Task.Delay(100).Wait();
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            Assert.IsTrue(layer.ReportedVolume < 1f);

            // Wait for a loop.
            Task.Delay(5000).Wait();
            // Should not take effect now.
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            Assert.AreEqual(1f, layer.ReportedVolume);

            // Reset. Queue two, should fade in on first only.
            layer.StopPlayingAll();
            layer.Play(playingFiles[0]).Wait();
            layer.QueuePlay(playingFiles[0]).Wait();
            WaitForSoundLoops(1);
            Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            Assert.IsTrue(layer.ReportedVolume < 1f);

            // Wait for first to be over.
            Task.Delay(5000).Wait();
            Assert.AreEqual(1f, layer.ReportedVolume);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test fading out.
        /// </summary>
        [TestMethod]
        public void FadeOut()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/money.wav");
            layer.FadeOutLength = 2;

            //// Should start at 1f.
            //Assert.AreEqual(1f, layer.ReportedVolume);

            //// Should fade out.
            //Task.Delay(3100).Wait();
            //float vol = layer.ReportedVolume;
            //Assert.IsTrue(vol < 1f);

            //// Should be at full.
            //Task.Delay(1000).Wait();
            //Assert.IsTrue(layer.ReportedVolume < vol);

            //// Loop layer.
            //layer.Looping = true;
            //// Wait for sound to loop.
            //Task.Delay(2000).Wait();
            //Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            //Assert.AreEqual(1f, layer.ReportedVolume);
            //// Should fade out again.
            //Task.Delay(3000).Wait();
            //Assert.IsTrue(layer.PlaybackLocation - 4f < 1f);
            //Assert.IsTrue(layer.ReportedVolume < vol);

            //// Reset. Queue two, should fade out on second only.
            //layer.StopPlayingAll();
            //layer.Play(playingFiles[0]).Wait();
            //layer.QueuePlay(playingFiles[0]).Wait();
            //Task.Delay(100).Wait();
            //Assert.IsTrue(layer.PlaybackLocation - 1f < 1f);
            //Assert.AreEqual(1f, layer.ReportedVolume);
            //Task.Delay(3000).Wait();
            //Assert.AreEqual(1f, layer.ReportedVolume);
            //// Wait for second one.
            //Task.Delay(5000).Wait();
            //Assert.IsTrue(layer.ReportedVolume < 1f);

            SoundFile newFile = Context.AssetLoader.Get<SoundFile>("Sounds/sadMeme.wav");

            // Reset. Setup fade out on change.
            layer.StopPlayingAll().Wait();
            layer.FadeOutOnChange = true;
            layer.Play(playingFiles[0]).Wait();
            // Should now be playing.
            WaitForSoundLoops(1);
            Assert.AreEqual(SoundStatus.Playing, layer.Status);
            Assert.AreEqual(1, layer.ReportedVolume);
            // Now play another file.
            layer.Play(newFile);
            // Should still be playing first, but be fading out.
            WaitForSoundLoops(1);
            Assert.AreEqual(SoundStatus.Playing, layer.Status);
            Assert.AreEqual(playingFiles[0], layer.CurrentlyPlayingFile);
            Assert.IsTrue(layer.ReportedVolume < 1f);
            Task.Delay(2300).Wait();
            WaitForSoundLoops(1);
            Assert.AreEqual(newFile, layer.CurrentlyPlayingFile);
            // Stop playing.
            layer.StopPlayingAll();
            WaitForSoundLoops(1);
            Assert.AreEqual(SoundStatus.Playing, layer.Status);
            Assert.IsTrue(layer.ReportedVolume < 1f);
            Task.Delay(2300).Wait();
            WaitForSoundLoops(1);
            Assert.AreEqual(SoundStatus.Stopped, layer.Status);

            // Play again. Try to queue something. This shouldn't cause a fade out.
            layer.Play(newFile);
            WaitForSoundLoops(2);
            Assert.AreEqual(SoundStatus.Playing, layer.Status);
            Assert.AreEqual(1f, layer.ReportedVolume);
            layer.QueuePlay(playingFiles[0]);
            Task.Delay(1000).Wait();
            Assert.AreEqual(1f, layer.ReportedVolume);
            Assert.AreEqual(newFile, layer.CurrentlyPlayingFile);
            Task.Delay(TimeSpan.FromSeconds(newFile.Duration - 1f)).Wait();
            Assert.AreEqual(1f, layer.ReportedVolume);
            Task.Delay(100).Wait();
            Assert.AreEqual(1f, layer.ReportedVolume);
            Assert.AreEqual(playingFiles[0], layer.CurrentlyPlayingFile);
            layer.SkipNaturalFadeOut = true;
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration - 1.5f)).Wait();
            // Shouldn't be fading out as skip is true.
            Assert.AreEqual(1f, layer.ReportedVolume);

            layer.StopPlayingAll(true).Wait();
            Context.AssetLoader.Destroy(newFile.Name);
            WaitForSoundLoops(1);
            SoundTestCleanup(layer, playingFiles);
        }

        #region Helpers

        /// <summary>
        /// Setup a sound testing environment. Creates a scene, sound layer, loads tracks, and performs checks.
        /// </summary>
        /// <param name="playingFiles">The playing tracks.</param>
        /// <param name="layer">The created layer.</param>
        /// <param name="filesToPlay">Files to play on layer.</param>
        private void SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, params string[] filesToPlay)
        {
            SoundTestStartNoChecks(out SoundFile[] files, out SoundLayer testLayer, filesToPlay);
            playingFiles = files;
            layer = testLayer;

            // Ensure everything loaded correctly.
            Assert.AreEqual(playingFiles.Sum(x => x.Duration), layer.TotalDuration);
            Assert.AreEqual(playingFiles.Length, layer.PlayList.Count);
            for (int i = 0; i < playingFiles.Length; i++)
            {
                Assert.AreEqual(playingFiles[i], layer.PlayList[i]);
            }
            Assert.AreEqual(SoundStatus.Playing, layer.Status);
            Assert.AreEqual(playingFiles[0], layer.CurrentlyPlayingFile);
            Assert.AreEqual(0, layer.CurrentlyPlayingFileIndex);
        }

        /// <summary>
        /// Setup a sound testing environment. Creates a scene, sound layer, loads tracks.
        /// </summary>
        /// <param name="playingFiles">The playing tracks.</param>
        /// <param name="layer">The created layer.</param>
        /// <param name="filesToPlay">Files to play on layer.</param>
        private void SoundTestStartNoChecks(out SoundFile[] playingFiles, out SoundLayer layer, params string[] filesToPlay)
        {
            // Create a holder for the sound file.
            List<SoundFile> files = new List<SoundFile>();

            // Create scene for this test.
            ExternalScene extScene = new ExternalScene
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    foreach (string trackName in filesToPlay)
                    {
                        SoundFile track = Context.AssetLoader.Get<SoundFile>(trackName);
                        if (filesToPlay.Length == 1) Context.SoundManager.Play(track, "testLayer");
                        else
                            Context.SoundManager.QueuePlay(track, "testLayer");
                        files.Add(track);
                    }
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Set instant sound thread.
            Context.Flags.SoundThreadFrequency = 1;

            // Ensure layer is proper and get it.
            Assert.AreEqual(1, Context.SoundManager.Layers.Length);
            layer = Context.SoundManager.GetLayer("testLayer");
            Assert.AreEqual("testLayer", layer.Name);

            // Wait for two loops.
            WaitForSoundLoops(4);

            // Assign loaded files.
            playingFiles = files.ToArray();
        }

        /// <summary>
        /// Cleans up the sound testing environment.
        /// </summary>
        /// <param name="layer">The sound layer to cleanup.</param>
        /// <param name="playingFiles">The sound files to cleanup.</param>
        private void SoundTestCleanup(SoundLayer layer, SoundFile[] playingFiles)
        {
            // Stop playing.
            layer.StopPlayingAll(true);

            // Cleanup sound.
            foreach (SoundFile file in playingFiles)
            {
                Context.AssetLoader.Destroy(file.Name);
            }

            // Wait for cleanup. Only one file will be cleaned up per loop.
            WaitForSoundLoops(playingFiles.Length);

            // Should be cleaned up.
            foreach (SoundFile file in playingFiles)
            {
                Assert.AreEqual(-1, file.ALBuffer);
            }

            // Remove the layer.
            Context.SoundManager.RemoveLayer("testLayer");
            Assert.AreEqual(0, Context.SoundManager.Layers.Length);

            // Cleanup.
            Helpers.UnloadScene();

            // Restore sound thread frequency.
            Context.Flags.SoundThreadFrequency = 200;
        }

        /// <summary>
        /// Wait for the sound manager to loop.
        /// </summary>
        /// <param name="loopCount">How many times it should loop.</param>
        private void WaitForSoundLoops(int loopCount)
        {
            for (int i = 0; i < loopCount; i++)
            {
                Context.SoundManager.GetOneLoopToken().Wait();
            }
        }

        #endregion
    }
}