﻿using Microsoft.AspNetCore.Http;

namespace LiveStreamingServerNet.Flv.Contracts
{
    public interface IFlvHeaderWriter
    {
        Task WriteHeaderAsync(HttpContext httpContext, string streamPath, IDictionary<string, string> streamArguments, CancellationToken cancellation);
    }
}
