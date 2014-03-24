using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Agent.Core;
using Agent.Core.Data.Model;
using Agent.Core.Utils;
using Agent.RV.Utils;


namespace Agent.RV
{
    internal static class Downloader
    {
        public static Operations.SavedOpData DownloadFile(Operations.SavedOpData update, UpdateDirectories updateTypePath)
        {
            var uris = new List<DownloadUri>();
            string updateDirectory;

            switch (updateTypePath)
            {
                case UpdateDirectories.SupportedAppDir:
                    updateDirectory = Settings.SupportedAppDirectory;
                    break;
                case UpdateDirectories.CustomAppDir:
                    updateDirectory = Settings.CustomAppDirectory;
                    break;
                case UpdateDirectories.OSUpdateDir:
                    updateDirectory = Settings.UpdateDirectory;
                    break;
                default:
                    updateDirectory = Settings.UpdateDirectory;
                    break;
            }

            var savedir = Path.Combine(updateDirectory, update.filedata_app_id);

            foreach (var uriData in update.filedata_app_uris)
            {
                var tempDownloadUri = new DownloadUri();
                tempDownloadUri.FileName = uriData.file_name;
                tempDownloadUri.FileSize = uriData.file_size;
                tempDownloadUri.Hash = uriData.file_hash;

                //add uri link from WSUS if its enabled
                if (WSUS.IsWSUSEnabled())
                {
                    var tempAvilApp = Agent.RV.WindowsApps.WindowsUpdates.GetAvailableUpdates();
                    foreach (var application in tempAvilApp)
                    {
                        if (application.Name == update.filedata_app_name)
                        {
                            foreach (var availUri in application.FileData)
                            {
                                tempDownloadUri.Uris.Add(availUri.Uri);
                            }
                        }
                    }
                }

                foreach (var uri in uriData.file_uris)
                    tempDownloadUri.Uris.Add(uri);
                
                uris.Add(tempDownloadUri);
            }

            if (Directory.Exists(savedir))
                Directory.Delete(savedir, true);
            Directory.CreateDirectory(savedir);

            foreach (var file in uris)
            {
                // Just in case the web server is using a self-signed cert. 
                // Webclient won't validate the SSL/TLS cerficate if it's not trusted.
                var tempCallback = ServicePointManager.ServerCertificateValidationCallback;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

                var filepath = Path.Combine(savedir, file.FileName);

                try
                {
                    using (var client = new WebClient())
                    {
                        if (Settings.Proxy != null)
                            client.Proxy = Settings.Proxy;

                        var downloaded = false;
                        foreach (var uriSingle in file.Uris)
                        {
                            try
                            {
                                var splitted = uriSingle.Split(new[] { "//" }, StringSplitOptions.RemoveEmptyEntries);
                                var splitted2 = splitted[1].Split(new[] { '/' });
                                var relayserver = splitted2[0];

                                if (downloaded) break;

                                Logger.Log("Attempting to download {1} from {0} with file size of {2}.", LogLevel.Info, relayserver, file.FileName, file.FileSize);
                                client.DownloadFile(uriSingle, filepath);

                                if (File.Exists(filepath))
                                {
                                    var localFileHashMd5 = RvUtils.Md5HashFile(filepath).ToLower();
                                    var localFileHashSha1 = RvUtils.Sha1HashFile(filepath).ToLower();
                                    var localFileHashSha256 = RvUtils.Sha256HashFile(filepath).ToLower();

                                    Logger.Log("Download Complete,  {0}", LogLevel.Info, file.FileName);
                                    Logger.Log("Checking Hashes...");
                                    Logger.Log("Incoming Hash: {0}", LogLevel.Info, file.Hash);
                                    Logger.Log("Local MD5 Hash: {0}", LogLevel.Info, localFileHashMd5);
                                    Logger.Log("Local SHA1 Hash: {0}", LogLevel.Info, localFileHashSha1);
                                    Logger.Log("Local SHA256 Hash: {0}", LogLevel.Info, localFileHashSha256);
                                    downloaded = true;

                                    if (localFileHashMd5 != file.Hash.ToLower() && localFileHashSha1 != file.Hash.ToLower() && localFileHashSha256 != file.Hash.ToLower())
                                    {
                                        Logger.Log("Local file {0} Hash did not match remote's. Retrying with a different server.", LogLevel.Info, file.FileName);
                                        update.error = "Local file Hash did not match remote. Bad file integrity. ";
                                        update.success = false.ToString().ToLower();
                                        downloaded = false;
                                    }
                                }
                                else
                                {
                                    Logger.Log("File {0} did not download. Retrying with a different server.", LogLevel.Info, file.FileName);
                                    update.error = "File did not download successfully, it was not found on disk. Please check download server.";
                                    update.success = false.ToString().ToLower();
                                    downloaded = false;
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Log("File {0} failed to download correctly... Possible connection issue, Retrying with a different server.", LogLevel.Info, file.FileName);
                                update.error = "File did not download correctly, Exception message: " + e.Message + ". Please check download server connectivity.";
                                update.success = false.ToString().ToLower();
                                downloaded = false;
                            }
                        }

                        //Check if the file was successfully downloaded and return.
                        if (downloaded)
                        {
                            update.error = String.Empty;
                            update.success = true.ToString().ToLower();
                        }
                    }
                }
                catch (Exception e)
                {
                    //Critical exception occurred.
                    update.error = "Application did not download, Exception occured, refer to log for details.";
                    update.success = false.ToString().ToLower();
                    Logger.Log("One or more Application Files were not downloaded successfully; {0}.",
                    LogLevel.Error, file.FileName);
                    Logger.LogException(e);
                    return update;
                }

                ServicePointManager.ServerCertificateValidationCallback = tempCallback;
            }

            return update;
        }

        
        public enum UpdateDirectories
        {
            SupportedAppDir,
            CustomAppDir,
            OSUpdateDir
        }
    }

