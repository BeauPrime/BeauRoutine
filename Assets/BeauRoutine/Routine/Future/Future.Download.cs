/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 Oct 2017
 * 
 * File:    Future.Download.cs
 * Purpose: Shortcut methods for downloading to a Future.
*/

using System;
using System.Collections;
using UnityEngine;

namespace BeauRoutine
{
    /// <summary>
    /// Static methods for dealing with Futures.
    /// </summary>
    static public partial class Future
    {
        /// <summary>
        /// Download methods. These will create Routines to download data from a URL,
        /// and return a Future for the result of that download.
        /// </summary>
        static public class Download
        {
            #region WWW

            /// <summary>
            /// Downloads from the given WWW and returns a Future for that WWW.
            /// </summary>
            static public Future<WWW> WWW(WWW inWWW, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<WWW>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadWWW(future, inWWW))
                    );
                return future;
            }

            /// <summary>
            /// Downloads from the given URL and returns a Future for the resulting WWW.
            /// </summary>
            static public Future<WWW> WWW(string inURL, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<WWW>();
                try
                {
                    WWW www = new WWW(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadWWW(future, www))
                        );
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadWWW(Future<WWW> inFuture, WWW inWWW)
            {
                using (inWWW)
                {
                    yield return inWWW;
                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        inFuture.Complete(inWWW);
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }

                    // wait two frames to ensure the Future's callbacks have been invoked
                    yield return null;
                    yield return null;
                }
            }

            #endregion

            #region Text

