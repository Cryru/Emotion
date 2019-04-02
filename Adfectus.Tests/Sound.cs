#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Adfectus.Common;
using Adfectus.Sound;
using Adfectus.Tests.Scenes;
using Xunit;

#endregion

namespace Adfectus.Tests
{
    /// <summary>
    /// Tests connected with sound and the SoundManager module.
    /// </summary>
    [Collection("main")]
    public class Sound
    {
        /// <summary>
        /// Test whether loading and playing of sound works.
        /// </summary>
        [Fact]
        public void LoadAndPlaySound()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/kids.wav");

            // Record memory usage for checking after cleanup.
            long memoryUsage = Process.GetCurrentProcess().WorkingSet64;

            // File should've loaded correctly.
            Assert.Equal(25f, playingFiles[0].Duration);

            // Playback location should be somewhere at the start.
            float oldPlayback = layer.PlaybackLocation;
            Assert.True(oldPlayback < 1);

            // Wait for 5 seconds.
            Task.Delay(5000).Wait();

            // The current duration should be around 5 seconds in.
            Assert.True(layer.PlaybackLocation - (oldPlayback + 5) < 1);

            SoundTestCleanup(layer, playingFiles);

            // Memory usage should've fallen after cleanup.
            Assert.True(memoryUsage > Process.GetCurrentProcess().WorkingSet64);
        }

        /// <summary>
        /// Test whether looping of a sound works. Also tests loading of stereo files as all other tests use mono tracks.
        /// </summary>
        [Fact]
        public void LoopSound()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noiceStereo.wav");
            layer.Looping = true;

            // Assert that the sound file was loaded. The duration of the track is around 3 seconds.
            Assert.True(playingFiles[0].Duration - 3f < 1f);

            // Playback location should be somewhere at the start.
            Assert.True(layer.PlaybackLocation < 1f);

            // Wait for the duration of the track. This should cause it to loop.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();

            // Should still be playing.
            Assert.True(layer.Status == SoundStatus.Playing);

            // Duration should be about at the end of the second loop.
            Assert.True(layer.PlaybackLocation < 1f);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether queuing of sounds works.
        /// </summary>
        [Fact]
        public void SoundQueue()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav", "Sounds/kids.wav");

            // Playback location should be somewhere at the start.
            Assert.True(layer.PlaybackLocation < 1f);

            // Wait for 1 second.
            Task.Delay(1000).Wait();

            // Should still be playing.
            Assert.True(layer.Status == SoundStatus.Playing);
            // Check playback, should be around a second in.
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            // Should still be on the first file.
            Assert.Equal(playingFiles[0], layer.CurrentlyPlayingFile);

            // Wait for 2 more seconds for the first track to end.
            Task.Delay(2000).Wait();

            // Should still be playing.
            Assert.True(layer.Status == SoundStatus.Playing);

