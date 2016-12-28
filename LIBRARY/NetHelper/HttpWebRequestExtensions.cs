using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace TMP.Common.NetHelper
{
    public static class HttpWebRequestExtensions
    {
      public static Task<Stream> GetRequestStreamAsync(this HttpWebRequest request)
      {
          var tcs = new TaskCompletionSource<Stream>();
   
          try
          {
              request.BeginGetRequestStream(iar =>
              {
                  try
                  {
                      var response = request.EndGetRequestStream(iar);
                      tcs.SetResult(response);
                  }
                  catch (Exception exc)
                  {
                      tcs.SetException(exc);
                  }
              }, null);
          }
          catch (Exception exc)
          {
              tcs.SetException(exc);
          }
   
          return tcs.Task;
      }
   
      public static Task<HttpWebResponse> GetResponseAsync(this HttpWebRequest request)
      {
          var tcs = new TaskCompletionSource<HttpWebResponse>();
   
          try
          {
              request.BeginGetResponse(iar =>
              {
                  try
                  {
                      var response = (HttpWebResponse)request.EndGetResponse(iar);
                      tcs.SetResult(response);
                  }
                  catch (Exception exc)
                  {
                      tcs.SetException(exc);
                  }
              }, null);
          }
          catch (Exception exc)
          {
              tcs.SetException(exc);
          }
   
          return tcs.Task;
      }
    }
}