            /// <summary>
            /// Downloads text from the given WWW and returns a Future for the text.
            /// </summary>
            static public Future<string> Text(WWW inWWW, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<string>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadStringRoutine(future, inWWW))
                    );
                return future;
            }

            /// <summary>
            /// Downloads text from the given URL and returns a Future for the text.
            /// </summary>
            static public Future<string> Text(string inURL, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<string>();
                try
                {
                    WWW www = new WWW(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadStringRoutine(future, www))
                        );
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadStringRoutine(Future<string> inFuture, WWW inWWW)
            {
                using (inWWW)
                {
                    yield return inWWW;
                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        try
                        {
                            bool bValidMIMEType;
                            if (TryValidateMIMEType(inWWW, "text/", out bValidMIMEType))
                            {
                                if (!bValidMIMEType)
                                {
                                    inFuture.Fail(Future.FailureType.Unknown, "Invalid MIME type");
                                    yield break;
                                }
                            }

                            inFuture.Complete(inWWW.text);
                        }
                        catch (Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }
                }
            }

            #endregion

            #region Bytes

            /// <summary>
            /// Downloads bytes from the given WWW and returns a Future for the bytes.
            /// </summary>
            static public Future<byte[]> Bytes(WWW inWWW, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<byte[]>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadBytesRoutine(future, inWWW))
                    );
                return future;
            }

            /// <summary>
            /// Downloads bytes from the given URL and returns a Future for the bytes.
            /// </summary>
            static public Future<byte[]> Bytes(string inURL, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<byte[]>();
                try
                {
                    WWW www = new WWW(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadBytesRoutine(future, www))
                    );
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadBytesRoutine(Future<byte[]> inFuture, WWW inWWW)
            {
                using (inWWW)
                {
                    yield return inWWW;
                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        try
                        {
                            inFuture.Complete(inWWW.bytes);
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }
                    yield return null;
                }
            }

            #endregion

            #region Download Texture

            /// <summary>
            /// Downloads a texture from the given WWW and returns a Future for the texture.
            /// </summary>
            static public Future<Texture2D> Texture(WWW inWWW, bool inbDownloadAsNonReadable = false, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<Texture2D>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadTextureRoutine(future, inWWW, inbDownloadAsNonReadable))
                    );
                return future;
            }

            /// <summary>
            /// Downloads a texture from the given URL and returns a Future for the texture.
            /// </summary>
            static public Future<Texture2D> Texture(string inURL, bool inbDownloadAsNonReadable = false, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<Texture2D>();
                try
                {
                    WWW www = new WWW(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadTextureRoutine(future, www, inbDownloadAsNonReadable))
                        );
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadTextureRoutine(Future<Texture2D> inFuture, WWW inWWW, bool inbNonReadable)
            {
                using (inWWW)
                {
                    yield return inWWW;
                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        try
                        {
                            bool bValidMIMEType;
                            if (TryValidateMIMEType(inWWW, "image/", out bValidMIMEType))
                            {
                                if (!bValidMIMEType)
                                {
                                    inFuture.Fail(Future.FailureType.Unknown, "Invalid MIME type");
                                    yield break;
                                }
                            }

                            Texture2D texture = inbNonReadable ? inWWW.textureNonReadable : inWWW.texture;
                            if (texture == null)
                                inFuture.Fail(Future.FailureType.Unknown, "Texture is null");
                            else if (!TextureIsValid(texture))
                                inFuture.Fail(Future.FailureType.Unknown, "Not a valid texture");
                            else
                            {
                                texture.name = UnityEngine.WWW.UnEscapeURL(inWWW.url);
                                inFuture.Complete(texture);
                            }
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }
                    yield return null;
                }
            }

            static private bool TextureIsValid(Texture2D inTexture)
            {
                // TODO(Alex): Replace this with something better?
                // The default texture returned by Unity is 8x8,
                // but if we try downloading an 8x8 texture (no matter how unlikely),
                // this will fail
                return inTexture.width != 8 || inTexture.height != 8;
            }

            #endregion

            #region AudioClip

            /// <summary>
            /// Downloads an AudioClip from the given WWW and returns a Future for the texture.
            /// </summary>
            static public Future<AudioClip> AudioClip(WWW inWWW, bool inbCompressed = false, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<AudioClip>();
                future.LinkTo(
                    Routine.Start(inRoutineHost, DownloadAudioClipRoutine(future, inWWW, inbCompressed))
                    );
                return future;
            }

            /// <summary>
            /// Downloads an AudioClip from the given URL and returns a Future for the texture.
            /// </summary>
            static public Future<AudioClip> AudioClip(string inURL, bool inbCompressed = false, MonoBehaviour inRoutineHost = null)
            {
                var future = Future.Create<AudioClip>();
                try
                {
                    WWW www = new WWW(inURL);
                    future.LinkTo(
                        Routine.Start(inRoutineHost, DownloadAudioClipRoutine(future, www, inbCompressed))
                        );
                }
                catch(Exception e)
                {
                    future.Fail(Future.FailureType.Exception, e);
                }
                return future;
            }

            static private IEnumerator DownloadAudioClipRoutine(Future<AudioClip> inFuture, WWW inWWW, bool inbCompressed)
            {
                using (inWWW)
                {
                    yield return inWWW;
                    if (string.IsNullOrEmpty(inWWW.error))
                    {
                        try
                        {
                            bool bValidMIMEType;
                            if (TryValidateMIMEType(inWWW, "audio/", out bValidMIMEType))
                            {
                                if (!bValidMIMEType)
                                {
                                    inFuture.Fail(Future.FailureType.Unknown, "Invalid MIME type");
                                    yield break;
                                }
                            }

                            AudioClip audioClip = inbCompressed ? inWWW.GetAudioClipCompressed(false) : inWWW.GetAudioClip(false);
                            if (audioClip == null)
                                inFuture.Fail(Future.FailureType.Unknown, "Clip is null");
                            else
                            {
                                audioClip.name = UnityEngine.WWW.UnEscapeURL(inWWW.url);
                                inFuture.Complete(audioClip);
                            }
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(Future.FailureType.Exception, e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(Future.FailureType.Unknown, inWWW.error);
                    }
                    yield return null;
                }
            }

            #endregion

            // Returns whether it could validate the MIME type of the returned content.
            static private bool TryValidateMIMEType(WWW inWWW, string inMIMEType, out bool outbValidated)
            {
                outbValidated = true;
                
                try
                {
                    string contentType = string.Empty;
                    bool bFoundContentType = inWWW.responseHeaders != null && inWWW.responseHeaders.TryGetValue("Content-Type", out contentType);
                    if (bFoundContentType)
                    {
                        if (!contentType.StartsWith(inMIMEType))
                        {
                            outbValidated = false;
                        }

                        return true;
                    }
                }
                catch (Exception) { }

                return false;
            }
        }
    }
}