            // Playback should be in the beginning of the second file.
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.Equal(playingFiles[1], layer.CurrentlyPlayingFile);
            // Playlist should be one file.
            Assert.Single(layer.PlayList);
            Assert.Equal(playingFiles[1], layer.PlayList[0]);
            // Duration should be only the second file.
            Assert.Equal(playingFiles[1].Duration, layer.TotalDuration);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether queuing of files with a different channel count produces the expected negative result, as it is an
        /// unsupported operation.
        /// </summary>
        [Fact]
        public void SoundQueueChannelMix()
        {
            SoundTestStartNoChecks(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav", "Sounds/noiceStereo.wav");

            // Only the first queued track should play. As the other one could not be queued.
            Assert.Equal(playingFiles[0], layer.CurrentlyPlayingFile);
            // Only one track in the playlist.
            Assert.Single(layer.PlayList);
            Assert.Equal(playingFiles[0], layer.PlayList[0]);

            // Duration shouldn't include the second track.
            Assert.Equal(playingFiles[0].Duration, layer.TotalDuration);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether queuing of sounds works after it has stopped playing once.
        /// </summary>
        [Fact]
        public void SoundQueueAfterFinish()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");

            // Wait for it to finish.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // Should no longer be playing.
            Assert.True(layer.Status == SoundStatus.Stopped);
            // Current file should be none.
            Assert.Null(layer.CurrentlyPlayingFile);
            // Playlist should be empty.
            Assert.Empty(layer.PlayList);

            // Add new file.
            SoundFile newFile = Engine.AssetLoader.Get<SoundFile>("Sounds/kids.wav");

            // Queue a new sound.
            Engine.SoundManager.QueuePlay(newFile, "testLayer");

            // Loop sound manager once.
            WaitForSoundLoops(1);
            Assert.Equal(SoundStatus.Playing, layer.Status);
            Assert.True(layer.PlaybackLocation < 1f);

            // Stop playing.
            layer.StopPlayingAll();

            // Play again.
            Engine.SoundManager.Play(newFile, "testLayer");

            // Wait until it starts playing.
            WaitForSoundLoops(1);
            Assert.Equal(SoundStatus.Playing, layer.Status);
            Assert.True(layer.PlaybackLocation < 1f);

            SoundTestCleanup(layer, playingFiles);
            Engine.AssetLoader.Destroy(newFile.Name);
        }

        /// <summary>
        /// Test whether looping of single track works. Additionally adding tracks to a loop.
        /// </summary>
        [Fact]
        public void LoopSingleAndAddingToLoop()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");
            layer.Looping = true;
            layer.LoopLastOnly = false;

            // Wait for track to be over, so it loops.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // First file should be playing now as it has looped.
            Assert.Equal(playingFiles[0], layer.CurrentlyPlayingFile);
            // And it should still be in the playlist, and as the only thing there.
            Assert.Equal(playingFiles[0], layer.PlayList[0]);
            Assert.Single(layer.PlayList);
            // And the duration shouldn't have changed.
            Assert.Equal(playingFiles[0].Duration, layer.TotalDuration);
            // And playback should be correct.
            Assert.True(layer.PlaybackLocation < 1f);

            // Play a new track on the looping layer.
            SoundFile newTrack = Engine.AssetLoader.Get<SoundFile>("Sounds/money.wav");
            // Wait for it to be queued.
            layer.Play(newTrack).Wait();
            WaitForSoundLoops(1);

            // Check whether all the reporting is correct.
            Assert.Equal(newTrack, layer.CurrentlyPlayingFile);
            Assert.Equal(newTrack, layer.PlayList[0]);
            Assert.Single(layer.PlayList);
            Assert.Equal(newTrack.Duration, layer.TotalDuration);
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.True(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            // Check monitoring.
            Assert.Equal(newTrack, layer.CurrentlyPlayingFile);
            Assert.Equal(newTrack, layer.PlayList[0]);
            Assert.Single(layer.PlayList);
            Assert.Equal(newTrack.Duration, layer.TotalDuration);
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.True(layer.Status == SoundStatus.Playing);

            SoundFile queueTrack = Engine.AssetLoader.Get<SoundFile>("Sounds/sadMeme.wav");

            // Queue new track, and wait for a loop.
            layer.QueuePlay(queueTrack).Wait();
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            // Should be playing second track.
            Assert.Equal(queueTrack, layer.CurrentlyPlayingFile);
            Assert.Equal(queueTrack, layer.PlayList[1]);
            Assert.Equal(2, layer.PlayList.Count);
            Assert.Equal(newTrack.Duration + queueTrack.Duration, layer.TotalDuration);
            Assert.True(layer.PlaybackLocation - newTrack.Duration < 1f);
            Assert.True(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(queueTrack.Duration)).Wait();

            Assert.Equal(newTrack, layer.CurrentlyPlayingFile);
            Assert.Equal(newTrack, layer.PlayList[0]);
            Assert.Equal(2, layer.PlayList.Count);
            Assert.Equal(newTrack.Duration + queueTrack.Duration, layer.TotalDuration);
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.True(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            Assert.Equal(queueTrack, layer.CurrentlyPlayingFile);
            Assert.Equal(queueTrack, layer.PlayList[1]);
            Assert.Equal(2, layer.PlayList.Count);
            Assert.Equal(newTrack.Duration + queueTrack.Duration, layer.TotalDuration);
            Assert.True(layer.PlaybackLocation - newTrack.Duration < 1f);
            Assert.True(layer.Status == SoundStatus.Playing);

            SoundTestCleanup(layer, playingFiles);
            Engine.AssetLoader.Destroy(queueTrack.Name);
            Engine.AssetLoader.Destroy(newTrack.Name);
        }

        /// <summary>
        /// Test whether looping of single track works, when looping the last only. Additionally adding tracks to a last only loop.
        /// </summary>
        [Fact]
        public void LoopSingleLastOnlyAndAddingToLoop()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");
            layer.Looping = true;
            layer.LoopLastOnly = true;

            // Wait for track to be over, so it loops.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // First file should be playing now as it has looped.
            Assert.Equal(playingFiles[0], layer.CurrentlyPlayingFile);
            // And it should still be in the playlist, and as the only thing there.
            Assert.Equal(playingFiles[0], layer.PlayList[0]);
            Assert.Single(layer.PlayList);
            // And the duration shouldn't have changed.
            Assert.Equal(playingFiles[0].Duration, layer.TotalDuration);
            // And playback should be correct.
            Assert.True(layer.PlaybackLocation < 1f);

            // Play a new track on the looping layer.
            SoundFile newTrack = Engine.AssetLoader.Get<SoundFile>("Sounds/sadMeme.wav");
            // Wait for it to be queued.
            layer.Play(newTrack).Wait();
            WaitForSoundLoops(1);

            // Check whether all the reporting is correct.
            Assert.Equal(newTrack, layer.CurrentlyPlayingFile);
            Assert.Equal(newTrack, layer.PlayList[0]);
            Assert.Single(layer.PlayList);
            Assert.Equal(newTrack.Duration, layer.TotalDuration);
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.True(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            // Check monitoring.
            Assert.Equal(newTrack, layer.CurrentlyPlayingFile);
            Assert.Equal(newTrack, layer.PlayList[0]);
            Assert.Single(layer.PlayList);
            Assert.Equal(newTrack.Duration, layer.TotalDuration);
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.True(layer.Status == SoundStatus.Playing);

            SoundFile queueTrack = Engine.AssetLoader.Get<SoundFile>("Sounds/money.wav");

            // Queue new track, and wait for a loop.
            layer.QueuePlay(queueTrack).Wait();
            Task.Delay(TimeSpan.FromSeconds(newTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            // Since we are only looping the last track only it should be left in the playlist.
            Assert.Equal(queueTrack, layer.CurrentlyPlayingFile);
            Assert.Equal(queueTrack, layer.PlayList[0]);
            Assert.Single(layer.PlayList);
            Assert.Equal(queueTrack.Duration, layer.TotalDuration);
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.True(layer.Status == SoundStatus.Playing);

            // Wait for a loop.
            Task.Delay(TimeSpan.FromSeconds(queueTrack.Duration)).Wait();
            WaitForSoundLoops(1);

            Assert.Equal(queueTrack, layer.CurrentlyPlayingFile);
            Assert.Equal(queueTrack, layer.PlayList[0]);
            Assert.Single(layer.PlayList);
            Assert.Equal(queueTrack.Duration, layer.TotalDuration);
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.True(layer.Status == SoundStatus.Playing);

            SoundTestCleanup(layer, playingFiles);
            Engine.AssetLoader.Destroy(queueTrack.Name);
            Engine.AssetLoader.Destroy(newTrack.Name);
        }

        /// <summary>
        /// Test whether looping of a queue works.
        /// </summary>
        [Fact]
        public void LoopQueue()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav", "Sounds/sadMeme.wav");
            layer.Looping = true;
            layer.LoopLastOnly = false;

            // Get to the second track.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // Second file should be playing.
            Assert.Equal(playingFiles[1], layer.CurrentlyPlayingFile);
            // But the top of the playlist should still be the first file.
            Assert.Equal(playingFiles[0], layer.PlayList[0]);
            // And the duration shouldn't have changed.
            Assert.Equal(playingFiles[0].Duration + playingFiles[1].Duration, layer.TotalDuration);

            // Finish second track.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[1].Duration)).Wait();
            WaitForSoundLoops(1);

            // First file should be playing now as it has looped.
            Assert.Equal(playingFiles[0], layer.CurrentlyPlayingFile);
            // But the top of the playlist should still be the first file.
            Assert.Equal(playingFiles[0], layer.PlayList[0]);
            Assert.Equal(2, layer.PlayList.Count);
            // And the duration shouldn't have changed.
            Assert.Equal(playingFiles[0].Duration + playingFiles[1].Duration, layer.TotalDuration);

            // Should still be playing.
            Assert.True(layer.Status == SoundStatus.Playing);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether looping of a queue works, when looping the last only.
        /// </summary>
        [Fact]
        public void LoopLastQueue()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav", "Sounds/sadMeme.wav");
            layer.Looping = true;
            layer.LoopLastOnly = true;

            // Get to the second track.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration)).Wait();
            WaitForSoundLoops(1);

            // Second file should be playing.
            Assert.Equal(playingFiles[1], layer.CurrentlyPlayingFile);
            // Duration should've changed as the first track is removed from the playlist as it won't loop.
            Assert.Equal(playingFiles[1], layer.PlayList[0]);
            Assert.Equal(playingFiles[1].Duration, layer.TotalDuration);

            // Finish second track.
            Task.Delay(TimeSpan.FromSeconds(playingFiles[1].Duration)).Wait();
            WaitForSoundLoops(1);

            // Second file should be playing again as we are only looping last.
            Assert.Equal(playingFiles[1], layer.CurrentlyPlayingFile);
            Assert.Equal(playingFiles[1], layer.PlayList[0]);
            Assert.Single(layer.PlayList);
            Assert.Equal(playingFiles[1].Duration, layer.TotalDuration);

            // Should still be playing.
            Assert.True(layer.Status == SoundStatus.Playing);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether pausing, resuming, and stopping of sounds. Includes changing of the looping settings on a paused layer.
        /// </summary>
        [Fact]
        public void PauseResumeStopSound()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");

            // Playback location should be somewhere at the start.
            float oldPlayback = layer.PlaybackLocation;
            Assert.True(oldPlayback < 1);

            // Pause.
            layer.Pause().Wait();
            Assert.True(layer.Status == SoundStatus.Paused);

            // Wait for 1 seconds.
            Task.Delay(1000).Wait();

            // Should still be paused.
            Assert.True(layer.Status == SoundStatus.Paused);
            // Duration shouldn't be changed.
            Assert.True(layer.PlaybackLocation - oldPlayback < 1f);

            // Try to pause again.
            layer.Pause().Wait();
            Assert.True(layer.Status == SoundStatus.Paused);

            // Change looping setting.
            layer.Looping = true;
            WaitForSoundLoops(1);
            Assert.True(layer.Status == SoundStatus.Paused);
            layer.Looping = false;
            WaitForSoundLoops(1);

            // Resume.
            layer.Resume().Wait();
            Assert.True(layer.Status == SoundStatus.Playing);

            // Wait for a second.
            Task.Delay(1000).Wait();

            Assert.True(layer.Status == SoundStatus.Playing);
            Assert.True(layer.PlaybackLocation - (oldPlayback + 1f) < 1f);

            // Try to resume again.
            layer.Resume().Wait();
            Assert.True(layer.Status == SoundStatus.Playing);

            // Stop playing.
            layer.StopPlayingAll().Wait();

            // Try to pause stopped.
            layer.Pause().Wait();
            Assert.True(layer.Status == SoundStatus.Stopped);

            // Try to resume stopped.
            layer.Resume().Wait();
            Assert.True(layer.Status == SoundStatus.Stopped);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether playing and queueing on a paused layer works as expected.
        /// </summary>
        [Fact]
        public void PlayOnPausedLayer()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");

            // Playback location should be somewhere at the start.
            float oldPlayback = layer.PlaybackLocation;
            Assert.True(oldPlayback < 1);

            // Pause.
            layer.Pause().Wait();
            Assert.True(layer.Status == SoundStatus.Paused);

            // Wait for 2 seconds.
            Task.Delay(2000).Wait();

            // Should still be paused.
            Assert.True(layer.Status == SoundStatus.Paused);
            // Duration shouldn't be changed.
            Assert.True(layer.PlaybackLocation - oldPlayback < 1f);

            // Play a sound.
            layer.Play(playingFiles[0]).Wait();
            WaitForSoundLoops(1);

            // Should've started playing.
            Assert.True(layer.Status == SoundStatus.Playing);
            Assert.True(layer.PlaybackLocation < 1);

            // Pause.
            layer.Pause().Wait();
            Assert.True(layer.Status == SoundStatus.Paused);

            // Queue a sound.
            layer.QueuePlay(playingFiles[0]).Wait();

            // Shouldn't play. But should be queued.
            Assert.True(layer.Status == SoundStatus.Paused);
            Assert.True(layer.PlaybackLocation < 1);
            Assert.Equal(2, layer.PlayList.Count);

            // Resume.
            layer.Resume().Wait();

            // Wait three seconds.
            Task.Delay(3100).Wait();

            Assert.True(layer.Status == SoundStatus.Playing);
            Assert.True(layer.PlaybackLocation < 1);
            Assert.Single(layer.PlayList);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether sound is paused when the host is paused.
        /// </summary>
        [Fact]
        public void FocusLossPause()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/noice.wav");

            // Check playback.
            Assert.True(layer.PlaybackLocation < 1f);

            // Defocus host.
            Engine.ForceUnfocus(true);
            WaitForSoundLoops(1);

            // Check playback.
            Assert.True(layer.PlaybackLocation < 1f);
            Task.Delay(1000).Wait();
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.Equal(SoundStatus.FocusLossPause, layer.Status);

            // Resume and check again.
            Engine.ForceUnfocus(false);
            WaitForSoundLoops(1);

            Assert.True(layer.PlaybackLocation < 1f);
            Task.Delay(1000).Wait();
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.Equal(SoundStatus.Playing, layer.Status);

            // Pause again.
            Engine.ForceUnfocus(true);
            WaitForSoundLoops(1);

            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Task.Delay(1000).Wait();
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.Equal(SoundStatus.FocusLossPause, layer.Status);

            // Resume while it is focus loss paused.
            layer.Resume();
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.Equal(SoundStatus.FocusLossPause, layer.Status);

            // Play a track. Which shouldn't have changed it too.
            layer.Play(playingFiles[0]);
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.Equal(SoundStatus.FocusLossPause, layer.Status);

            // Queue while focus paused.
            layer.QueuePlay(playingFiles[0]);
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.Equal(SoundStatus.FocusLossPause, layer.Status);
            Assert.Single(layer.PlayList);

            // Resume. All the things should run now.
            Engine.ForceUnfocus(false);
            WaitForSoundLoops(1);
            Assert.Equal(2, layer.PlayList.Count);
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.Equal(SoundStatus.Playing, layer.Status);

            // Pause.
            layer.Pause().Wait();
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.Equal(SoundStatus.Paused, layer.Status);
            // Remove focus.
            Engine.ForceUnfocus(true);
            WaitForSoundLoops(1);
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.Equal(SoundStatus.Paused, layer.Status);

            // Restore focus.
            Engine.ForceUnfocus(false);
            WaitForSoundLoops(1);
            // Should still be paused.
            Assert.True(layer.PlaybackLocation < 1f);
            Assert.Equal(SoundStatus.Paused, layer.Status);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test whether global and local volume affect the layer as expected.
        /// </summary>
        [Fact]
        public void SoundLayerVolume()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/sadMeme.wav");

            float reportedVolumeFirst = layer.ReportedVolume;

            // Change global volume.
            Engine.SoundManager.Volume = 50;
            WaitForSoundLoops(1);
            float reportedVolumeSecond = layer.ReportedVolume;

            // Reported volume should be lower.
            Assert.True(reportedVolumeSecond < reportedVolumeFirst);

            // Lower local volume.
            layer.Volume = 50;
            WaitForSoundLoops(1);
            float reportedVolumeThird = layer.ReportedVolume;

            // Reported volume should be lower.
            Assert.True(reportedVolumeThird < reportedVolumeSecond);

            // Raise global volume.
            Engine.SoundManager.Volume = 100;
            WaitForSoundLoops(1);
            Assert.True(layer.ReportedVolume > reportedVolumeThird);

            // Restore both.
            layer.Volume = 100;
            WaitForSoundLoops(1);
            Assert.True(layer.ReportedVolume == reportedVolumeFirst);

            // Set sound to off.
            Engine.SoundManager.Sound = false;
            WaitForSoundLoops(1);
            Assert.True(layer.ReportedVolume == 0f);

            // Restore
            Engine.SoundManager.Sound = true;

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test fading in.
        /// </summary>
        [Fact]
        public void FadeIn()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/money.wav");
            layer.FadeInLength = 2;

            // Should fade in.
            Task.Delay(1000).Wait();
            Assert.True(layer.ReportedVolume < 1f);

            // Should be at full.
            Task.Delay(1000).Wait();
            Assert.Equal(1, layer.ReportedVolume);

            // Loop layer.
            layer.Looping = true;
            // Wait for sound to loop.
            Task.Delay(3000).Wait();
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            // Should fade in again.
            Assert.True(layer.ReportedVolume < 1f);

            // Fade in on the first loop only.
            layer.FadeInFirstLoopOnly = true;
            // Wait for loop. It should not take effect.
            Task.Delay(5000).Wait();
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.Equal(1f, layer.ReportedVolume);

            // Reset
            layer.StopPlayingAll();
            layer.Play(playingFiles[0]).Wait();
            Task.Delay(100).Wait();
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.True(layer.ReportedVolume < 1f);

            // Wait for a loop.
            Task.Delay(5000).Wait();
            // Should not take effect now.
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.Equal(1f, layer.ReportedVolume);

            // Reset. Queue two, should fade in on first only.
            layer.StopPlayingAll();
            layer.Play(playingFiles[0]).Wait();
            layer.QueuePlay(playingFiles[0]).Wait();
            WaitForSoundLoops(1);
            Assert.True(layer.PlaybackLocation - 1f < 1f);
            Assert.True(layer.ReportedVolume < 1f);

            // Wait for first to be over.
            Task.Delay(5000).Wait();
            Assert.Equal(1f, layer.ReportedVolume);

            SoundTestCleanup(layer, playingFiles);
        }

        /// <summary>
        /// Test fading out.
        /// </summary>
        [Fact]
        public void FadeOut()
        {
            SoundTestStart(out SoundFile[] playingFiles, out SoundLayer layer, "Sounds/money.wav");
            layer.FadeOutLength = 2;

            //// Should start at 1f.
            //Assert.Equal(1f, layer.ReportedVolume);

            //// Should fade out.
            //Task.Delay(3100).Wait();
            //float vol = layer.ReportedVolume;
            //Assert.True(vol < 1f);

            //// Should be at full.
            //Task.Delay(1000).Wait();
            //Assert.True(layer.ReportedVolume < vol);

            //// Loop layer.
            //layer.Looping = true;
            //// Wait for sound to loop.
            //Task.Delay(2000).Wait();
            //Assert.True(layer.PlaybackLocation - 1f < 1f);
            //Assert.Equal(1f, layer.ReportedVolume);
            //// Should fade out again.
            //Task.Delay(3000).Wait();
            //Assert.True(layer.PlaybackLocation - 4f < 1f);
            //Assert.True(layer.ReportedVolume < vol);

            //// Reset. Queue two, should fade out on second only.
            //layer.StopPlayingAll();
            //layer.Play(playingFiles[0]).Wait();
            //layer.QueuePlay(playingFiles[0]).Wait();
            //Task.Delay(100).Wait();
            //Assert.True(layer.PlaybackLocation - 1f < 1f);
            //Assert.Equal(1f, layer.ReportedVolume);
            //Task.Delay(3000).Wait();
            //Assert.Equal(1f, layer.ReportedVolume);
            //// Wait for second one.
            //Task.Delay(5000).Wait();
            //Assert.True(layer.ReportedVolume < 1f);

            SoundFile newFile = Engine.AssetLoader.Get<SoundFile>("Sounds/sadMeme.wav");

            // Reset. Setup fade out on change.
            layer.StopPlayingAll().Wait();
            layer.FadeOutOnChange = true;
            layer.Play(playingFiles[0]).Wait();
            // Should now be playing.
            WaitForSoundLoops(1);
            Assert.Equal(SoundStatus.Playing, layer.Status);
            Assert.Equal(1, layer.ReportedVolume);
            // Now play another file.
            layer.Play(newFile);
            // Should still be playing first, but be fading out.
            WaitForSoundLoops(1);
            Assert.Equal(SoundStatus.Playing, layer.Status);
            Assert.Equal(playingFiles[0], layer.CurrentlyPlayingFile);
            Assert.True(layer.ReportedVolume < 1f);
            Task.Delay(2300).Wait();
            WaitForSoundLoops(1);
            Assert.Equal(newFile, layer.CurrentlyPlayingFile);
            // Stop playing.
            layer.StopPlayingAll();
            WaitForSoundLoops(1);
            Assert.Equal(SoundStatus.Playing, layer.Status);
            Assert.True(layer.ReportedVolume < 1f);
            Task.Delay(2300).Wait();
            WaitForSoundLoops(1);
            Assert.Equal(SoundStatus.Stopped, layer.Status);

            // Play again. Try to queue something. This shouldn't cause a fade out.
            layer.Play(newFile);
            WaitForSoundLoops(2);
            Assert.Equal(SoundStatus.Playing, layer.Status);
            Assert.Equal(1f, layer.ReportedVolume);
            layer.QueuePlay(playingFiles[0]);
            Task.Delay(1000).Wait();
            Assert.Equal(1f, layer.ReportedVolume);
            Assert.Equal(newFile, layer.CurrentlyPlayingFile);
            Task.Delay(TimeSpan.FromSeconds(newFile.Duration - 1f)).Wait();
            Assert.Equal(1f, layer.ReportedVolume);
            Task.Delay(100).Wait();
            Assert.Equal(1f, layer.ReportedVolume);
            Assert.Equal(playingFiles[0], layer.CurrentlyPlayingFile);
            layer.SkipNaturalFadeOut = true;
            Task.Delay(TimeSpan.FromSeconds(playingFiles[0].Duration - 1.5f)).Wait();
            // Shouldn't be fading out as skip is true.
            Assert.Equal(1f, layer.ReportedVolume);

            layer.StopPlayingAll(true).Wait();
            Engine.AssetLoader.Destroy(newFile.Name);
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

            WaitForSoundLoops(1);

            // Ensure everything loaded correctly.
            Assert.Equal(playingFiles.Sum(x => x.Duration), layer.TotalDuration);
            Assert.Equal(playingFiles.Length, layer.PlayList.Count);
            for (int i = 0; i < playingFiles.Length; i++)
            {
                Assert.Equal(playingFiles[i], layer.PlayList[i]);
            }

            Assert.Equal(SoundStatus.Playing, layer.Status);
            Assert.Equal(playingFiles[0], layer.CurrentlyPlayingFile);
            Assert.Equal(0, layer.CurrentlyPlayingFileIndex);
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
            TestScene extScene = new TestScene
            {
                // This will test loading testing in another thread.
                ExtLoad = () =>
                {
                    foreach (string trackName in filesToPlay)
                    {
                        SoundFile track = Engine.AssetLoader.Get<SoundFile>(trackName);
                        if (filesToPlay.Length == 1) Engine.SoundManager.Play(track, "testLayer");
                        else
                            Engine.SoundManager.QueuePlay(track, "testLayer");
                        files.Add(track);
                    }
                }
            };

            // Load scene.
            Helpers.LoadScene(extScene);

            // Set instant sound thread.
            Engine.Flags.SoundThreadFrequency = 1;

            // Ensure layer is proper and get it.
            Assert.Single(Engine.SoundManager.Layers);
            layer = Engine.SoundManager.GetLayer("testLayer");
            Assert.Equal("testLayer", layer.Name);

            // Wait for two loops.
            WaitForSoundLoops(2);

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
                Engine.AssetLoader.Destroy(file.Name);
            }

            // Wait for cleanup. Only one file will be cleaned up per loop.
            WaitForSoundLoops(playingFiles.Length);

            // Should be cleaned up.
            foreach (SoundFile file in playingFiles)
            {
                Assert.Equal((uint) 0, file.ALBuffer);
            }

            // Remove the layer.
            Engine.SoundManager.RemoveLayer("testLayer");
            Assert.Empty(Engine.SoundManager.Layers);

            // Cleanup.
            Helpers.UnloadScene();

            // Restore sound thread frequency.
            Engine.Flags.SoundThreadFrequency = 200;
        }

        /// <summary>
        /// Wait for the sound manager to loop.
        /// </summary>
        /// <param name="loopCount">How many times it should loop.</param>
        private void WaitForSoundLoops(int loopCount)
        {
            for (int i = 0; i < loopCount + 1; i++)
            {
                Engine.SoundManager.GetOneLoopToken().Wait();
            }
        }

        #endregion
    }
}