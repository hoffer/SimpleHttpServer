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
        const string STATUS_ROUTE = "status";
        static string[] allowedType =
        {
            ".md",
            ".txt",
            ".log"
        };
        public static HttpResponse ComposeResponseLog(HttpRequest request)
        {
            var content = "## Error Accessing Path " + request.Path;

            string[] stringSeparators = new string[] { $"/{STATUS_ROUTE}/" };
            var requestPathSplited = request.Path.Split(stringSeparators, StringSplitOptions.None);

            if (requestPathSplited.Length == 2)
            {
                var relativeFilePath = requestPathSplited[1].Split('?')[0];
                var fileext = Path.GetExtension(relativeFilePath);

                var match = allowedType.ToList().FirstOrDefault(x => string.Equals(x, fileext, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(match))
                {
                    var filePath = Path.Combine(appLocation, relativeFilePath);
                    try
                    {
                        content = File.ReadAllText(filePath);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Read file [{0}] Error: {1}", filePath, e.ToString());
                    }
                }
                else
                {
                    content = "## Access Denied on Path " + request.Path;
                }
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
                    UrlRegex = @"^/"+STATUS_ROUTE+@"/.*",
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
