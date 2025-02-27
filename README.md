# Live-Streaming-Server-Net

This open-source project provides an implementation of an RTMP live streaming server which allows you to build your own live streaming server using .NET.

## Features

- **RTMP/RTMPS protocol**: Supports the RTMP and RTMPS protocols for streaming audio, video, and data.
- **HTTP-FLV/WebSocket-FLV with ASP.NET CORE**: Provides support for serving FLV live streams using HTTP-FLV and WebSocket-FLV protocols within an ASP.NET Core application.
- **Remuxing RTMP streams into HLS/DASH streams**: Allows you to remux RTMP streams into HLS (HTTP Live Streaming) or DASH (Dynamic Adaptive Streaming over HTTP) streams.
- **GOP caching**: Supports caching the Group of Pictures (GOP) to ensure immediate availability of live streaming content.
- **Custom authorization**: Enables you to implement custom authorization mechanisms for accessing live streams.
- **Admin panel**: Includes an admin panel that provides an user interface for managing and monitoring the live streaming server.

## Roadmap

- Native AOT support
- Kubernetes operator

## Quick Start

### Run the RTMP Server

Create a .NET 8 console application project and add the dependencies

```
dotnet new console
dotnet add package LiveStreamingServerNet
dotnet add package Microsoft.Extensions.Logging.Console
```

Program.cs

```
using LiveStreamingServerNet;
using Microsoft.Extensions.Logging;
using System.Net;

await using var server = LiveStreamingServerBuilder.Create()
    .ConfigureLogging(options => options.AddConsole())
    .Build();

await server.RunAsync(new IPEndPoint(IPAddress.Any, 1935));
```

Run the application

```
dotnet run
```

### Publish a Live Stream

#### With FFmpeg

Use the following command to publish a video as the live stream using FFmpeg

```
ffmpeg -re -i <input_file> -c:v libx264 -c:a aac -f flv rtmp://localhost:1935/live/demo
```

#### With OBS Studio

1. Open OBS Studio and go to "Settings".
2. In the "Settings" window, select the "Stream" tab.
3. Choose "Custom" as the "Service".
4. Enter "Server": `rtmp://localhost:1935/live` and "Stream Key": `demo`.
5. Click "OK" to save the settings.
6. Click the "Start Streaming" button in OBS Studio to begin sending live stream to the RTMP server.

### Play the Live Stream

#### With FFplay

Use the following command to play the live stream using FFplay

```
ffplay rtmp://localhost:1935/live/demo
```

#### With VLC Media Player

1. Open VLC Media Player.
2. Go to the "Media" menu and select "Open Network Stream".
3. In the "Network" tab, enter the URL: `rtmp://localhost:1935/live/demo`.
4. Click the "Play" button to start playing the live stream.

---

### Serve FLV Live Streams

Create a ASP.NET CORE 8 Web API application project and add the dependencies

```
dotnet new webapi
dotnet add package LiveStreamingServerNet
dotnet add package LiveStreamingServerNet.Flv
```

Program.cs

```
using System.Net;
using LiveStreamingServerNet;
using LiveStreamingServerNet.Flv.Installer;
using LiveStreamingServerNet.Networking.Helpers;

var builder = WebApplication.CreateBuilder(args);

await using var liveStreamingServer = LiveStreamingServerBuilder.Create()
    .ConfigureRtmpServer(options => options.AddFlv())
    .ConfigureLogging(options => options.AddConsole())
    .Build();

builder.Services.AddBackgroundServer(liveStreamingServer, new IPEndPoint(IPAddress.Any, 1935));

var app = builder.Build();

app.UseWebSockets();
app.UseWebSocketFlv(liveStreamingServer);

app.UseHttpFlv(liveStreamingServer);

await app.RunAsync();
```

Run the application

```
dotnet run --urls="https://+:8080"
```

#### Play FLV Live Streams

Given a live stream is published to `rtmp://localhost:1935/live/demo`

**HTTP-FLV**

```
https://localhost:8080/live/demo.flv
```

**WebSocket-FLV**

```
wss://localhost:8080/live/demo.flv
```

#### Remux RTMP Streams into HLS Streams with FFmpeg

Please refer to the [LiveStreamServerNet.HlsDemo](https://github.com/josephnhtam/live-streaming-server-net/tree/master/samples/LiveStreamingServerNet.HlsDemo)

## Admin Panel

![Admin Panel](images/admin-panel.jpeg)

![HTTP-FLV Preview](images/http-flv-preview.jpeg)

Please refer to the [LiveStreamServerNet.StandaloneDemo](https://github.com/josephnhtam/live-streaming-server-net/tree/master/samples/LiveStreamingServerNet.StandaloneDemo)

## NuGet Packages

<table>
  <thead>
    <tr>
      <th>Package</th>
      <th>Latest Version</th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <th>LiveStreamingServerNet</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.AdminPanelUI</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.AdminPanelUI"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.AdminPanelUI.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Flv</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Flv"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Flv.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Networking</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Networking"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Networking.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Rtmp</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Rtmp"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Rtmp.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Transmuxer</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Transmuxer"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Transmuxer.svg?logo=nuget"></a></td>
    </tr>
    <tr>
      <th>LiveStreamingServerNet.Utilities</th>
      <td><a href="https://www.nuget.org/packages/LiveStreamingServerNet.Utilities"><img src="https://img.shields.io/nuget/v/LiveStreamingServerNet.Utilities?logo=nuget"></a></td>
    </tr>
  </tbody>
</table>
