using System.IO;
using System.Net;
using BepInEx;

namespace jshepler.ngu.mods.WebService
{
    internal class TotalTimePlayed
    {
        internal static void HandleRequest(HttpListenerContext context)
        {
            var totalseconds = Plugin.Character.totalPlaytime.totalseconds;
            var path = $"{Paths.ConfigPath}\\TotalTimePlayed.htm";

            if (!File.Exists(path))
                File.WriteAllText(path, _defaultTimeHtml);

            var html = File.ReadAllText(path).Replace("%totalSeconds%", totalseconds.ToString());
            context.Response.SendResponse(HttpStatusCode.OK, html, "text/html");
        }

        private static string _defaultTimeHtml = @"
<html>
    <head>
        <style>
            body {
                background-color: #000;
                color: #fff;
                font-size: 32px;
                font-weight: bold;
            }
        </style>
    </head>
    <body>
        <div id='time'></div>
        <script type='text/javascript'>
            let totalTimePlayed = %totalSeconds%;
            let el = document.getElementById(""time"");

            let start = document.timeline.currentTime;
            requestAnimationFrame(onFrame);

            function onFrame(ts)
            {
                displayTime(totalTimePlayed + (ts - start) / 1000);
                requestAnimationFrame(onFrame);
            }

            function displayTime(totalSeconds)
            {
                let days = Math.floor(totalSeconds / 86400);
                totalSeconds -= days * 86400;
    
                let hours = Math.floor(totalSeconds / 3600);
                totalSeconds -= hours * 3600;
    
                let minutes = Math.floor(totalSeconds / 60);
                totalSeconds -= minutes * 60;
    
                let seconds = Math.floor(totalSeconds);
                totalSeconds -= seconds;
                
                let ms = Math.floor(totalSeconds * 10);

                let text = days + "" days "" + (""0"" + hours).slice(-2) + "":"" + (""0"" + minutes).slice(-2) + "":"" + (""0"" + seconds).slice(-2) + ""."" + ms
                el.innerText = text;
            }
        </script>
    </body>
</html>
";
    }
}
