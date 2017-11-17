/*
 * Copyright (C) 2016-2017. Filament Games, LLC. All rights reserved.
 * Author:  Alex Beauchesne
 * Date:    8 Oct 2017
 * 
 * File:    FutureShortcuts.cs
 * Purpose: Shortcut methods for creating and dealing with Futures.
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
                Routine.Start(inRoutineHost, DownloadWWW(future, inWWW));
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
                    Routine.Start(inRoutineHost, DownloadWWW(future, www));
                }
                catch(Exception e)
                {
                    future.Fail(e);
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
                        inFuture.Fail(inWWW.error);
                    }
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
                Routine.Start(inRoutineHost, DownloadStringRoutine(future, inWWW));
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
                    Routine.Start(inRoutineHost, DownloadStringRoutine(future, www));
                }
                catch(Exception e)
                {
                    future.Fail(e);
                }
                return future;
            }

            static private IEnumerator DownloadStringRoutine(Future<string> inFuture, WWW inWWW)
            {
                yield return inWWW;
                if (string.IsNullOrEmpty(inWWW.error))
                {
                    try
                    {
                        inFuture.Complete(inWWW.text);
                    }
                    catch(Exception e)
                    {
                        inFuture.Fail(e);
                    }
                }
                else
                {
                    inFuture.Fail(inWWW.error);
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
                Routine.Start(inRoutineHost, DownloadBytesRoutine(future, inWWW));
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
                    Routine.Start(inRoutineHost, DownloadBytesRoutine(future, www));
                }
                catch(Exception e)
                {
                    future.Fail(e);
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
                            inFuture.Fail(e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(inWWW.error);
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
                Routine.Start(inRoutineHost, DownloadTextureRoutine(future, inWWW, inbDownloadAsNonReadable));
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
                    Routine.Start(inRoutineHost, DownloadTextureRoutine(future, www, inbDownloadAsNonReadable));
                }
                catch(Exception e)
                {
                    future.Fail(e);
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
                            Texture2D texture = inbNonReadable ? inWWW.textureNonReadable : inWWW.texture;
                            if (texture == null)
                                inFuture.Fail("Texture was null");
                            else if (!TextureIsValid(texture))
                                inFuture.Fail("Texture was invalid.");
                            else
                                inFuture.Complete(texture);
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(inWWW.error);
                    }
                    yield return null;
                }
            }

            static private bool TextureIsValid(Texture2D inTexture)
            {
                // TODO(Alex): Replace this with something better?
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
                Routine.Start(inRoutineHost, DownloadAudioClipRoutine(future, inWWW, inbCompressed));
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
                    Routine.Start(inRoutineHost, DownloadAudioClipRoutine(future, www, inbCompressed));
                }
                catch(Exception e)
                {
                    future.Fail(e);
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
                            AudioClip audioClip = inbCompressed ? inWWW.GetAudioClipCompressed() : inWWW.GetAudioClip();
                            if (audioClip == null)
                                inFuture.Fail("Clip is null");
                            else
                                inFuture.Complete(audioClip);
                        }
                        catch(Exception e)
                        {
                            inFuture.Fail(e);
                        }
                    }
                    else
                    {
                        inFuture.Fail(inWWW.error);
                    }
                    yield return null;
                }
            }

            #endregion
        }

        /// <summary>
        /// Asynchronous resource loading methods. These will create Routines to load resources
        /// asynchronously and return a Future for the result.
        /// </summary>
        static public class Resources
        {
            /// <summary>
            /// Loads an asset from the Resources folder asynchronously
            /// and returns a Future for the loaded asset.
            /// </summary>
            static public Future<T> LoadAsync<T>(string inPath, MonoBehaviour inRoutineHost = null) where T : UnityEngine.Object
            {
                var future = Future.Create<T>();
                Routine.Start(inRoutineHost, DownloadResource<T>(future, inPath));
                return future;
            }

            static private IEnumerator DownloadResource<T>(Future<T> inFuture, string inPath) where T : UnityEngine.Object
            {
                var resourceRequest = UnityEngine.Resources.LoadAsync<T>(inPath);
                yield return resourceRequest;
                if (resourceRequest.asset == null)
                    inFuture.Fail("No resource found");
                else
                    inFuture.Complete((T)resourceRequest.asset);
            }
        }
    }
}