    public class ThrottledStream : Stream
    {
        /// <summary>
        /// A constant used to specify an infinite number of bytes that can be transferred per second.
        /// </summary>
        public const long Infinite = 0;

        #region Private members
        /// <summary>
        /// The base stream.
        /// </summary>
        private Stream _baseStream;

        /// <summary>
        /// The maximum bytes per second that can be transferred through the base stream.
        /// </summary>
        private long _maximumBytesPerSecond;

        /// <summary>
        /// The number of bytes that has been transferred since the last throttle.
        /// </summary>
        private long _byteCount;

        /// <summary>
        /// The start time in milliseconds of the last throttle.
        /// </summary>
        private long _start;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the current milliseconds.
        /// </summary>
        /// <value>The current milliseconds.</value>
        protected long CurrentMilliseconds
        {
            get
            {
                return Environment.TickCount;
            }
        }

        /// <summary>
        /// Gets or sets the maximum bytes per second that can be transferred through the base stream.
        /// </summary>
        /// <value>The maximum bytes per second.</value>
        public long MaximumBytesPerSecond
        {
            get
            {
                return _maximumBytesPerSecond;
            }
            set
            {
                if (MaximumBytesPerSecond != value)
                {
                    _maximumBytesPerSecond = value;
                    Reset();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get
            {
                return _baseStream.CanRead;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get
            {
                return _baseStream.CanSeek;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <value></value>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite
        {
            get
            {
                return _baseStream.CanWrite;
            }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <value></value>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="T:System.NotSupportedException">The base stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Length
        {
            get
            {
                return _baseStream.Length;
            }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        /// <value></value>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The base stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Position
        {
            get
            {
                return _baseStream.Position;
            }
            set
            {
                _baseStream.Position = value;
            }
        }
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ThrottledStream"/> class with an
        /// infinite amount of bytes that can be processed.
        /// </summary>
        /// <param name="baseStream">The base stream.</param>
        public ThrottledStream(Stream baseStream)
            : this(baseStream, ThrottledStream.Infinite)
        {
            // Nothing todo.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ThrottledStream"/> class.
        /// </summary>
        /// <param name="baseStream">The base stream.</param>
        /// <param name="maximumBytesPerSecond">The maximum bytes per second that can be transferred through the base stream.</param>
        /// <exception cref="ArgumentNullException">Thrown when <see cref="baseStream"/> is a null reference.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="maximumBytesPerSecond"/> is a negative value.</exception>
        public ThrottledStream(Stream baseStream, long maximumBytesPerSecond)
        {
            if (baseStream == null)
            {
                throw new ArgumentNullException("baseStream");
            }

            if (maximumBytesPerSecond < 0)
            {
                throw new ArgumentOutOfRangeException("maximumBytesPerSecond",
                    maximumBytesPerSecond, "The maximum number of bytes per second can't be negatie.");
            }

            _baseStream = baseStream;
            _maximumBytesPerSecond = maximumBytesPerSecond;
            _start = CurrentMilliseconds;
            _byteCount = 0;
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
        public override void Flush()
        {
            _baseStream.Flush();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between offset and (offset + count - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The sum of offset and count is larger than the buffer length. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="T:System.NotSupportedException">The base stream does not support reading. </exception>
        /// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Throttle(count);

            return _baseStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"></see> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The base stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="T:System.NotSupportedException">The base stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The base stream does not support writing. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <exception cref="T:System.ArgumentNullException">buffer is null. </exception>
        /// <exception cref="T:System.ArgumentException">The sum of offset and count is greater than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">offset or count is negative. </exception>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Throttle(count);

            _baseStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return _baseStream.ToString();
        }
        #endregion

        #region Protected methods
        /// <summary>
        /// Throttles for the specified buffer size in bytes.
        /// </summary>
        /// <param name="bufferSizeInBytes">The buffer size in bytes.</param>
        protected void Throttle(int bufferSizeInBytes)
        {
            // Make sure the buffer isn't empty.
            if (_maximumBytesPerSecond <= 0 || bufferSizeInBytes <= 0)
            {
                return;
            }

            _byteCount += bufferSizeInBytes;
            long elapsedMilliseconds = CurrentMilliseconds - _start;

            if (elapsedMilliseconds > 0)
            {
                // Calculate the current bps.
                long bps = _byteCount * 1000L / elapsedMilliseconds;

                // If the bps are more then the maximum bps, try to throttle.
                if (bps > _maximumBytesPerSecond)
                {
                    // Calculate the time to sleep.
                    long wakeElapsed = _byteCount * 1000L / _maximumBytesPerSecond;
                    int toSleep = (int)(wakeElapsed - elapsedMilliseconds);

                    if (toSleep > 1)
                    {
                        try
                        {
                            // The time to sleep is more then a millisecond, so sleep.
                            Thread.Sleep(toSleep);
                        }
                        catch (ThreadAbortException)
                        {
                            // Eatup ThreadAbortException.
                        }

                        // A sleep has been done, reset.
                        Reset();
                    }
                }
            }
        }

        /// <summary>
        /// Will reset the bytecount to 0 and reset the start time to the current time.
        /// </summary>
        protected void Reset()
        {
            long difference = CurrentMilliseconds - _start;

            // Only reset counters when a known history is available of more then 1 second.
            if (difference > 1000)
            {
                _byteCount = 0;
                _start = CurrentMilliseconds;
            }
        }
        #endregion
    }


}
