// Copyright (C) 2016 by Barend Erasmus, David Jeske and donated to the public domain

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using SimpleHttpServer;
using SimpleHttpServer.Models;
using SimpleHttpServer.RouteHandlers;

namespace SimpleHttpServer.App
{
    class Program
    {
        static string appLocation;

        public static HttpResponse ComposeResponseLog(HttpRequest request)
        {
            var frontPath = request.Path.Split('?')[0];
            var filename = Path.GetFileName(frontPath);
            var filePath = Path.Combine(appLocation, "logs", filename);
            var content = "## Conflict Accessing file "+ filePath;
            try
            {
                content = File.ReadAllText(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Read file [{0}] Error: {1}", filePath, e.ToString());
            }
            return new HttpResponse()
            {
                ContentAsUTF8 = content,
                ReasonPhrase = "OK",
                StatusCode = "200"
            };
        }

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            appLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

            var route_config = new List<Models.Route>() {
                new Route {
                    Name = "Hello Handler",
                    UrlRegex = @"^/$",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = "Hello from SimpleHttpServer",
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "Text File Static Handler",
                    UrlRegex = @"^/logs/.*",
                    Method = "GET",
                    Callable = ComposeResponseLog
                },
                //new Route {   
                //    Name = "FileSystem Static Handler",
                //    UrlRegex = @"^/Static/(.*)$",
                //    Method = "GET",
                //    Callable = new FileSystemRouteHandler() { BasePath = @"C:\Tmp", ShowDirectories=true }.Handle,
                //},
            };

            HttpServer httpServer = new HttpServer(8080, route_config);
            
            Thread thread = new Thread(new ThreadStart(httpServer.Listen));
            thread.Start();
        }
    }
}
