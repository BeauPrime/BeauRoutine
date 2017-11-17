using UnityEngine;
using BeauRoutine;
using System.Collections;
using System;

// This class is a service that performs operations and returns values asynchronously.
// This kind of pattern can be useful if you have a service that multiple objects
// must interface with.
public class Example6_Service : MonoBehaviour
{
    // This function will create a future and run a routine
    // that completes that promise asynchronously.
    public Future<string> ReverseString(string inString)
    {
        var future = Future.Create<string>();
        Routine.Start(this, ReverseStringRoutine(future, inString));
        return future;
    }

    // Note that a reference to the Future must always be present somewhere.
    private IEnumerator ReverseStringRoutine(Future<string> inFuture, string inString)
    {
        char[] chars = inString.ToCharArray();
        for (int i = 0; i < chars.Length / 2; ++i)
        {
            char temp = chars[i];
            chars[i] = chars[chars.Length - 1 - i];
            chars[chars.Length - 1 - i] = temp;
            
            // Simulate doing some work with this delay
            yield return 0.2f;
        }

        inFuture.Complete(new string(chars));
    }

    public Future<Color> ParseColor(string inString)
    {
        var future = Future.Create<Color>();
        Routine.Start(this, ParseColorRoutine(future, inString));
        return future;
    }

    private IEnumerator ParseColorRoutine(Future<Color> inFuture, string inString)
    {
        // Simulate doing some work with a delay
        yield return 1;

        Color color;
        bool bSuccess = ColorUtility.TryParseHtmlString(inString, out color);
        if (!bSuccess)
        {
            inFuture.Fail();
        }
        else
        {
            inFuture.Complete(color);
        }
    }

    public Future<string> ParseURL(string inURL)
    {
        // Future shortcuts exist in Future.Download
        return Future.Download.Text(inURL);
    }

    public Future<Texture2D> ParseURLTexture(string inURL)
    {
        return Future.Download.Texture(inURL);
    }
}
